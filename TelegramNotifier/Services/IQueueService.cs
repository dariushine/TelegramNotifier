using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramNotifier.Entities;

namespace TelegramNotifier.Services
{
    public interface IQueueService
    {
         Task StartAsync(Func<Notification, Task> notify, CancellationToken stoppingToken);
    }
}