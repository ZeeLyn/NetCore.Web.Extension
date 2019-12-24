using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using NetCore.Web.AutoGenerateHtmlControl;

namespace WebApplication
{
    public class ProfessionDataSource : ISelectDataSource
    {
        public async Task<IEnumerable<SelectListItem>> GetAsync(IEnumerable<object> values)
        {
            return await Task.FromResult(new List<SelectListItem> {
                new SelectListItem("Engineer","Engineer",values.Contains("Engineer"),true),
                new SelectListItem("Farmer","Farmer",values.Contains("Farmer"))
            });
        }
    }
}
