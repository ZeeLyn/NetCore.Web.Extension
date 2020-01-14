using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetCore.Web.AutoGenerateHtmlControl;
using NetCore.Web.AutoGenerateHtmlControl.Attributes;

namespace WebApplication.Controllers
{
    public class CardController : Controller
    {
        public IActionResult Index()
        {
            return View(new List<DataCardViewModel>
            {
                new DataCardViewModel
                {
                    Header="文章",
                    Author="Jack",
                    Tags="美食,时尚",
                    Title = "Card title",
                    Banner="/b1110d9d193b4fd68b3a0c164688ba12.jpg",
                    Summary =
                        "Some quick example text to build on the card title and make up the bulk of the card's content.",
                    CreatedOn = DateTime.Now,
                }
            });
        }
    }

    [DataList(HtmlAttribute = "{id:'table1'}")]
    public class DataCardViewModel
    {
        [DataListColumn(CardContentContainer = CardContentContainer.Header)]
        public string Header { get; set; }

        [DataListColumn(Format = "<img src=\"{Banner}\" class=\"card-img-top\" />", CardContentContainer = CardContentContainer.Root)]
        public string Banner { get; set; }

        [DataListColumn(ContentHtmlAttribute = "{class:'card-title'}", Format = "<h3>{Title}</h3>")]
        [DisplayName("标题")]
        public string Title { get; set; }

        [DataListColumn]
        [DisplayName("描述")]
        public string Summary { get; set; }

        [DataListColumn(CardContentContainer = CardContentContainer.ListGroup)]
        [DisplayName("描述")]
        public string Author { get; set; }

        [DataListColumn(CardContentContainer = CardContentContainer.ListGroup)]
        [DisplayName("标签")]
        public string Tags { get; set; }

        [DataListColumn(CardContentContainer = CardContentContainer.Footer, Format = "创建时间：{CreatedOn}")]
        [DisplayName("添加时间")]
        public DateTime CreatedOn { get; set; }
    }
}