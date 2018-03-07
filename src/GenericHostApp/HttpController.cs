using GenericHostApp.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GenericHostApp
{
    [Route("/")]
    [ApiController]
    public class HttpController : ControllerBase
    {
        private BasketContext _context;

        public HttpController(BasketContext context)
        {
            _context = context;
        }

        [HttpGet("/")]
        public IEnumerable<Basket> Hello()
        {
            return _context.Baskets
                           .Include(basket => basket.Items)
                           .ToList();
        }

        [HttpGet("{customerId}")]
        public Basket Hello(int customerId)
        {
            return _context.Baskets
                           .Include(basket => basket.Items)
                           .FirstOrDefault(x => x.CustomerId == customerId);
        }
    }
}
