using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrontEnd
{
    public class PriceChangedCommand
    {
        public PriceChangedCommand()
        {
            CommandId = Guid.NewGuid();
        }

        public Guid CommandId { get; set; }
        public int ItemId { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
    }
}
