#if NETCOREAPP3_1_OR_GREATER

using IntelliTect.Coalesce.Api.Behaviors;
using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;


namespace IntelliTect.Coalesce
{
    public static class CoalesceApplicationBuilderExtensions
    {
        public static IEndpointConventionBuilder MapCoalesceSecurityOverview(this IEndpointRouteBuilder builder, string pattern)
        {
            return builder.MapGet(pattern, async context =>
            {
                var repo = context.RequestServices.GetRequiredService<ReflectionRepository>();
                var dsFactory = context.RequestServices.GetRequiredService<IDataSourceFactory>();
                var behaviorsFactory = context.RequestServices.GetRequiredService<IBehaviorsFactory>();

                var data = new
                {
                    CrudTypes = repo
                        .CrudApiBackedClasses
                        .OrderBy(c => c.Name)
                        .Select(c =>
                        {
                            var actualDefaultSource = new ReflectionClassViewModel(
                                dsFactory.GetDefaultDataSource(c.BaseViewModel, c).GetType()
                            );

                            var behavious = c.SecurityInfo is
                            {
                                Create: { NoAccess: true },
                                Edit: { NoAccess: true },
                                Save: { NoAccess: true },
                                Delete: { NoAccess: true }
                            } ? null : new ReflectionClassViewModel(behaviorsFactory.GetBehaviors(c.BaseViewModel, c).GetType());

                            return new
                            {
                                Name = c.Name,
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
                                        Parameters = ds.DataSourceParameters.Select(p => new
                                        {
                                            p.Name
                                        })
                                    })
                                    .Prepend(new
                                    {
                                        actualDefaultSource.FullyQualifiedName,
                                        Name = DataSourceFactory.DefaultSourceName,
                                        IsDefault = true,
                                        Parameters = actualDefaultSource.DataSourceParameters.Select(p => new
                                        {
                                            p.Name
                                        })
                                    })
                                    .GroupBy(c => new { c.FullyQualifiedName })
                                    .Select(c => new
                                    {
                                        Names = c.Select(c => c.Name).ToList(),
                                        ClassName = Regex.Replace(c.Key.FullyQualifiedName, "<.*", ""),
                                        IsDefault = c.Any(x => x.IsDefault),
                                        c.First().Parameters,
                                    })
                                    .OrderByDescending(c => c.IsDefault),
                                BehaviorsTypeName = behavious == null ? null : Regex.Replace(behavious.FullyQualifiedName, "<.*", ""),
                                Methods = c.ClientMethods.Select(GetMethodInfo),
                                Properties = c.ClientProperties.Select(GetPropertyInfo)
                            };
                        }),
                    ExternalTypes = repo.ExternalTypes
                        .OrderBy(c => c.Name)
                        .Select(c => new
                        {
                            Name = c.Name,
                            Properties = c.ClientProperties.Select(GetPropertyInfo)
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
                
                var serializeOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                context.Response.StatusCode = 200;
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync("<script>DATA=");
                await JsonSerializer.SerializeAsync(context.Response.Body, data, serializeOptions);
                await context.Response.WriteAsync("</script>\n");

                var content = Assembly
                    .GetExecutingAssembly()
                    .GetManifestResourceStream("IntelliTect.Coalesce.Application.SecurityOverview.html")!;
                await content.CopyToAsync(context.Response.Body);

                object GetPropertyInfo(PropertyViewModel p) => new
                {
                    p.Name,
                    Type = GetTypeInfo(p.Type),
                    p.SecurityInfo.Read,
                    p.SecurityInfo.Edit,
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

                object GetTypeInfo(TypeViewModel t) => new
                {
                    // Strip out namespaces:
                    Display = Regex.Replace(t.FullyQualifiedName, @"[^<>,+]*\.", ""),
                    LinkedType = repo.ClientClasses.Contains(t.PureType.ClassViewModel) ? t.PureType.Name : null,
                };
            });
        }
    }
}

#endif