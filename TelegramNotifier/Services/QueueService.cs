using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TelegramNotifier.Entities;

namespace TelegramNotifier.Services
{
    public class QueueService : IQueueService
    {
        private readonly ILogger<QueueService> _logger;
        public QueueService(ILogger<QueueService> logger)
        {
            _logger = logger;
        }

        public async Task StartAsync(Func<Notification, Task> notify, CancellationToken stoppingToken)
        {

            var factory = new ConnectionFactory { HostName = "localhost" };
            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(exchange: "notifications",
                type: ExchangeType.Direct, durable: true);

            // declare a server-named queue
            QueueDeclareOk queueDeclareResult = await channel.QueueDeclareAsync();
            string queueName = queueDeclareResult.QueueName;
            await channel.QueueBindAsync(queue: queueName, exchange: "notifications", routingKey: "key");

            _logger.LogInformation("QueueService: Ready.");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                byte[] body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                try
                {
                    Notification? notification = JsonSerializer.Deserialize<Notification>(message);
                    if (notification != null) 
                        await notify(notification);
                    else 
                    {
                        _logger.LogError("Message conversion returned null: {message}", message);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError("Message failed to parse: {message}\nException: {exception}", message, ex);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Message failed to notify: {message}\nException: {exception}", message, ex);
                    // TODO: Add requeue logic
                    throw;
                }
            };

            await channel.BasicConsumeAsync(queueName, autoAck: true, consumer: consumer, cancellationToken: stoppingToken);
        }

    }
}