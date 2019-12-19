using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication.Controllers
{
    public class HomeController : Controller
    {
        [Authorize(AuthenticationSchemes = "Cookies")]
        public IActionResult Index()
        {
            return View();
        }
    }
}