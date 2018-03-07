using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GenericHostApp.Http;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Libuv;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.IO;
using Microsoft.Extensions.FileProviders;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;

namespace GenericHostApp
{
    public class HttpHostedService : IHostedService
    {
        private IWebHost _host;

        public HttpHostedService(IWebHost host)
        {
            _host = host;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
           await  _host.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _host.StopAsync();
        }
    }

    public static class HostBuilderExtensions
    {
        public static IHostBuilder AddKestrelApp(this IHostBuilder builder, Action<IApplicationBuilder> appBuilder)
        {
            builder.ConfigureServices(services =>
            {
                var sp = services.BuildServiceProvider();

                var appLifetime = ActivatorUtilities.CreateInstance<ApplicationLifetime>(sp);
                services.AddSingleton<Microsoft.AspNetCore.Hosting.IApplicationLifetime>(appLifetime);
                services.AddSingleton<Microsoft.Extensions.Hosting.IApplicationLifetime>(appLifetime);

                var hostOptions = new WebHostOptions(sp.GetRequiredService<IConfiguration>(), Assembly.GetEntryAssembly()?.GetName().Name);

                var commonHosting = new HostingEnvironment();
                commonHosting.Initialize(sp.GetRequiredService<Microsoft.Extensions.Hosting.IHostingEnvironment>(), hostOptions);
                services.AddSingleton<Microsoft.AspNetCore.Hosting.IHostingEnvironment>(commonHosting);
                services.AddSingleton<Microsoft.Extensions.Hosting.IHostingEnvironment>(commonHosting);

                var listener = new DiagnosticListener("Microsoft.AspNetCore");
                services.AddSingleton<DiagnosticListener>(listener);
                services.AddSingleton<DiagnosticSource>(listener);

                services.AddTransient<IServiceProviderFactory<IServiceCollection>, DefaultServiceProviderFactory>();

                services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

                services.AddTransient<IApplicationBuilderFactory, ApplicationBuilderFactory>();
                services.AddTransient<IHttpContextFactory, HttpContextFactory>();
                services.AddScoped<IMiddlewareFactory, MiddlewareFactory>();
                services.AddTransient<IStartupFilter, AutoRequestServicesStartupFilter>();

                services.TryAddSingleton<ITransportFactory, LibuvTransportFactory>();

                services.AddTransient<IConfigureOptions<KestrelServerOptions>, KestrelServerOptionsSetup>();
                services.AddSingleton<IServer, KestrelServer>();

                services.AddSingleton(provider => ConstructWebHost(provider, hostOptions, appBuilder));
                services.AddSingleton<IHostedService, HttpHostedService>();
            });

            return builder;
        }

        private static IWebHost ConstructWebHost(IServiceProvider sp, WebHostOptions hostOptions, Action<IApplicationBuilder> appBuilder)
        {
            return ActivatorUtilities.CreateInstance(sp, typeof(SmallWebHost), hostOptions, appBuilder) as IWebHost;
        }
    }

    public static class HostingEnvironmentExtensions
    {
        public static Microsoft.AspNetCore.Hosting.IHostingEnvironment Initialize(this Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment, Microsoft.Extensions.Hosting.IHostingEnvironment baseEnv, WebHostOptions options)
        {
            hostingEnvironment.ApplicationName = options.ApplicationName;
            hostingEnvironment.ContentRootPath = baseEnv.ContentRootPath;
            hostingEnvironment.ContentRootFileProvider = new PhysicalFileProvider(hostingEnvironment.ContentRootPath);

            var webRoot = options.WebRoot;
            if (webRoot == null)
            {
                // Default to /wwwroot if it exists.
                var wwwroot = Path.Combine(hostingEnvironment.ContentRootPath, "wwwroot");
                if (Directory.Exists(wwwroot))
                {
                    hostingEnvironment.WebRootPath = wwwroot;
                }
            }
            else
            {
                hostingEnvironment.WebRootPath = Path.Combine(hostingEnvironment.ContentRootPath, webRoot);
            }

            if (!string.IsNullOrEmpty(hostingEnvironment.WebRootPath))
            {
                hostingEnvironment.WebRootPath = Path.GetFullPath(hostingEnvironment.WebRootPath);
                if (!Directory.Exists(hostingEnvironment.WebRootPath))
                {
                    Directory.CreateDirectory(hostingEnvironment.WebRootPath);
                }
                hostingEnvironment.WebRootFileProvider = new PhysicalFileProvider(hostingEnvironment.WebRootPath);
            }
            else
            {
                hostingEnvironment.WebRootFileProvider = new NullFileProvider();
            }

            hostingEnvironment.EnvironmentName =
                options.Environment ??
                hostingEnvironment.EnvironmentName;

            return hostingEnvironment;
        }
    }
}
