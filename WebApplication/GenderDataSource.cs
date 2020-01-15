using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using NetCore.Web.AutoGenerateHtmlControl;

namespace WebApplication
{
    public class GenderDataSource : IDataSource
    {
        public async Task<IEnumerable<SelectListItem>> GetAsync(IEnumerable<object> values)
        {
            return await Task.FromResult(new List<SelectListItem>
            {
                new SelectListItem("Male", "1", values?.Contains(1)??false),
                new SelectListItem("Female", "2", values?.Contains(2)??false)
            });
        }
    }
}
