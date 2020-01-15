using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using NetCore.Web.AutoGenerateHtmlControl;

namespace WebApplication
{
    public class FavoriteFoodDataSource : IDataSource
    {
        public async Task<IEnumerable<SelectListItem>> GetAsync(IEnumerable<object> values)
        {
            return await Task.FromResult(new List<SelectListItem>
            {
                new SelectListItem("Potato", "Potato", values?.Contains("Potato")??false),
                new SelectListItem("Tomato", "Tomato", values?.Contains("Tomato")??false),
                new SelectListItem("Onion", "Onion", values?.Contains("Onion")??false),
                new SelectListItem("Carrot", "Carrot", values?.Contains("Carrot")??false),
                new SelectListItem("Eggplant", "Eggplant", values?.Contains("Eggplant")??false)
            });
        }
    }
}
