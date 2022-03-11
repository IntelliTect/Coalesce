using System.Diagnostics;
using System.Linq;
using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
            var data = repo
                .ControllerBackedClasses
                .Select(c =>
                {
                    var actualDefaultSource = new ReflectionClassViewModel(HttpContext.RequestServices
                        .GetRequiredService<IDataSourceFactory>()
                        .GetDefaultDataSource(c.BaseViewModel, c).GetType()
                    );
                    return new
                    {
                        Name = c.Name,
                        SecurityInfo = new
                        {
                            Read = c.SecurityInfo.Read.ToString(),
                            Create = c.SecurityInfo.Create.ToString(),
                            Edit = c.SecurityInfo.Edit.ToString(),
                            Delete = c.SecurityInfo.Delete.ToString(),
                        },
                        DataSources = c
                            .ClientDataSources(repo)
                            .Select(ds => new
                            {
                                ds.FullyQualifiedName,
                                ds.ClientTypeName,
                                ds.IsDefaultDataSource,
                                // TODO: DS parameters
                            })
                            .Append(new
                            {
                                actualDefaultSource.FullyQualifiedName,
                                ClientTypeName = DataSourceFactory.DefaultSourceName,
                                IsDefaultDataSource = true,
                            })
                            .DistinctBy(c => c.FullyQualifiedName),
                            // TODO: behaviors
                        Methods = c.ClientMethods.Select(m => new
                        {
                            m.Name,
                            m.LoadFromDataSourceName,
                            Execute = m.SecurityInfo.Execute.ToString(),
                            m.ApiActionHttpMethod
                        }),
                        Properties = c.ClientProperties.Select(p => new
                        {
                            p.Name, 
                            Read = p.SecurityInfo.Read.ToString(),
                            Edit = !p.IsClientSerializable ? "Deny" : p.SecurityInfo.Edit.ToString(),
                        })
                    };
                })
                .ToList();

            var settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.Converters.Add(new StringEnumConverter());

            return Json(data, settings);
        }
    }
}
