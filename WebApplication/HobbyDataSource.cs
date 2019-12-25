using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using NetCore.Web.AutoGenerateHtmlControl;


namespace WebApplication
{
    public class HobbyDataSource : IDataSource
    {
        public async Task<IEnumerable<SelectListItem>> GetAsync(IEnumerable<object> values)
        {
            return await Task.FromResult(new List<SelectListItem> {
                new SelectListItem("Tourism","Tourism",values.Contains("Tourism")),
                new SelectListItem("Run","Run",values.Contains("Run")),
                new SelectListItem("Draw","Draw",values.Contains("Draw"))
            });
        }
    }
}
