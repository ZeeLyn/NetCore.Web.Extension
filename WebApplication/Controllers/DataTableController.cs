using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetCore.Web.AutoGenerateHtmlControl.Attributes;

namespace WebApplication.Controllers
{
    public class DataTableController : Controller
    {
        public IActionResult Index()
        {
            return View(new List<DataTable>
            {
                new DataTable
                {
                    Id = 1,
                    Title = "This is title",
                    CreatedOn = DateTime.Now
                }
            });
        }
    }

    public class DataTable
    {
        [DataTable(Format = "{Id}:{Title}")]
        [DisplayName("编号")]
        public int Id { get; set; }

        [DataTable]
        [DisplayName("标题")]
        public string Title { get; set; }

        [DataTable]
        [DisplayName("添加时间")]
        public DateTime CreatedOn { get; set; }
    }
}