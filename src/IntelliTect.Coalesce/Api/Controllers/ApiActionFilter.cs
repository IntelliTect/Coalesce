using System;
using System.Linq;
using IntelliTect.Coalesce.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

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
                    .Where(v => v.Value?.Errors.Any() == true && v.Key.StartsWith("dataSource", StringComparison.InvariantCultureIgnoreCase))
                    .SelectMany(v => v.Value!.Errors.Select(e => (key: v.Key, error: e.ErrorMessage)))
                    .ToList();

                if (errors.Any())
                {
                    // Lots of client methods in the knockout typescript aren't expecting an object that looks like this.
                    // Anything that takes a SaveResult or ListResult should be fine, but other things (Count) won't handle this.
                    // The Vue typescript should handle this just fine.
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
                if (context.Exception is OperationCanceledException
                    && context.HttpContext.RequestAborted.IsCancellationRequested)
                {
                    // Request was aborted by client. We don't want to log this.
                    // The response we send back also doesn't matter because
                    // the client will never see it.
                    context.ExceptionHandled = true;
                    // 499 is a non-standard code "Client Closed Request" introduced by nginx .
                    context.Result = new StatusCodeResult(499);
                    return;
                }

                logger?.LogError(context.Exception, context.Exception.Message);
                context.ExceptionHandled = true;
                response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var customResult = options.Value.ExceptionResponseFactory?.Invoke(context);
                if (customResult != null)
                {
                    context.Result = new JsonResult(customResult);
                }
                else if (context.Result == null)
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
                result.StatusCode = 
                response.StatusCode = 
                    (int)HttpStatusCode.BadRequest;
            }
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            int statusCode;
            ApiResult result;

            switch (context.Result)
            {
                case ChallengeResult _:
                    // ChallengeResult is the authentication flow response for a 401
                    // when Identity is setup such that it redirects the user
                    // to a login page to get them to log in.

                    // Redirecting to a login page makes no sense for an API call,
                    // so we intercept to return an ApiResult so that Coalesce can understand it.

                    // An IAlwaysRunResultFilter is the only way we can intercept this response
                    // and turn it into a ApiResult.

                    statusCode = StatusCodes.Status401Unauthorized;
                    result = new ApiResult("Unauthorized.");
                    break;

                case ForbidResult _:
                    // ForbidResult is the authentication flow response for a 403.
                    // This happens when you return Forbid() in a controller, 
                    // or when authorization fails via the [Authorize] attribute.

                    // An IAlwaysRunResultFilter is the only way we can intercept this response
                    // and turn it into a ApiResult.

                    statusCode = StatusCodes.Status403Forbidden;
                    result = new ApiResult("Access Denied.");
                    break;

                default:
                    return;
            }

            context.Result = new JsonResult(result)
            {
                StatusCode = statusCode,
            };
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
            // not needed
        }
    }
}
