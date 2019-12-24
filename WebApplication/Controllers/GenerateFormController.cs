using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NetCore.Web.AutoGenerateHtmlControl.Attributes;


namespace WebApplication.Controllers
{
    public class GenerateFormController : Controller
    {
        public IActionResult Index()
        {
            return View(new ArticleModel
            {
                Id = 1024,
                Title = "This is title",
                Summary = "This is summary",
                Profession = "Farmer",
                Birthday = new DateTime(1986, 10, 12)
            });
        }
    }

    public class ArticleModel
    {

        [Label]
        [Hidden]
        public int Id { get; set; }

        [TextBox(Placeholder = "Please enter a title")]
        [Button(ButtonText = "点击")]
        [Required]
        [DisplayName("标题")]
        public string Title { get; set; }

        [TextArea(Placeholder = "Please enter a summary", Rows = 5)]
        public string Summary { get; set; }

        [Password(Placeholder = "Please enter a password")]
        [DisplayName("密码")]
        public string Password { get; set; }

        [DropDownList(DataSource = typeof(ProfessionDataSource), OptionLabel = "-----")]
        public string Profession { get; set; }

        [ListBox(DataSource = typeof(HobbyDataSource))]
        public List<string> Hobby { get; set; } = new List<string> { "Tourism", "Draw" };

        [TextBox(HtmlAttributes = "{readonly:'readonly'}")]
        public DateTime Birthday { get; set; }
    }
}