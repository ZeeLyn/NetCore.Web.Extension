using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        public async Task<IActionResult> Post([FromForm]Login form)
        {
            await this.SignInAsync(new Dictionary<string, string> { { "nick", form.NickName } }, TimeSpan.FromMinutes(10), "test1", "test2");
            return Redirect("/");
        }
    }

    public class Login
    {
        public string NickName { get; set; }
    }
}