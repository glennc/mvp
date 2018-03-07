using GenericHostApp.Model;
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
        public void HandlePriceChanged(PriceChangedCommand Data)
        {
            foreach (var item in _context.BasketItems.Where(x => x.Id == Data.ItemId))
            {
                item.Cost = Data.NewPrice;
            }
            _context.SaveChanges();
        }
    }
}
