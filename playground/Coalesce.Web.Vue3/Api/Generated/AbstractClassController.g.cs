
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
    [Route("api/AbstractClass")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class AbstractClassController
        : BaseApiController<Coalesce.Domain.AbstractClass, AbstractClassParameter, AbstractClassResponse, Coalesce.Domain.AppDbContext>
    {
        public AbstractClassController(CrudContext<Coalesce.Domain.AppDbContext> context) : base(context)
        {
            GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<Coalesce.Domain.AbstractClass>();
        }

        [HttpGet("get/{id}")]
        [Authorize]
        public virtual Task<ItemResult<AbstractClassResponse>> Get(
            int id,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.AbstractClass> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [Authorize]
        public virtual Task<ListResult<AbstractClassResponse>> List(
            [FromQuery] ListParameters parameters,
            IDataSource<Coalesce.Domain.AbstractClass> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [Authorize]
        public virtual Task<ItemResult<int>> Count(
            [FromQuery] FilterParameters parameters,
            IDataSource<Coalesce.Domain.AbstractClass> dataSource)
            => CountImplementation(parameters, dataSource);

        [HttpPost("bulkSave")]
        [Authorize]
        public virtual Task<ItemResult<AbstractClassResponse>> BulkSave(
            [FromBody] BulkSaveRequest dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.AbstractClass> dataSource,
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromServices] IBehaviorsFactory behaviorsFactory)
            => BulkSaveImplementation(dto, parameters, dataSource, dataSourceFactory, behaviorsFactory);

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
            var _method = GeneratedForClassViewModel!.MethodByName("GetId");
            var _params = new
            {
                Id = id
            };

            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.AbstractClass, Coalesce.Domain.AbstractClass>("Default");
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

        public class AbstractClassGetIdParameters
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
            [FromBody] AbstractClassGetIdParameters _params
        )
        {
            var _method = GeneratedForClassViewModel!.MethodByName("GetId");
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.AbstractClass, Coalesce.Domain.AbstractClass>("Default");
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
            var _method = GeneratedForClassViewModel!.MethodByName("GetCount");
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
            var _method = GeneratedForClassViewModel!.MethodByName("EchoAbstractModel");
            var _params = new
            {
                Model = !Request.Form.HasAnyValue("model") ? null : model
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
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

        public class AbstractClassEchoAbstractModelParameters
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
            [FromBody] AbstractClassEchoAbstractModelParameters _params
        )
        {
            var _method = GeneratedForClassViewModel!.MethodByName("EchoAbstractModel");
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
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
