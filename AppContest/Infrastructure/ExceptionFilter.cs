using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace AppContest.Infrastructure
{
    public class ExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.HttpContext.Request.Method == "POST")
            {
                var result = new ContentResult();
                var content = JsonConvert.SerializeObject(context.Exception,
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                result.Content = content;
                result.ContentType = "application/json";

                context.HttpContext.Response.StatusCode = 500;
                context.Result = result;
            }

        }
    }
}
