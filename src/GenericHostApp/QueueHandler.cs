using GenericHostApp.Model;
using Microsoft.Extensions.Logging;
using MvpPrototypes.RabbitMq;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericHostApp
{
    public class QueueHandler
    {
        private BasketContext _context;

        public QueueHandler(BasketContext context)
        {
            _context = context;
        }

        [RouteKey("PriceChange")]
        public void HandlePriceChanged(PriceChanged data)
        {
            foreach (var item in _context.BasketItems.Where(x => x.Id == data.ItemId))
            {
                item.Cost = data.NewPrice;
            }
            _context.SaveChanges();
        }
    }
}
