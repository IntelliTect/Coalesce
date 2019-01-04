using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using IntelliTect.Coalesce.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace IntelliTect.Coalesce.Api.Controllers
{
    public class ApiActionFilter : IApiActionFilter
    {
        protected readonly ILogger<ApiActionFilter> logger;
        protected readonly IOptions<CoalesceOptions> options;

        public ApiActionFilter(ILogger<ApiActionFilter> logger, IOptions<CoalesceOptions> options)
        {
            this.logger = logger;
            this.options = options;
        }

        public virtual void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(v => v.Value.Errors.Any() && v.Key.StartsWith("dataSource", StringComparison.InvariantCultureIgnoreCase))
                    .SelectMany(v => v.Value.Errors.Select(e => (key: v.Key, error: e.ErrorMessage)))
                    .ToList();

                if (errors.Any())
                {
                    // TODO: this could be more robust.
                    // Lots of client methods in the typescript aren't expecting an object that looks like this.
                    // Anything that takes a SaveResult or ListResult should be fine, but other things (Csv..., Count) won't handle this.
                    context.Result = new BadRequestObjectResult(
                        new ApiResult(string.Join("; ", errors.Select(e => $"Invalid value for parameter {e.key}: {e.error}")))
                    );
                    return;
                }
            }
        }

        public virtual void OnActionExecuted(ActionExecutedContext context)
        {
            var response = context.HttpContext.Response;

            if (context.Exception != null && !context.ExceptionHandled)
            {
                logger?.LogError(context.Exception, context.Exception.Message);
                context.ExceptionHandled = true;
                response.StatusCode = (int)HttpStatusCode.InternalServerError;

                if (context.Result == null)
                {
                    var requestId = context.HttpContext.TraceIdentifier;
                    if (options.Value.DetailedExceptionMessages)
                    {
                        context.Result = new JsonResult(
                            new ApiResult($"{context.Exception.Message} (request {requestId})")
                        );
                    }
                    else
                    {
                        context.Result = new JsonResult(
                            new ApiResult($"An error occurred while processing request {requestId}.")
                        );
                    }
                }
            }

            if (response.StatusCode == (int)HttpStatusCode.OK
                && context.Result is ObjectResult result
                && result.Value is ApiResult apiResult
                && !apiResult.WasSuccessful
            )
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }
    }
}
