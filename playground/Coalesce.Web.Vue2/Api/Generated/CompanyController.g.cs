
using Coalesce.Web.Vue2.Models;
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

namespace Coalesce.Web.Vue2.Api
{
    [Route("api/Company")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class CompanyController
        : BaseApiController<Coalesce.Domain.Company, CompanyDtoGen, Coalesce.Domain.AppDbContext>
    {
        public CompanyController(CrudContext<Coalesce.Domain.AppDbContext> context) : base(context)
        {
            GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<Coalesce.Domain.Company>();
        }

        [HttpGet("get/{id}")]
        [Authorize]
        public virtual Task<ItemResult<CompanyDtoGen>> Get(
            int id,
            DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.Company> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [Authorize]
        public virtual Task<ListResult<CompanyDtoGen>> List(
            ListParameters parameters,
            IDataSource<Coalesce.Domain.Company> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [Authorize]
        public virtual Task<ItemResult<int>> Count(
            FilterParameters parameters,
            IDataSource<Coalesce.Domain.Company> dataSource)
            => CountImplementation(parameters, dataSource);

        [HttpPost("save")]
        [Authorize]
        public virtual Task<ItemResult<CompanyDtoGen>> Save(
            [FromForm] CompanyDtoGen dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.Company> dataSource,
            IBehaviors<Coalesce.Domain.Company> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

        [HttpPost("bulkSave")]
        [Authorize]
        public virtual Task<ItemResult<CompanyDtoGen>> BulkSave(
            [FromBody] BulkSaveRequest dto,
            [FromQuery] DataSourceParameters parameters,
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromServices] IBehaviorsFactory behaviorsFactory)
            => BulkSaveImplementation(dto, parameters, dataSourceFactory, behaviorsFactory);

        [HttpPost("delete/{id}")]
        [Authorize]
        public virtual Task<ItemResult<CompanyDtoGen>> Delete(
            int id,
            IBehaviors<Coalesce.Domain.Company> behaviors,
            IDataSource<Coalesce.Domain.Company> dataSource)
            => DeleteImplementation(id, new DataSourceParameters(), dataSource, behaviors);

        // Methods from data class exposed through API Controller.

        /// <summary>
        /// Method: ConflictingParameterNames
        /// </summary>
        [HttpPost("ConflictingParameterNames")]
        [Authorize]
        public virtual async Task<ItemResult> ConflictingParameterNames(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromForm(Name = "id")] int id,
            [FromForm(Name = "companyParam")] CompanyDtoGen companyParam,
            [FromForm(Name = "name")] string name)
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Company, Coalesce.Domain.Company>("Default");
            var (itemResult, _) = await dataSource.GetItemAsync(id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult(itemResult);
            }
            var item = itemResult.Object;
            var _params = new
            {
                companyParam = companyParam,
                name = name
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("ConflictingParameterNames"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return _validationResult;
            }

            var _mappingContext = new MappingContext(User);
            item.ConflictingParameterNames(
                _params.companyParam.MapToNew(_mappingContext),
                _params.name
            );
            await Db.SaveChangesAsync();
            var _result = new ItemResult();
            return _result;
        }

        /// <summary>
        /// Method: GetCertainItems
        /// </summary>
        [HttpPost("GetCertainItems")]
        [Authorize]
        public virtual ItemResult<System.Collections.Generic.ICollection<CompanyDtoGen>> GetCertainItems(
            [FromForm(Name = "isDeleted")] bool isDeleted = false)
        {
            var _params = new
            {
                isDeleted = isDeleted
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("GetCertainItems"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<System.Collections.Generic.ICollection<CompanyDtoGen>>(_validationResult);
            }

            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(User);
            var _methodResult = Coalesce.Domain.Company.GetCertainItems(
                Db,
                _params.isDeleted
            );
            var _result = new ItemResult<System.Collections.Generic.ICollection<CompanyDtoGen>>();
            _result.Object = _methodResult?.ToList().Select(o => Mapper.MapToDto<Coalesce.Domain.Company, CompanyDtoGen>(o, _mappingContext, includeTree)).ToList();
            return _result;
        }
    }
}
