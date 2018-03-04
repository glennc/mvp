using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Builder;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace GenericHostApp.Http
{
    public class SmallWebHost : IWebHost
    {
        private WebHostOptions _options;
        private ILogger<IWebHost> _logger;
        private Microsoft.Extensions.Hosting.Internal.ApplicationLifetime _applicationLifetime;
        private Action<IApplicationBuilder> _appFuncBuilder;
        private IConfiguration _config;
        private bool _stopped;
        private readonly AggregateException _hostingStartupErrors;

        internal WebHostOptions Options => _options;

        public IFeatureCollection ServerFeatures
        {
            get
            {
                EnsureServer();
                return Server?.Features;
            }
        }

        public IServiceProvider Services { get; private set; }

        public IServer Server { get; private set; }

        public SmallWebHost(IServiceProvider provider,
                            IConfiguration config,
                            Microsoft.Extensions.Hosting.IApplicationLifetime applicationLifetime,
                            ILogger<SmallWebHost> logger,
                            WebHostOptions options,
                            Action<IApplicationBuilder> appFuncBuilder)
        {
            Services = provider;
            _config = config;
            _options = options;
            _logger = logger;
            _applicationLifetime = applicationLifetime as Microsoft.Extensions.Hosting.Internal.ApplicationLifetime;
            _appFuncBuilder = appFuncBuilder;
        }

        private void EnsureServer()
        {
            if (Server == null)
            {
                Server = Services.GetRequiredService<IServer>();

                var serverAddressesFeature = Server.Features?.Get<IServerAddressesFeature>();
                var addresses = serverAddressesFeature?.Addresses;
                if (addresses != null && !addresses.IsReadOnly && addresses.Count == 0)
                {
                    var urls = _config[WebHostDefaults.ServerUrlsKey];
                    if (!string.IsNullOrEmpty(urls))
                    {
                        serverAddressesFeature.PreferHostingUrls = WebHostUtilities.ParseBool(_config, WebHostDefaults.PreferHostingUrlsKey);

                        foreach (var value in urls.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            addresses.Add(value);
                        }
                    }
                }
            }
        }

        public void Start()
        {
            StartAsync().GetAwaiter().GetResult();
        }

        public virtual async Task StartAsync(CancellationToken cancellationToken = default)
        {
            HostingEventSource.Log.HostStart();
            _logger.Starting();

            var application = BuildApplication();

            var diagnosticSource = Services.GetRequiredService<DiagnosticListener>();
            var httpContextFactory = Services.GetRequiredService<IHttpContextFactory>();
            var hostingApp = new HostingApplication(application, _logger, diagnosticSource, httpContextFactory);

            await Server.StartAsync(hostingApp, cancellationToken).ConfigureAwait(false);

            // Fire IApplicationLifetime.Started
            _applicationLifetime?.NotifyStarted();

            _logger.Started();

            // Log the fact that we did load hosting startup assemblies.
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                foreach (var assembly in _options.GetFinalHostingStartupAssemblies())
                {
                    _logger.LogDebug("Loaded hosting startup assembly {assemblyName}", assembly);
                }
            }

            if (_hostingStartupErrors != null)
            {
                foreach (var exception in _hostingStartupErrors.InnerExceptions)
                {
                    _logger.HostingStartupAssemblyError(exception);
                }
            }
        }

        private RequestDelegate BuildApplication()
        {
            try
            {
                EnsureServer();

                var builderFactory = Services.GetRequiredService<IApplicationBuilderFactory>();
                var builder = builderFactory.CreateBuilder(Server.Features);
                builder.ApplicationServices = Services;

                var startupFilters = Services.GetService<IEnumerable<IStartupFilter>>();
                Action<IApplicationBuilder> configure = _appFuncBuilder;
                foreach (var filter in startupFilters.Reverse())
                {
                    configure = filter.Configure(configure);
                }

                configure(builder);

                return builder.Build();
            }
            catch (Exception ex)
            {
                if (!_options.SuppressStatusMessages)
                {
                    // Write errors to standard out so they can be retrieved when not in development mode.
                    Console.WriteLine("Application startup exception: " + ex.ToString());
                }

                _logger.ApplicationError(ex);

                if (!_options.CaptureStartupErrors)
                {
                    throw;
                }

                EnsureServer();

                // Generate an HTML error page.
                var hostingEnv = Services.GetRequiredService<IHostingEnvironment>();
                var showDetailedErrors = hostingEnv.IsDevelopment() || _options.DetailedErrors;

                throw;
//TODO: all this stuff.
                //var model = new ErrorPageModel
                //{
                //    RuntimeDisplayName = RuntimeInformation.FrameworkDescription
                //};
                //var systemRuntimeAssembly = typeof(System.ComponentModel.DefaultValueAttribute).GetTypeInfo().Assembly;
                //var assemblyVersion = new AssemblyName(systemRuntimeAssembly.FullName).Version.ToString();
                //var clrVersion = assemblyVersion;
                //model.RuntimeArchitecture = RuntimeInformation.ProcessArchitecture.ToString();
                //var currentAssembly = typeof(ErrorPage).GetTypeInfo().Assembly;
                //model.CurrentAssemblyVesion = currentAssembly
                //    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                //    .InformationalVersion;
                //model.ClrVersion = clrVersion;
                //model.OperatingSystemDescription = RuntimeInformation.OSDescription;

                //if (showDetailedErrors)
                //{
                //    var exceptionDetailProvider = new ExceptionDetailsProvider(
                //        hostingEnv.ContentRootFileProvider,
                //        sourceCodeLineCount: 6);

                //    model.ErrorDetails = exceptionDetailProvider.GetDetails(ex);
                //}
                //else
                //{
                //    model.ErrorDetails = new ExceptionDetails[0];
                //}

                //var errorPage = new ErrorPage(model);
                //return context =>
                //{
                //    context.Response.StatusCode = 500;
                //    context.Response.Headers["Cache-Control"] = "no-cache";
                //    return errorPage.ExecuteAsync(context);
                //};
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (_stopped)
            {
                return;
            }
            _stopped = true;

            _logger?.Shutdown();

            var timeoutToken = new CancellationTokenSource(Options.ShutdownTimeout).Token;
            if (!cancellationToken.CanBeCanceled)
            {
                cancellationToken = timeoutToken;
            }
            else
            {
                cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken).Token;
            }

            // Fire IApplicationLifetime.Stopping
            _applicationLifetime?.StopApplication();

            if (Server != null)
            {
                await Server.StopAsync(cancellationToken).ConfigureAwait(false);
            }

            // Fire IApplicationLifetime.Stopped
            _applicationLifetime?.NotifyStopped();

            HostingEventSource.Log.HostStop();
        }

        public void Dispose()
        {
            if (!_stopped)
            {
                try
                {
                    StopAsync().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    _logger?.ServerShutdownException(ex);
                }
            }

            (Services as IDisposable)?.Dispose();
        }

    }
}
