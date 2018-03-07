using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericHostApp.Model
{
    public class Basket
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public List<BasketItem> Items { get; set; }
    }
}
