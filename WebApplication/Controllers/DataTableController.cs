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
                    CreatedOn = DateTime.Now,
                    Published=true
                }
            });
        }
    }

    [DataListView(HtmlAttribute = "{id:'table1'}")]
    public class DataTable
    {
        [DataListItemView(Format = "{Id}:{Title}")]
        [DisplayName("编号")]
        public int Id { get; set; }

        [DataListItemView(HeaderHtmlAttribute = "{class:'a'}")]
        [DisplayName("标题")]
        public string Title { get; set; }

        [DataListItemView(ValueMap = "{True:'<label>√</label>',False:'<label>×</label>'}")]
        [DisplayName("发布")]
        public bool Published { get; set; }

        [DataListItemView]
        [DisplayName("添加时间")]
        public DateTime CreatedOn { get; set; }
    }
}