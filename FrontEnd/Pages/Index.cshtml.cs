using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace FrontEnd.Pages
{
    public class IndexModel : PageModel
    {
        public string BasketContents { get; set; }

        [BindProperty]
        public string NewPrice { get; set; }

        public IndexModel()
        {
        }

        public async Task OnGet([FromServices]IBasketClient client)
        {
            try
            {
                BasketContents = await client.GetBasketForCustomer(1);
            }
            catch(Exception)
            {
                BasketContents = "Basket Service Unavailable";
            }
        }

        public IActionResult OnPost()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "basket",
                                  durable: true,
                                  autoDelete: false,
                                  type: "topic");

                var props = channel.CreateBasicProperties();
                props.Persistent = true;

                var command = new PriceChangedCommand
                {
                    ItemId = 1,
                    OldPrice = 5,
                    NewPrice = Decimal.Parse(NewPrice)
                };

                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(command));

                channel.BasicPublish(exchange: "basket",
                                     routingKey: "PriceChange",
                                     basicProperties: props,
                                     body: body);
            }

            return RedirectToPage();
        }
    }
}
