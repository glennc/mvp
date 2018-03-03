using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GenericHostApp.Kafka;
using Microsoft.AspNetCore.Hosting;
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
                    .ConfigureServices(ConfigureServices);

            await hostBuilder.Build().RunAsync();
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddSingleton<IHostedService, KafkaHostedService>();
        }
    }
}
