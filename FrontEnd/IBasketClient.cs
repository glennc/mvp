using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrontEnd
{
    public interface IBasketClient
    {
        [Get("/{customerId}")]
        Task<string> GetBasketForCustomer(int customerId);
    }
}
