using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MvpPrototypes.RabbitMq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvpPrototypes.WebHost
{
    public static class HostBuilderExtensions
    {
        public static IWebHostBuilder UseRabbitMq<T>(this IWebHostBuilder builder,
                                                     Action<RabbitMQOptions> options)
        {
            builder.ConfigureServices(services =>
            {
                services.Configure(options);
                services.AddSingleton<IHostedService, RabbitMQHostedService<T>>();
            });
            return builder;
        }

        //public static IWebHostBuilder UseRabbitMq<T>(this IWebHostBuilder builder,
        //                                     Action<RabbitMQOptions> options)
        //{
        //    builder.ConfigureServices(services =>
        //    {
        //        services.Configure(options);
        //        services.AddSingleton<IHostedService, RabbitMQHostedService<T>>();
        //    });
        //    return builder;
        //}

        public static IWebHostBuilder UseRabbitMq<T>(this IWebHostBuilder builder)
        {
            return builder;
        }
    }
}
