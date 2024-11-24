using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using TelegramNotifier.Entities;

namespace TelegramNotifier.Services
{
    public class BotService : BackgroundService
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ILogger<BotService> _logger;
        private readonly IQueueService _queueService;

        public BotService(ILogger<BotService> logger, ITelegramBotClient telegramBotClient, IQueueService queueService)
        {
            _telegramBotClient = telegramBotClient;
            _logger = logger;
            _queueService = queueService;
        }

        private async Task Notify(Notification notification)
        {
            try 
            {
                await _telegramBotClient.SendMessage(-4545875140, $"{notification.Type}: {notification.Content}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception during notification: {exception}", ex);
                throw;
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var tasks = new List<Task>
            {
                _queueService.StartAsync(Notify, stoppingToken),
            };

            return Task.WhenAll(tasks);
        }
    }
}