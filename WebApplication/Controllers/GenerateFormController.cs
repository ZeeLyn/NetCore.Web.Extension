using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NetCore.Web.AutoGenerateHtmlControl;
using NetCore.Web.AutoGenerateHtmlControl.Attributes;


namespace WebApplication.Controllers
{
    [Route("generateform")]
    public class GenerateFormController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View(new ArticleModel
            {
                Id = 1024,
                Title = "This is title",
                Summary = "This is summary",
                Profession = "Farmer",
                Birthday = new DateTime(1986, 10, 12),
                Gender = 2,
                FavoriteFood = new List<int> { 1, 3, 4 },
                Resume = "<h1>This is ckeditor5</h1>",
                Archive = true,
                LivePhoto = new List<string> { "/20201102/d6c5f9f4381248d5915389a7f5cc8978.jpg", "/20200117/2d180692d6be45afa8f5e528e836fee6.jpg" }
            });
        }

        [HttpPost("post")]
        public IActionResult Post([FromForm] ArticleModel form)
        {
            var state = ModelState;
            return View("index", form);
        }
    }

    public class ArticleModel
    {
        [Hide]
        [Hidden]
        public int Id { get; set; }
        [DisplayOrder(1)]
        [TextBox(Placeholder = "Please enter a title")]
        [Required(ErrorMessage = "Title is required")]
        [DisplayName("标题")]
        public string Title { get; set; }

        [DisplayOrder(2)]
        [TextArea(Placeholder = "Please enter a summary", Rows = 5)]
        public string Summary { get; set; }

        [DisplayOrder(3)]
        [Password(Placeholder = "Please enter a password")]
        [DisplayName("密码")]
        public string Password { get; set; }

        [DisplayOrder(4)]
        [DropDownList(DataSource = typeof(ProfessionDataSource), OptionLabel = "-----")]
        public string Profession { get; set; }

        [DisplayOrder(5)]
        [ListBox(DataSource = typeof(HobbyDataSource))]
        public List<string> Hobby { get; set; } = new List<string> { "Tourism", "Draw" };

        [DisplayOrder(6)]
        [TextBox(HtmlAttributes = "{readonly:'readonly'}")]
        [DateTimeConverter("yyyy-MM-dd")]
        public DateTime Birthday { get; set; }

        [DisplayOrder(7)]
        [RadioButton(DataSource = typeof(GenderDataSource), HtmlAttributes = "{class:''}")]
        public int Gender { get; set; }

        [DisplayOrder(8)]
        //[File(HtmlAttributes = "{class:'',style:'display:block'}")]
        [Required(ErrorMessage = "Avatar is required!")]
        [Uploader(PartialName = "CustomUploader")]
        [Button(ButtonText = "Upload", HtmlAttributes = "{class:'btn btn-primary btn-sm',onclick:'upload();'}")]
        public string Avatar { get; set; }


        [DisplayOrder(9)]
        [DisplayName("Favorite food")]
        [CheckBox(DataSource = typeof(FavoriteFoodDataSource), HtmlAttributes = "{class:''}")]
        public List<int> FavoriteFood { get; set; }


        [DisplayOrder(10)]
        [Uploader(Multiple = UploaderOptionEnum.True, FileNumLimit = 10)]
        public List<string> LivePhoto { get; set; }


        [DisplayOrder(11)]
        [Required(ErrorMessage = "Resume is required")]
        [RichEditor(PartialName = "CustomRichEditorPartial")]
        public string Resume { get; set; }

        [DisplayOrder(12)]
        [Required(ErrorMessage = "Resume2 is required")]
        [RichEditor]
        public string Resume2 { get; set; }


        [DisplayOrder(13)]
        [CheckBox(HtmlAttributes = "{class:''}")]
        public bool Archive { get; set; }
    }
}