using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GenericHostApp.Kafka
{
    public class KafkaHostedService : IHostedService
    {
        private Consumer<Ignore, string> _consumer;
        private Task _runningTask;
        private string _topics = "test";
        private CancellationTokenSource _runToken;
        private ILogger _logger;

        public KafkaHostedService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(_topics);
            _runToken = new CancellationTokenSource();

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            string brokerList = "localhost";

            var config = new Dictionary<string, object>
            {
                { "group.id", "simple-consumer" },
                { "bootstrap.servers", brokerList }
            };

            _consumer = new Consumer<Ignore, string>(config, null, new StringDeserializer(Encoding.UTF8));

            //_consumer.Assign(new List<TopicPartition> { new TopicPartition(_topics, 0) });
            _consumer.Subscribe(_topics);

            _consumer.OnError += (_, error)
                => _logger.LogError("Error: {error}", error);

            _consumer.OnConsumeError += (_, error)
                => _logger.LogError("Consume error: {error}", error);

            _consumer.OnMessage += ConsumeMessage;

            _runningTask = Task.Run(() => Run(_runToken.Token), _runToken.Token);
            return Task.CompletedTask;
        }

        private void ConsumeMessage(object sender, Message<Ignore, string> e)
        {
            _logger.LogInformation($"Topic: {e.Topic} Partition: {e.Partition} Offset: {e.Offset} {e.Value}");
        }

        private async Task Run(CancellationToken token)
        {
            while(!token.IsCancellationRequested)
            {
                _consumer.Poll(3000);
            }
            await _consumer.CommitAsync();
            _consumer.Dispose();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _runToken.Cancel();
            _runningTask.Wait(10000);

            if(!_runningTask.IsCompletedSuccessfully)
            {
                _logger.LogError("Unable to shutdown consumer task in a timely manner.");
            }

            return Task.CompletedTask;
        }
    }

    public static class KafkaExtensions
    {
        public static IHostBuilder AddKafka(this IHostBuilder builder)
        {
            builder.ConfigureServices(services => services.AddSingleton<IHostedService, KafkaHostedService>());
            return builder;
        }
    }
}
