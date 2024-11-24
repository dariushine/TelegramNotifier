using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramNotifier.Entities
{
    public class Notification
    {
        public required string Type { get; set; }
        public required string Content { get; set; }
    }
}