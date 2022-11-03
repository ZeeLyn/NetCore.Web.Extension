using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
            await this.SignInAsync(new Dictionary<string, string> { { "nick", form.NickName } }, TimeSpan.FromHours(2), "test1", "test2");
            //var claimId = new Claim("nick", "form.NickName");
            //var claimsIdentity = new ClaimsIdentity(new Claim[] { claimId }, "ShenFenZheng");
            //var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            //await ControllerContext.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, new AuthenticationProperties()
            //{
            //    AllowRefresh = true,
            //    IsPersistent = true,
            //});

            return Redirect("/");
        }
    }

    public class Login
    {
        [Required]
        public string NickName { get; set; }
    }
}