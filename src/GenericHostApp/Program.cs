using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GenericHostApp.Kafka;
using GenericHostApp.Model;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MvpPrototypes;

namespace GenericHostApp
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            await CreateWebHostBuilder(args)
                         .Build()
                         .RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
                  WebHost.CreateDefaultBuilder<Startup>(args)
                         .ConfigureServices((context, services) =>
                         {
                             services.AddDbContext<BasketContext>(b =>
                                  b.UseSqlite(context.Configuration["ConnectionString"]));
                         })
                         .UseRabbitMq<QueueHandler>(options =>
                         {
                             options.Exchange = "basket";
                             options.HostName = "localhost";
                         });
    }
}
