using System;
using System.Collections.Generic;
using System.Text;

namespace Auditor
{
    public class PriceChanged
    {
        public Guid CommandId { get; set; }
        public int ItemId { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
    }
}
