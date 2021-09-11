using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace BK.DotNet.Template.Helpers
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
