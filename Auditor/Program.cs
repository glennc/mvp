using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MVPPrototypes.HostBuilder;
using System;
using System.Threading.Tasks;
using MvpPrototypes.RabbitMq;
using RabbitMQ.Client.Events;
using System.Text;

namespace Auditor
{
    public class Auditor
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                    .ConfigureLogging(l => l.AddConsole())
                    .UseRabbitMq<Auditor>(options =>
                    {
                        options.Exchange = "basket";
                        options.HostName = "localhost";
                    })
                    .Build();
            await host.RunAsync();
        }

        [RouteKey("PriceChange")]
        public void LogPriceChangeRequest(BasicDeliverEventArgs e, ILogger<Auditor> logger)
        {
            logger.LogInformation("Price change requested: {PriceChangedData}", Encoding.UTF8.GetString(e.Body));
        }
    }
}
