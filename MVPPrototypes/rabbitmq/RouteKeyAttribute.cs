using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvpPrototypes.RabbitMq
{
    public class RouteKey : Attribute
    {
        public string Key { get; set; }

        public RouteKey(string key)
        {
            Key = key;
        }
    }
}
