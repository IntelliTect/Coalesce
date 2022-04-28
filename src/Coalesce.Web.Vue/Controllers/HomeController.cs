using System.Diagnostics;
using System.Linq;
using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Coalesce.Web.Vue.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment hostingEnvironment;

        public HomeController(IWebHostEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {

            IFileProvider provider = new PhysicalFileProvider(hostingEnvironment.WebRootPath);
            IFileInfo fileInfo = provider.GetFileInfo("index.html");
            if (!fileInfo.Exists)
            {
                return Ok("index.html not found. HMR build is probably still running for the first time. Keep refreshing...");
            }
            var readStream = fileInfo.CreateReadStream();

            return File(readStream, "text/html");
        }

        public IActionResult Error()
        {
            ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            return View();
        }

        public IActionResult SecurityOverview()
        {
            // TODO: TEMPORARY WIP.

            var repo = ReflectionRepository.Global;
            var crudClasses = repo
                .CrudApiBackedClasses
                .OrderBy(c => c.Name)
                .Select(c =>
                {
                    var actualDefaultSource = new ReflectionClassViewModel(HttpContext.RequestServices
                        .GetRequiredService<IDataSourceFactory>()
                        .GetDefaultDataSource(c.BaseViewModel, c).GetType()
                    );

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
                                // Put the resolved default FIRST (.Prepend) so that the
                                // effective default for StandaloneEntities is correctly tagged.
                                actualDefaultSource.FullyQualifiedName,
                                Name = DataSourceFactory.DefaultSourceName,
                                IsDefault = true,
                                Parameters = actualDefaultSource.DataSourceParameters.Select(p => new
                                {
                                    p.Name
                                })
                            })
                            .DistinctBy(c => c.FullyQualifiedName)
                            .OrderByDescending(c => c.IsDefault),
                            // TODO: behaviors
                        Methods = c.ClientMethods.Select(m => new
                        {
                            m.Name,
                            m.LoadFromDataSourceName,
                            Execute = m.SecurityInfo.Execute,
                            HttpMethod = m.ApiActionHttpMethod
                        }),
                        Properties = c.ClientProperties.Select(p => new
                        {
                            p.Name, 
                            p.SecurityInfo.Read,
                            p.SecurityInfo.Edit,
                        })
                    };
                })
                .ToList();

            var settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.Converters.Add(new StringEnumConverter());

            return Json(new
            {
                CrudTypes = crudClasses,
                ServiceTypes = repo.Services
                    .OrderBy(c => c.Name)
                    .Select(c =>
                    {
                        return new
                        {
                            Name = c.Name,
                            Route = c.ApiRouteControllerPart,
                            Methods = c.ClientMethods.Select(m => new
                            {
                                m.Name,
                                Execute = m.SecurityInfo.Execute,
                                HttpMethod = m.ApiActionHttpMethod
                            })
                        };
                    })
                    .ToList()
            }, settings);
        }
    }
}
