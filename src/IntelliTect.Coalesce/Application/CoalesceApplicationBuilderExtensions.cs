using IntelliTect.Coalesce.Api.Behaviors;
using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce;

public static partial class CoalesceApplicationBuilderExtensions
{
    /// <summary>
    /// Add static file middleware, and configure it to place a long client cache duration (30 days)
    /// on any files that contain a cache-busting hash as produced by Vite's build output.
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <param name="options">Options for configuring the Vite static file middleware</param>
    public static IApplicationBuilder UseViteStaticFiles(this IApplicationBuilder app, ViteStaticFilesOptions? options = null)
    {
        return app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponseAsync = async ctx =>
            {
                // Check if unauthorized response should be returned
                if (options?.OnAuthorizeAsync is not null && !await options.OnAuthorizeAsync(ctx))
                {
                    ctx.Context.Response.Clear();
                    ctx.Context.Response.Body = new MemoryStream();
                    ctx.Context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                if (ContainsFileHashRegex().IsMatch(ctx.File.Name))
                {
                    ctx.Context.Response.Headers.CacheControl = "max-age=2592000, public";
                }

                // Call the passthrough OnPrepareResponse hook
                if (options?.OnPrepareResponseAsync is not null)
                {
                    await options.OnPrepareResponseAsync(ctx);
                }
            }
        });
    }

    // vite puts 8-hex-char hashes before the file extension.
    // Use this to determine if we can send a long-term cache duration.
    [GeneratedRegex(@"\.[0-9a-fA-F]{8}\.[^\.]*$", RegexOptions.Compiled)]
    private static partial Regex ContainsFileHashRegex();

    /// <summary>
    /// Add a CacheControl: no-cache, no-store header to all responses that reach this point in the pipeline.
    /// This middleware is a pre-hook and so the resulting Cache-Control can be overridden by other middleware or by individual endpoints.
    /// This middleware prevents browsers from unexpectedly caching API responses.
    /// </summary>
    public static IApplicationBuilder UseNoCacheResponseHeader(this IApplicationBuilder app)
    {
        return app.Use((context, next) =>
        {
            context.Response.Headers.CacheControl = "no-cache, no-store";
            return next(context);
        });
    }

    /// <summary>
    /// Map a route that will present an HTML page featuring a high level overview of all 
    /// types exposed by Coalesce and the static security rules that govern them.
    /// Security can be added by chaining a call to .RequireAuthorization(new AuthorizeAttribute { ... }) for example.
    /// </summary>
    public static IEndpointConventionBuilder MapCoalesceSecurityOverview(this IEndpointRouteBuilder builder, string pattern)
    {
        return builder.MapGet(pattern, async context =>
        {
            var repo = context.RequestServices.GetRequiredService<ReflectionRepository>();
            var dsFactory = context.RequestServices.GetRequiredService<IDataSourceFactory>();
            var behaviorsFactory = context.RequestServices.GetRequiredService<IBehaviorsFactory>();

            var data = GetSecurityOverviewData(repo, dsFactory, behaviorsFactory);

            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync("<script>DATA=");
            await JsonSerializer.SerializeAsync(context.Response.Body, data, serializeOptions);
            await context.Response.WriteAsync("</script>\n");

#if DEBUG
            static string GetThisFilePath([CallerFilePath] string? path = null) => path!;
            var thisSourceFilePath = GetThisFilePath();
            var file = new System.IO.FileInfo(thisSourceFilePath).Directory?.EnumerateFiles("SecurityOverview.html").FirstOrDefault();
            using var content = file?.OpenRead() ?? throw new Exception("SecurityOverview.html not found on disk.");
#else
            var content = System.Reflection.Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream("IntelliTect.Coalesce.Application.SecurityOverview.html")!;
#endif

            await content.CopyToAsync(context.Response.Body);
        });
    }

    internal static object GetSecurityOverviewData(ReflectionRepository repo, IDataSourceFactory dsFactory, IBehaviorsFactory behaviorsFactory)
    {
        return new
        {
            CrudTypes = repo
                .CrudApiBackedClasses
                .OrderBy(c => c.IsCustomDto)
                .ThenBy(c => c.Name)
                .Select(c =>
                {
                    var isImmutable = c.SecurityInfo is
                    {
                        Create.NoAccess: true,
                        Edit.NoAccess: true,
                        Save.NoAccess: true,
                        Delete.NoAccess: true
                    };

                    var actualDefaultSource = isImmutable && c.SecurityInfo.Read.NoAccess
                        ? null
                        : new ReflectionClassViewModel(dsFactory.GetDefaultDataSource(c.BaseViewModel, c).GetType());

                    var behaviors = isImmutable ? null : new ReflectionClassViewModel(behaviorsFactory.GetBehaviors(c.BaseViewModel, c).GetType());
                    var declaredBehaviors = repo.GetBehaviorsDeclaredFor(c);

                    return new
                    {
                        c.Name,
                        DtoBaseType = GetTypeInfo(c.DtoBaseViewModel?.Type),
                        Route = c.ApiRouteControllerPart,
                        c.SecurityInfo.Read,
                        c.SecurityInfo.Create,
                        c.SecurityInfo.Edit,
                        c.SecurityInfo.Delete,
                        DataSources = c
                            .ClientDataSources(repo)
                            .Select(ds => new
                            {
                                ds.FullyQualifiedName,
                                Name = ds.ClientTypeName,
                                IsDefault = ds.IsDefaultDataSource,
                                Kind = "custom",
                                Parameters = ds.DataSourceParameters.Select(p => new
                                {
                                    p.Name
                                })
                            })
                            .Append(actualDefaultSource is null ? null : new
                            {
                                actualDefaultSource.FullyQualifiedName,
                                Name = DataSourceFactory.DefaultSourceName,
                                IsDefault = true,
                                Kind = !actualDefaultSource.FullyQualifiedName.StartsWith("IntelliTect.Coalesce")
                                    ? "custom-fallback"
                                    : "default",
                                Parameters = actualDefaultSource.DataSourceParameters.Select(p => new
                                {
                                    p.Name
                                })
                            })
                            .Where(c => c != null).Select(c => c!)
                            .GroupBy(c => new { c.FullyQualifiedName })
                            .Select(c => new
                            {
                                Names = c.Where(ds => ds.Name != DataSourceFactory.DefaultSourceName || c.Count() == 1).Select(c => c.Name).ToList(),
                                ClassName = StripNamespacesFromTypeParams(c.Key.FullyQualifiedName),
                                IsDefault = c.Any(x => x.IsDefault),
                                c.First().Parameters,
                                c.First().Kind,
                            })
                            .OrderByDescending(c => c.IsDefault),

                        BehaviorsKind =
                            behaviors is null ? null :
                            declaredBehaviors is not null ? "custom" :
                            !behaviors.FullyQualifiedName.StartsWith("IntelliTect.Coalesce") ? "custom-fallback" :
                            "default",
                        BehaviorsTypeName =
                            behaviors is null ? null :
                            StripNamespacesFromTypeParams(behaviors.FullyQualifiedName),

                        Methods = c.ClientMethods.Select(GetMethodInfo),
                        Properties = c.ClientProperties.Select(GetPropertyInfo),
                        Usages = c.Usages.OrderBy(u => u.GetType().Name).Select(GetUsageInfo)
                    };
                }),
            ExternalTypes = repo.ExternalTypes
                .OrderBy(c => c.Name)
                .Select(c => new
                {
                    Name = c.Name,
                    Properties = c.ClientProperties.Select(GetPropertyInfo),
                    Usages = c.Usages.OrderBy(u => u.GetType().Name).Select(GetUsageInfo)
                }),
            ServiceTypes = repo.Services
                .OrderBy(c => c.Name)
                .Select(c => new
                {
                    Name = c.Name,
                    Route = c.ApiRouteControllerPart,
                    Methods = c.ClientMethods.Select(GetMethodInfo)
                })
        };

        string StripNamespacesFromTypeParams(string s) => s.IndexOf('<') is int start and >= 0
            ? new Regex(@"[^<>,+\s]*\.").Replace(s, "", 5, start)
            : s;

        object GetUsageInfo(ValueViewModel v) => v switch
        {
            PropertyViewModel p => new { Type = GetTypeInfo(p.EffectiveParent.Type), Property = GetPropertyInfo(p) },
            ParameterViewModel p => new { Type = GetTypeInfo(p.Parent.Parent.Type), Method = GetMethodInfo(p.Parent), Parameter = p.Name },
            MethodReturnViewModel p => new { Type = GetTypeInfo(p.Method.Parent.Type), Method = GetMethodInfo(p.Method), IsReturn = true },
            _ => throw new NotImplementedException(),
        };

        object GetPropertyInfo(PropertyViewModel p) => new
        {
            p.Name,
            Type = GetTypeInfo(p.Type),
            p.SecurityInfo.Read,
            p.SecurityInfo.Init,
            p.SecurityInfo.Edit,
            Restrictions = p.SecurityInfo.Restrictions.Select(t => t.Name).ToList(),
            p.IsCreateOnly,
            p.IsPrimaryKey
        };

        object GetMethodInfo(MethodViewModel m) => new
        {
            m.Name,
            Execute = m.SecurityInfo.Execute,
            HttpMethod = m.ApiActionHttpMethod,

            m.LoadFromDataSourceName,
            m.IsModelInstanceMethod,

            Parameters = m.ApiParameters.Select(p => new
            {
                p.Name,
                Type = GetTypeInfo(p.Type)
            }),
            Return = GetTypeInfo(m.ResultType),
        };

        object? GetTypeInfo(TypeViewModel? t) => t is null ? null : new
        {
            // Strip out namespaces:
            Display = Regex.Replace(t.FullyQualifiedName, @"[^<>,+]*\.", ""),
            LinkedType =
                repo.ClientClasses.Contains(t.PureType.ClassViewModel) ||
                repo.Services.Contains(t.PureType.ClassViewModel!)
                ? t.PureType.Name : null,
        };
    }

}

/// <summary>
/// Options for configuring Vite static file middleware.
/// </summary>
public class ViteStaticFilesOptions
{
    /// <summary>
    /// A delegate that determines if a request should return an unauthorized response.
    /// If this delegate returns false, an unauthorized response will be returned.
    /// </summary>
    public Func<StaticFileResponseContext, ValueTask<bool>>? OnAuthorizeAsync { get; set; }

    /// <summary>
    /// Additional logic to execute when preparing the response.
    /// This is called at the end of the response preparation process.
    /// </summary>
    public Func<StaticFileResponseContext, ValueTask>? OnPrepareResponseAsync { get; set; }
}
