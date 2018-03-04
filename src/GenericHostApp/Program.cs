using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GenericHostApp.Kafka;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GenericHostApp
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var hostBuilder = new HostBuilder()
                    .ConfigureLogging(loggerFactory => loggerFactory.AddConsole())
                    .AddKestrelApp(app => app.UseMvc())
                    .ConfigureServices(services => services.AddMvcCore()
                                                           .AddJsonFormatters()
                                                           .SetCompatibilityVersion(CompatibilityVersion.Version_2_1))
                    .AddKafka();

            await hostBuilder.RunConsoleAsync();
        }
    }
}
