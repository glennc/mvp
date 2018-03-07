using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericHostApp
{
    public class PriceChanged
    {
        public Guid CommandId { get; set; }
        public int ItemId { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
    }
}
