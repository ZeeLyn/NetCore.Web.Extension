using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetCore.Web.Extension;

namespace WebApplication.Controllers
{
    public class PagingController : Controller
    {
        public IActionResult Index([FromForm]Search search)
        {
            return View(search);
        }

    }
    public class Search
    {
        public string Title { get; set; }
    }
}