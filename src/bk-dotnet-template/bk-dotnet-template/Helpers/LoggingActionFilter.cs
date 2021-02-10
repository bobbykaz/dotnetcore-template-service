using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace bk_dotnet_template.Helpers
{
    public class LoggingActionFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {

        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            Log.Logger.Information($"Request: {context.HttpContext.Request.Method} ; {context.HttpContext.Request.Path} ; Query: {context.HttpContext.Request.QueryString};");
        }
    }
}
