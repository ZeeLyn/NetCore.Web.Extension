using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NetCore.Web.Extension
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private ILogger Logger { get; }

        private GlobalExceptionOptions Options { get; }
        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger, IOptions<GlobalExceptionOptions> options)
        {
            Logger = logger;
            Options = options.Value;
        }
        public void OnException(ExceptionContext context)
        {
            if (Options.Action != null)
                Options.Action(context, Logger);
            else
            {
                Logger.LogError(context.Exception, "Unhandled exception");
                context.Result = new BadRequestObjectResult(context.Exception.Message);
            }
        }
    }
}
