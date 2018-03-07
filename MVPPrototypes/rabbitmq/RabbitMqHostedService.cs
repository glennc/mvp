using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

namespace MvpPrototypes.RabbitMq
{
    public class RabbitMQHostedService<T> : IHostedService
    {
        private EventingBasicConsumer _consumer;
        private IConnection _connection;
        private IModel _channel;
        private ILogger _logger;
        private IServiceProvider _sp;
        private RabbitMQOptions _options;

        Dictionary<string, MethodInfo> _routes;

        public RabbitMQHostedService(ILoggerFactory loggerFactory, 
                                     IOptions<RabbitMQOptions> options,
                                     IServiceProvider sp)
        {
            _options = options.Value;
            _logger = loggerFactory.CreateLogger($"Exchange<{_options.Exchange}>");
            _sp = sp;
            _routes = new Dictionary<string, MethodInfo>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory() { HostName = _options.HostName };
            if (_options.HostPort.HasValue)
            {
                factory.Port = _options.HostPort.Value;
            }
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: _options.Exchange,
                                  durable: true,
                                  autoDelete: false,
                                  type: "topic");

            var queueName = _channel.QueueDeclare().QueueName;

            var routeKeyMethods = typeof(T).GetMethods().Where(x => x.GetCustomAttributes<RouteKey>().Any());

            foreach (var method in routeKeyMethods)
            {
                var keys = (RouteKey[])method.GetCustomAttributes<RouteKey>();
                foreach (var key in keys)
                {
                    _channel.QueueBind(queueName, _options.Exchange, key.Key);
                    _routes.Add(key.Key, method);
                }
            }

            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += Task_Recieved;

            _channel.BasicConsume(queue: queueName,
                                  autoAck: false,
                                  consumer: _consumer);

            _logger.LogInformation("Ready to consume messages from {queueName}", queueName);

            return Task.CompletedTask;
        }

        private void Task_Recieved(object sender, BasicDeliverEventArgs e)
        {
            _logger.LogTrace("Recieved message from " + _options.QueueName);
            if (!_routes.ContainsKey(e.RoutingKey))
            {
                _logger.LogInformation("No route configured for {RoutintKey}", e.RoutingKey);
                return;
            }

            var handler = _routes[e.RoutingKey];


            using (var scope = _sp.CreateScope())
            {
                try
                {
                    var handlerType = ActivatorUtilities.GetServiceOrCreateInstance<T>(scope.ServiceProvider);

                    var parameters = handler.GetParameters();
                    var handlerArgs = new object[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i].ParameterType == typeof(BasicDeliverEventArgs))
                        {
                            handlerArgs[i] = e;
                        }
                        else
                        {
                            var service = scope.ServiceProvider.GetService(parameters[i].ParameterType);
                            if (service != null)
                            {
                                handlerArgs[i] = service;
                            }
                            else
                            {
                                var data = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(e.Body), parameters[i].ParameterType);
                                handlerArgs[i] = data;
                            }
                        }
                    }
                    handler.Invoke(handlerType, handlerArgs);
                    _channel.BasicAck(e.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error occured handling a message: {ex}", ex);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _connection.Close();
            }
            finally
            {
                _channel.Dispose();
                _connection.Dispose();
            }
            return Task.CompletedTask;
        }
    }
}
