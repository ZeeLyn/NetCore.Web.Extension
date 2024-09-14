using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetCore.Web.Extension;

namespace WebApplication.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var context = this.ControllerContext.HttpContext.User;
            Console.WriteLine(User.Identity.IsAuthenticated.ToString());
            ViewBag.Nick = User.GetClaimValue("nick");
            return View();
        }
    }
}