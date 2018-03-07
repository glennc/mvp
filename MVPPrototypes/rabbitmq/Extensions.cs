using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MvpPrototypes.RabbitMq;
using System;
using System.Collections.Generic;
using System.Text;

namespace MVPPrototypes.HostBuilder
{
    public static class Extensions
    {
        public static IHostBuilder UseRabbitMq<T>(this IHostBuilder builder,
                             Action<RabbitMQOptions> options)
        {
            builder.ConfigureServices(services =>
            {
                services.Configure(options);
                services.AddSingleton<IHostedService, RabbitMQHostedService<T>>();
            });
            return builder;
        }
    }
}
