using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using NetCore.Web.Extension;

namespace WebApplication.Controllers
{
    [Route("login")]
    public class LoginController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("post")]
        public async Task<IActionResult> Post()
        {
            await this.SignInAsync(new Dictionary<string, string> { { "issuer", "issuer" } }, TimeSpan.FromMinutes(10), "issuer", "audience");
            return View("Index");
        }
    }
}