using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvpPrototypes.RabbitMq
{
    public class RabbitMQOptions
    {
        public string HostName { get; set; }
        public int? HostPort { get; set; }
        public string QueueName { get; set; }
        public string Exchange { get; set; }
    }
}
