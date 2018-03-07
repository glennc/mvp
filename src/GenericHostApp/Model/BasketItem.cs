using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericHostApp.Model
{
    public class BasketItem
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
    }
}
