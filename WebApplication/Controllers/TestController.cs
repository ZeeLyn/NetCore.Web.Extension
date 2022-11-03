using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace WebApplication.Controllers
{
    [Route("api/test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpPost("dateonly")]
        public async Task<IActionResult> Post([FromBody] TestClass data)
        {
            return Ok(data);
        }
    }


    public class TestClass
    {
        public DateOnly Date { get; set; }

        public TimeOnly Time { get; set; }
    }
}
