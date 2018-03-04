using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericHostApp
{
    [Route("/")]
    [ApiController]
    public class HttpController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<string> Hello()
        {
            return new string[] { "hello", "hello", "hello" };
        }
    }
}
