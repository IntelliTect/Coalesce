using System;
using System.IO;
using IntelliTect.Coalesce.CodeGeneration.Api.Generators;
using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.TypeDefinition;

namespace IntelliTect.Coalesce.Tests
{
    /// <summary>
    /// Sample code that demonstrates what a generated controller with DateTime primary key should look like
    /// </summary>
    public class GeneratedControllerExample
    {
        public static string GetExpectedDateTimeEntityController()
        {
            return @"
using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Api.Behaviors;
using IntelliTect.Coalesce.Api.Controllers;
using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Mapping.IncludeTrees;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Coalesce.Tests.Api
{
    [Route(""api/DateTimeEntity"")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class DateTimeEntityController
        : BaseApiController<DateTimeEntity, DateTimeEntityParameter, DateTimeEntityResponse, AppDbContext>
    {
        public DateTimeEntityController(CrudContext<AppDbContext> context) : base(context)
        {
            GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<DateTimeEntity>();
        }

        [HttpGet(""get/{id}"")]
        [AllowAnonymous]
        public virtual Task<ItemResult<DateTimeEntityResponse>> Get(
            string id,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<DateTimeEntity> dataSource)
        {
            var parsedId = (DateTime)Convert.ChangeType(id, typeof(DateTime));
            return GetImplementation(parsedId, parameters, dataSource);
        }

        [HttpPost(""delete/{id}"")]
        [Authorize]
        public virtual Task<ItemResult<DateTimeEntityResponse>> Delete(
            string id,
            IBehaviors<DateTimeEntity> behaviors,
            IDataSource<DateTimeEntity> dataSource)
        {
            var parsedId = (DateTime)Convert.ChangeType(id, typeof(DateTime));
            return DeleteImplementation(parsedId, new DataSourceParameters(), dataSource, behaviors);
        }
    }
}";
        }

        public static void TestControllerGeneration()
        {
            var repository = new ReflectionRepository();
            var classViewModel = repository.GetClassViewModel<DateTimeEntity>();
            var services = new GeneratorServices(repository);
            var generator = new ModelApiController(services);
            
            generator.Model = classViewModel;
            var generatedCode = generator.GenerateOutput();
            
            Console.WriteLine("Generated Controller Code:");
            Console.WriteLine(generatedCode);
        }
    }
}