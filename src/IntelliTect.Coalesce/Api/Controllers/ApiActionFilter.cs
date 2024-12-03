using IntelliTect.Coalesce.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IntelliTect.Coalesce.Api.Controllers
{
    public class ApiActionFilter : IApiActionFilter
    {
        protected readonly ILogger<ApiActionFilter> logger;
        protected readonly IOptions<CoalesceOptions> options;

        private static readonly MediaTypeHeaderValue RefTypeHeader = new MediaTypeHeaderValue("application/json+ref");

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
                    .SelectMany(v => v.Value!.Errors.Select(e => (key: v.Key, error: e.ErrorMessage)))
                    .ToList();

                if (errors.Any())
                {
                    context.Result = new BadRequestObjectResult(
                        new ApiResult(string.Join(" \n", errors.Select(e => string.IsNullOrWhiteSpace(e.key) ? e.error : $"{e.key}: {e.error}")))
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
                    
                    context.Result = new StatusCodeResult(StatusCodes.Status499ClientClosedRequest);
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
                        var messages = new StringBuilder();
                        Exception? currentEx = context.Exception;
                        while (currentEx is not null)
                        {
                            messages.AppendLine(currentEx.Message);
                            currentEx = currentEx.InnerException;
                        }
                        string message = messages.ToString();

                        if (
                            options.Value.DetailedEfMigrationExceptionMessages &&
                            (context.Exception as DbException ?? context.Exception?.InnerException) is DbException
                        )
                        {
                            var dbMessage = GetDbContextMigrationExceptionMessage(context);
                            if (!string.IsNullOrWhiteSpace(dbMessage))
                            {
                                message = dbMessage + "\n\n" + message;
                            }
                        }

                        context.Result = new JsonResult(
                            new ApiResult($"{message} (request {requestId})")
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

            if (context.Result is ObjectResult result)
            {
                if (context.HttpContext.Request.GetTypedHeaders().Accept.Any(h => h.IsSubsetOf(RefTypeHeader)))
                {
                    var jsonOptions = context.HttpContext.RequestServices.GetService<IOptions<JsonOptions>>()?.Value ?? new JsonOptions
                    {
                        JsonSerializerOptions =
                        {
                            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        }
                    };
                    var newOptions = new JsonSerializerOptions(jsonOptions.JsonSerializerOptions)
                    {
                        ReferenceHandler = new CoalesceJsonReferenceHandler()
                    };
                    result.Formatters.Add(new SystemTextJsonOutputFormatter(newOptions));
                }

                if (response.StatusCode == (int)HttpStatusCode.OK && 
                    result.Value is ApiResult apiResult && 
                    !apiResult.WasSuccessful)
                {
                    result.StatusCode =
                    response.StatusCode =
                        (int)HttpStatusCode.BadRequest;
                }
            }
        }

        private static string GetDbContextMigrationExceptionMessage(ActionExecutedContext context)
        {
            List<string> messages = [];
            try
            {
                var registeredContexts = context.HttpContext.RequestServices
                    .GetServices<DbContextOptions>()
                    .Select(o => o.ContextType)
                    .Distinct();

                foreach (var dbcontextType in registeredContexts)
                {
                    if (context.HttpContext.RequestServices.GetService(dbcontextType) is not DbContext db) continue;

                    if (db.GetService<IDatabaseCreator>() is not IRelationalDatabaseCreator relationalDatabaseCreator) continue;
                    var databaseExists = relationalDatabaseCreator.Exists();
                    if (!databaseExists) continue;

                    var migrationsAssembly = db.GetService<IMigrationsAssembly>();
                    var historyRepository = db.GetService<IHistoryRepository>();
                    if (!historyRepository.Exists())
                    {
                        // App is not using migrations.
                        continue;
                    }
                    
                    var dbMessage = "";
                    const string fragmentAddMigration = "Add a migration by running `dotnet ef migrations add <migrationName>` in your Data project.";

                    var pendingModelChanges = db.Database.HasPendingModelChanges();
                    if (!pendingModelChanges && migrationsAssembly?.ModelSnapshot is null)
                    {
                        dbMessage = $"{dbcontextType.Name} has been migrated, but no migrations exist. " + fragmentAddMigration;
                    }
                    else if (pendingModelChanges)
                    {
                        dbMessage = $"{dbcontextType.Name} has model changes that have not been captured by a migration. " + fragmentAddMigration;
                    }
                    else if (db.Database.GetPendingMigrations().Any())
                    {
                        dbMessage = $"{dbcontextType.Name} has unapplied migrations. They may have an error, or may have not been attempted.";
                    }
                    if (dbMessage is "") continue;

                    messages.Add(dbMessage);
                }
            }
            catch { }

            if (messages.Count == 0) return "";

            return string.Join(" ", messages) + " This is the likely cause of the following error:";
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
