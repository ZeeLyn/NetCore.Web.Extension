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
            return View(new List<DataTableViewModel>
            {
                new DataTableViewModel
                {
                    Id = 1,
                    Title = "This is title",
                    CreatedOn = DateTime.Now,
                    Published=true,
                    Banner= "/b1110d9d193b4fd68b3a0c164688ba12.jpg"
                }
            });
        }
    }

    [DataList(HtmlAttribute = "{id:'table1'}")]
    public class DataTableViewModel
    {
        [DataListColumn]
        [DisplayName("编号")]
        public int Id { get; set; }

        [DataListColumn(HeaderHtmlAttribute = "{class:'a'}")]
        [DisplayName("标题")]
        public string Title { get; set; }

        [DataListColumn(Format = "<img src=\"{Banner}\" style=\"width:100px; height:60px;\" />")]
        public string Banner { get; set; }

        [DataListColumn(ValueMap = "{true:'<label>√</label>',false:'<label>×</label>'}")]
        [DisplayName("发布")]
        public bool Published { get; set; }

        [DataListColumn]
        [DisplayName("添加时间")]
        [DateTimeConverter("yyyy-MM-dd HH:mm:ss")]
        public DateTime CreatedOn { get; set; }
    }
}