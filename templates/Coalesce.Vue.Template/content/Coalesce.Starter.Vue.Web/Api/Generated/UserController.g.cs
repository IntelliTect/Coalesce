
using Coalesce.Starter.Vue.Web.Models;
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

namespace Coalesce.Starter.Vue.Web.Api
{
    [Route("api/User")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class UserController
        : BaseApiController<Coalesce.Starter.Vue.Data.Models.User, UserParameter, UserResponse, Coalesce.Starter.Vue.Data.AppDbContext>
    {
        public UserController(CrudContext<Coalesce.Starter.Vue.Data.AppDbContext> context) : base(context)
        {
            GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<Coalesce.Starter.Vue.Data.Models.User>();
        }

        [HttpGet("get/{id}")]
        [Authorize]
        public virtual Task<ItemResult<UserResponse>> Get(
            string id,
            DataSourceParameters parameters,
            IDataSource<Coalesce.Starter.Vue.Data.Models.User> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [Authorize]
        public virtual Task<ListResult<UserResponse>> List(
            ListParameters parameters,
            IDataSource<Coalesce.Starter.Vue.Data.Models.User> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [Authorize]
        public virtual Task<ItemResult<int>> Count(
            FilterParameters parameters,
            IDataSource<Coalesce.Starter.Vue.Data.Models.User> dataSource)
            => CountImplementation(parameters, dataSource);

        [HttpPost("save")]
        [Authorize]
        public virtual Task<ItemResult<UserResponse>> Save(
            [FromForm] UserParameter dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Starter.Vue.Data.Models.User> dataSource,
            IBehaviors<Coalesce.Starter.Vue.Data.Models.User> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

        [HttpPost("bulkSave")]
        [Authorize]
        public virtual Task<ItemResult<UserResponse>> BulkSave(
            [FromBody] BulkSaveRequest dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Starter.Vue.Data.Models.User> dataSource,
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromServices] IBehaviorsFactory behaviorsFactory)
            => BulkSaveImplementation(dto, parameters, dataSource, dataSourceFactory, behaviorsFactory);

        // Methods from data class exposed through API Controller.

        /// <summary>
        /// Method: GetPhoto
        /// </summary>
        [HttpGet("GetPhoto")]
        [Authorize]
        public virtual async Task<ActionResult<ItemResult<IntelliTect.Coalesce.Models.IFile>>> GetPhoto(
            [FromServices] IDataSourceFactory dataSourceFactory,
            string id,
            byte[] etag)
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Starter.Vue.Data.Models.User, Coalesce.Starter.Vue.Data.Models.User>("Default");
            var itemResult = await dataSource.GetItemAsync(id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<IntelliTect.Coalesce.Models.IFile>(itemResult);
            }
            var item = itemResult.Object;

            var _currentVaryValue = item.PhotoMD5;
            if (_currentVaryValue != default)
            {
                var _expectedEtagHeader = new Microsoft.Net.Http.Headers.EntityTagHeaderValue('"' + Microsoft.AspNetCore.WebUtilities.Base64UrlTextEncoder.Encode(_currentVaryValue) + '"');
                var _cacheControlHeader = new Microsoft.Net.Http.Headers.CacheControlHeaderValue { Private = true, MaxAge = TimeSpan.Zero };
                if (etag != default && _currentVaryValue.SequenceEqual(etag))
                {
                    _cacheControlHeader.MaxAge = TimeSpan.FromDays(30);
                }
                Response.GetTypedHeaders().CacheControl = _cacheControlHeader;
                Response.GetTypedHeaders().ETag = _expectedEtagHeader;
                if (Request.GetTypedHeaders().IfNoneMatch.Any(value => value.Compare(_expectedEtagHeader, true)))
                {
                    return StatusCode(StatusCodes.Status304NotModified);
                }
            }

            var _methodResult = item.GetPhoto(
                Db
            );
            if (_methodResult.Object != null)
            {
                string _contentType = _methodResult.Object.ContentType;
                if (string.IsNullOrWhiteSpace(_contentType) && (
                    string.IsNullOrWhiteSpace(_methodResult.Object.Name) ||
                    !(new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider().TryGetContentType(_methodResult.Object.Name, out _contentType))
                ))
                {
                    _contentType = "application/octet-stream";
                }
                return File(_methodResult.Object.Content, _contentType, _methodResult.Object.Name, !(_methodResult.Object.Content is System.IO.MemoryStream));
            }
            var _result = new ItemResult<IntelliTect.Coalesce.Models.IFile>(_methodResult);
            _result.Object = _methodResult.Object;
            return _result;
        }
    }
}
