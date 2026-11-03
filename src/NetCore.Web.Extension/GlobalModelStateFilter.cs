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
            if (context.ModelState.IsValid) return;
            if (Options.Action != null)
                Options.Action(context);
            else
            {
                var errorMessage = context.ModelState.Values.SelectMany(value => value.Errors)
                    .Select(error => error.ErrorMessage).LastOrDefault(message => !string.IsNullOrWhiteSpace(message))
                    ?? "The request contains invalid data.";
                context.Result = new BadRequestObjectResult(errorMessage);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {

        }
    }
}
