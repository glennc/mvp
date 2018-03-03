using Confluent.Kafka;
using Confluent.Kafka.Serialization;
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
                { "group.id", "simple-csharp-consumer" },
                { "bootstrap.servers", brokerList }
            };

            _consumer = new Consumer<Ignore, string>(config, null, new StringDeserializer(Encoding.UTF8));

            _consumer.Assign(new List<TopicPartitionOffset> { new TopicPartitionOffset(_topics, 0, 0) });

            _consumer.OnError += (_, error)
                => _logger.LogError("Error: {error}", error);

            _consumer.OnConsumeError += (_, error)
                => _logger.LogError("Consume error: {error}", error);

            _runningTask = Task.Run(() => Run(_runToken.Token), _runToken.Token);
            return Task.CompletedTask;
        }

        private async Task Run(CancellationToken token)
        {
            while(!token.IsCancellationRequested)
            {
                if (_consumer.Consume(out var msg, TimeSpan.FromSeconds(1)))
                {
                    _logger.LogInformation($"Topic: {msg.Topic} Partition: {msg.Partition} Offset: {msg.Offset} {msg.Value}");
                    await _consumer.CommitAsync(msg);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _runToken.Cancel();
            _runningTask.Wait(10000);

            if(!_runningTask.IsCompletedSuccessfully)
            {
                _logger.LogError("Unable to shutdown consumer task in a timely manner.");
            }

            _consumer.Dispose();

            return Task.CompletedTask;
        }
    }
}
