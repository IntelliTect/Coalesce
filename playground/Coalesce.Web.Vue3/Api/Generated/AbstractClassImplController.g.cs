
using Coalesce.Web.Vue3.Models;
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

namespace Coalesce.Web.Vue3.Api
{
    [Route("api/AbstractClassImpl")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class AbstractClassImplController
        : BaseApiController<Coalesce.Domain.AbstractClassImpl, AbstractClassImplParameter, AbstractClassImplResponse, Coalesce.Domain.AppDbContext>
    {
        public AbstractClassImplController(CrudContext<Coalesce.Domain.AppDbContext> context) : base(context)
        {
            GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<Coalesce.Domain.AbstractClassImpl>();
        }

        [HttpGet("get/{id}")]
        [Authorize]
        public virtual Task<ItemResult<AbstractClassImplResponse>> Get(
            int id,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.AbstractClassImpl> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [Authorize]
        public virtual Task<ListResult<AbstractClassImplResponse>> List(
            [FromQuery] ListParameters parameters,
            IDataSource<Coalesce.Domain.AbstractClassImpl> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [Authorize]
        public virtual Task<ItemResult<int>> Count(
            [FromQuery] FilterParameters parameters,
            IDataSource<Coalesce.Domain.AbstractClassImpl> dataSource)
            => CountImplementation(parameters, dataSource);

        [HttpPost("save")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        [Authorize]
        public virtual Task<ItemResult<AbstractClassImplResponse>> Save(
            [FromForm] AbstractClassImplParameter dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.AbstractClassImpl> dataSource,
            IBehaviors<Coalesce.Domain.AbstractClassImpl> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

        [HttpPost("save")]
        [Consumes("application/json")]
        [Authorize]
        public virtual Task<ItemResult<AbstractClassImplResponse>> SaveFromJson(
            [FromBody] AbstractClassImplParameter dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.AbstractClassImpl> dataSource,
            IBehaviors<Coalesce.Domain.AbstractClassImpl> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

        [HttpPost("bulkSave")]
        [Authorize]
        public virtual Task<ItemResult<AbstractClassImplResponse>> BulkSave(
            [FromBody] BulkSaveRequest dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.AbstractClassImpl> dataSource,
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromServices] IBehaviorsFactory behaviorsFactory)
            => BulkSaveImplementation(dto, parameters, dataSource, dataSourceFactory, behaviorsFactory);

        [HttpPost("delete/{id}")]
        [Authorize]
        public virtual Task<ItemResult<AbstractClassImplResponse>> Delete(
            int id,
            IBehaviors<Coalesce.Domain.AbstractClassImpl> behaviors,
            IDataSource<Coalesce.Domain.AbstractClassImpl> dataSource)
            => DeleteImplementation(id, new DataSourceParameters(), dataSource, behaviors);

        // Methods from data class exposed through API Controller.

        /// <summary>
        /// Method: GetId
        /// </summary>
        [HttpPost("GetId")]
        [Authorize]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual async Task<ItemResult<int>> GetId(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromForm(Name = "id")] int id)
        {
            var _params = new
            {
                Id = id
            };

            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.AbstractClassImpl, Coalesce.Domain.AbstractClassImpl>("Default");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<int>(itemResult);
            }
            var item = itemResult.Object;
            var _methodResult = item.GetId();
            var _result = new ItemResult<int>();
            _result.Object = _methodResult;
            return _result;
        }

        public class AbstractClassImplGetIdParameters
        {
            public int Id { get; set; }
        }

        /// <summary>
        /// Method: GetId
        /// </summary>
        [HttpPost("GetId")]
        [Authorize]
        [Consumes("application/json")]
        public virtual async Task<ItemResult<int>> GetId(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromBody] AbstractClassImplGetIdParameters _params
        )
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.AbstractClassImpl, Coalesce.Domain.AbstractClassImpl>("Default");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<int>(itemResult);
            }
            var item = itemResult.Object;
            var _methodResult = item.GetId();
            var _result = new ItemResult<int>();
            _result.Object = _methodResult;
            return _result;
        }

        /// <summary>
        /// Method: GetCount
        /// </summary>
        [HttpPost("GetCount")]
        [Authorize]
        public virtual ItemResult<int> GetCount()
        {
            var _methodResult = Coalesce.Domain.AbstractClass.GetCount(
                Db
            );
            var _result = new ItemResult<int>();
            _result.Object = _methodResult;
            return _result;
        }

        /// <summary>
        /// Method: EchoAbstractModel
        /// </summary>
        [HttpPost("EchoAbstractModel")]
        [Authorize]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual ItemResult<AbstractClassResponse> EchoAbstractModel(
            [FromForm(Name = "model")] AbstractClassParameter model)
        {
            var _params = new
            {
                Model = !Request.Form.HasAnyValue("model") ? null : model
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("EchoAbstractModel"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<AbstractClassResponse>(_validationResult);
            }

            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = Coalesce.Domain.AbstractClass.EchoAbstractModel(
                _params.Model?.MapToNew(_mappingContext)
            );
            var _result = new ItemResult<AbstractClassResponse>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.AbstractClass, AbstractClassResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        }

        public class AbstractClassImplEchoAbstractModelParameters
        {
            public AbstractClassParameter Model { get; set; }
        }

        /// <summary>
        /// Method: EchoAbstractModel
        /// </summary>
        [HttpPost("EchoAbstractModel")]
        [Authorize]
        [Consumes("application/json")]
        public virtual ItemResult<AbstractClassResponse> EchoAbstractModel(
            [FromBody] AbstractClassImplEchoAbstractModelParameters _params
        )
        {
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("EchoAbstractModel"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<AbstractClassResponse>(_validationResult);
            }

            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = Coalesce.Domain.AbstractClass.EchoAbstractModel(
                _params.Model?.MapToNew(_mappingContext)
            );
            var _result = new ItemResult<AbstractClassResponse>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.AbstractClass, AbstractClassResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        }
    }
}
