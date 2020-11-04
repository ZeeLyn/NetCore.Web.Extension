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
                new SelectListItem("Potato", "1", values?.Contains(1) ?? false),
                new SelectListItem("Tomato", "2", values?.Contains(2) ?? false),
                new SelectListItem("Onion", "3", values?.Contains(3) ?? false),
                new SelectListItem("Carrot", "4", values?.Contains(4) ?? false),
                new SelectListItem("Eggplant", "5", values?.Contains(5) ?? false)
            });
        }
    }
}
