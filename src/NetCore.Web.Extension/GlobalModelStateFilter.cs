using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace NetCore.Web.Extension
{
    public class GlobalModelStateFilter : IActionFilter
    {
        private GlobalModelStateOptions Options { get; }
        public GlobalModelStateFilter(IOptions<GlobalModelStateOptions> options)
        {
            Options = options.Value;
        }
        public void OnActionExecuting(ActionExecutingContext context)
        {

        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.ModelState.IsValid) return;
            if (Options.Action != null)
                Options.Action(context);
            else
            {
                var firstError = context.ModelState.Keys.SelectMany(k => context.ModelState[k].Errors).Select(e => e.ErrorMessage).LastOrDefault();
                context.Result = new BadRequestObjectResult(firstError);
            }
        }
    }
}
