using AppContest.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppContest.Infrastructure
{
    public class DbContextTransactionPageFilter : IAsyncPageFilter
    {
        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

            try
            {
                await db.BeginTransactionAsync();

                var actionExecuted = await next();
                if (actionExecuted.Exception != null && !actionExecuted.ExceptionHandled)
                {
                    db.RollbackTransaction();
                }
                else
                {
                    await db.CommitTransactionAsync();
                }
            }
            catch (Exception)
            {
                db.RollbackTransaction();
                throw;
            }
        }
    }
}
