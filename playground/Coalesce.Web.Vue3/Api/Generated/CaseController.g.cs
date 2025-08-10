
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
    [Route("api/Case")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class CaseController
        : BaseApiController<Coalesce.Domain.Case, CaseParameter, CaseResponse, Coalesce.Domain.AppDbContext>
    {
        public CaseController(CrudContext<Coalesce.Domain.AppDbContext> context) : base(context)
        {
            GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<Coalesce.Domain.Case>();
        }

        [HttpGet("get/{id}")]
        [AllowAnonymous]
        public virtual Task<ItemResult<CaseResponse>> Get(
            int id,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.Case> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [AllowAnonymous]
        public virtual Task<ListResult<CaseResponse>> List(
            [FromQuery] ListParameters parameters,
            IDataSource<Coalesce.Domain.Case> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [AllowAnonymous]
        public virtual Task<ItemResult<int>> Count(
            [FromQuery] FilterParameters parameters,
            IDataSource<Coalesce.Domain.Case> dataSource)
            => CountImplementation(parameters, dataSource);

        [HttpPost("save")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        [AllowAnonymous]
        public virtual Task<ItemResult<CaseResponse>> Save(
            [FromForm] CaseParameter dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.Case> dataSource,
            IBehaviors<Coalesce.Domain.Case> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

        [HttpPost("save")]
        [Consumes("application/json")]
        [AllowAnonymous]
        public virtual Task<ItemResult<CaseResponse>> SaveFromJson(
            [FromBody] CaseParameter dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.Case> dataSource,
            IBehaviors<Coalesce.Domain.Case> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

        [HttpPost("bulkSave")]
        [AllowAnonymous]
        public virtual Task<ItemResult<CaseResponse>> BulkSave(
            [FromBody] BulkSaveRequest dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.Case> dataSource,
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromServices] IBehaviorsFactory behaviorsFactory)
            => BulkSaveImplementation(dto, parameters, dataSource, dataSourceFactory, behaviorsFactory);

        [HttpPost("delete/{id}")]
        [Authorize]
        public virtual Task<ItemResult<CaseResponse>> Delete(
            int id,
            IBehaviors<Coalesce.Domain.Case> behaviors,
            IDataSource<Coalesce.Domain.Case> dataSource)
            => DeleteImplementation(id, new DataSourceParameters(), dataSource, behaviors);

        // Methods from data class exposed through API Controller.

        /// <summary>
        /// Method: GetCaseTitles
        /// </summary>
        [HttpPost("GetCaseTitles")]
        [Authorize]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual ItemResult<System.Collections.Generic.ICollection<string>> GetCaseTitles(
            [FromForm(Name = "search")] string search)
        {
            var _method = GeneratedForClassViewModel!.MethodByName("GetCaseTitles");
            var _params = new
            {
                Search = search
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return new ItemResult<System.Collections.Generic.ICollection<string>>(_validationResult);
            }

            var _methodResult = Coalesce.Domain.Case.GetCaseTitles(
                Db,
                _params.Search
            );
            var _result = new ItemResult<System.Collections.Generic.ICollection<string>>();
            _result.Object = _methodResult?.ToList();
            return _result;
        }

        public class GetCaseTitlesParameters
        {
            public string Search { get; set; }
        }

        /// <summary>
        /// Method: GetCaseTitles
        /// </summary>
        [HttpPost("GetCaseTitles")]
        [Authorize]
        [Consumes("application/json")]
        public virtual ItemResult<System.Collections.Generic.ICollection<string>> GetCaseTitles(
            [FromBody] GetCaseTitlesParameters _params
        )
        {
            var _method = GeneratedForClassViewModel!.MethodByName("GetCaseTitles");
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return new ItemResult<System.Collections.Generic.ICollection<string>>(_validationResult);
            }

            var _methodResult = Coalesce.Domain.Case.GetCaseTitles(
                Db,
                _params.Search
            );
            var _result = new ItemResult<System.Collections.Generic.ICollection<string>>();
            _result.Object = _methodResult?.ToList();
            return _result;
        }

        /// <summary>
        /// Method: GetSomeCases
        /// </summary>
        [HttpPost("GetSomeCases")]
        [Authorize]
        public virtual ItemResult<System.Collections.Generic.ICollection<CaseResponse>> GetSomeCases()
        {
            var _method = GeneratedForClassViewModel!.MethodByName("GetSomeCases");
            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = Coalesce.Domain.Case.GetSomeCases(
                Db
            );
            var _result = new ItemResult<System.Collections.Generic.ICollection<CaseResponse>>();
            _result.Object = _methodResult?.ToList().Select(o => Mapper.MapToDto<Coalesce.Domain.Case, CaseResponse>(o, _mappingContext, includeTree)).ToList();
            return _result;
        }

        /// <summary>
        /// Method: GetAllOpenCasesCount
        /// </summary>
        [HttpPost("GetAllOpenCasesCount")]
        [Authorize]
        public virtual ItemResult<int> GetAllOpenCasesCount()
        {
            var _method = GeneratedForClassViewModel!.MethodByName("GetAllOpenCasesCount");
            var _methodResult = Coalesce.Domain.Case.GetAllOpenCasesCount(
                Db
            );
            var _result = new ItemResult<int>();
            _result.Object = _methodResult;
            return _result;
        }

        /// <summary>
        /// Method: RandomizeDatesAndStatus
        /// </summary>
        [HttpPost("RandomizeDatesAndStatus")]
        [Authorize]
        public virtual ItemResult RandomizeDatesAndStatus()
        {
            var _method = GeneratedForClassViewModel!.MethodByName("RandomizeDatesAndStatus");
            Coalesce.Domain.Case.RandomizeDatesAndStatus(
                Db
            );
            var _result = new ItemResult();
            return _result;
        }

        /// <summary>
        /// Method: UploadImage
        /// </summary>
        [HttpPost("UploadImage")]
        [Authorize]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual async Task<ItemResult> UploadImage(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromForm(Name = "id")] int id,
            Microsoft.AspNetCore.Http.IFormFile @file)
        {
            var _method = GeneratedForClassViewModel!.MethodByName("UploadImage");
            var _params = new
            {
                Id = id,
                File = @file == null ? null : new IntelliTect.Coalesce.Models.File { Name = @file.FileName, ContentType = @file.ContentType, Length = @file.Length, Content = @file.OpenReadStream() }
            };

            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Case, Coalesce.Domain.Case>("Default");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult(itemResult);
            }
            var item = itemResult.Object;
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return _validationResult;
            }

            await item.UploadImage(
                Db,
                _params.File
            );
            var _result = new ItemResult();
            return _result;
        }

        public class UploadImageParameters
        {
            public int Id { get; set; }
            public IntelliTect.Coalesce.Models.FileParameter File { get; set; }
        }

        /// <summary>
        /// Method: UploadImage
        /// </summary>
        [HttpPost("UploadImage")]
        [Authorize]
        [Consumes("application/json")]
        public virtual async Task<ItemResult> UploadImage(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromBody] UploadImageParameters _params
        )
        {
            var _method = GeneratedForClassViewModel!.MethodByName("UploadImage");
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Case, Coalesce.Domain.Case>("Default");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult(itemResult);
            }
            var item = itemResult.Object;
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return _validationResult;
            }

            await item.UploadImage(
                Db,
                _params.File
            );
            var _result = new ItemResult();
            return _result;
        }

        /// <summary>
        /// Method: DownloadImage
        /// </summary>
        [HttpGet("DownloadImage")]
        [Authorize]
        public virtual async Task<ActionResult<ItemResult<IntelliTect.Coalesce.Models.IFile>>> DownloadImage(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromQuery] int id,
            [FromQuery] byte[] etag)
        {
            var _method = GeneratedForClassViewModel!.MethodByName("DownloadImage");
            var _params = new
            {
                Id = id,
                Etag = etag
            };

            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Case, Coalesce.Domain.Case>("Default");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<IntelliTect.Coalesce.Models.IFile>(itemResult);
            }
            var item = itemResult.Object;

            var _currentVaryValue = item.AttachmentHash;
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

            var _methodResult = item.DownloadImage(
                Db
            );
            if (_methodResult != null)
            {
                return File(_methodResult);
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Method: UploadAndDownload
        /// </summary>
        [HttpPost("UploadAndDownload")]
        [Authorize]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual async Task<ActionResult<ItemResult<IntelliTect.Coalesce.Models.IFile>>> UploadAndDownload(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromForm(Name = "id")] int id,
            Microsoft.AspNetCore.Http.IFormFile @file)
        {
            var _method = GeneratedForClassViewModel!.MethodByName("UploadAndDownload");
            var _params = new
            {
                Id = id,
                File = @file == null ? null : new IntelliTect.Coalesce.Models.File { Name = @file.FileName, ContentType = @file.ContentType, Length = @file.Length, Content = @file.OpenReadStream() }
            };

            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Case, Coalesce.Domain.Case>("Default");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<IntelliTect.Coalesce.Models.IFile>(itemResult);
            }
            var item = itemResult.Object;
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return new ItemResult<IntelliTect.Coalesce.Models.IFile>(_validationResult);
            }

            var _methodResult = await item.UploadAndDownload(
                Db,
                _params.File
            );
            if (_methodResult.Object != null)
            {
                return File(_methodResult.Object);
            }
            var _result = new ItemResult<IntelliTect.Coalesce.Models.IFile>(_methodResult);
            _result.Object = _methodResult.Object;
            return _result;
        }

        public class UploadAndDownloadParameters
        {
            public int Id { get; set; }
            public IntelliTect.Coalesce.Models.FileParameter File { get; set; }
        }

        /// <summary>
        /// Method: UploadAndDownload
        /// </summary>
        [HttpPost("UploadAndDownload")]
        [Authorize]
        [Consumes("application/json")]
        public virtual async Task<ActionResult<ItemResult<IntelliTect.Coalesce.Models.IFile>>> UploadAndDownload(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromBody] UploadAndDownloadParameters _params
        )
        {
            var _method = GeneratedForClassViewModel!.MethodByName("UploadAndDownload");
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Case, Coalesce.Domain.Case>("Default");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<IntelliTect.Coalesce.Models.IFile>(itemResult);
            }
            var item = itemResult.Object;
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return new ItemResult<IntelliTect.Coalesce.Models.IFile>(_validationResult);
            }

            var _methodResult = await item.UploadAndDownload(
                Db,
                _params.File
            );
            if (_methodResult.Object != null)
            {
                return File(_methodResult.Object);
            }
            var _result = new ItemResult<IntelliTect.Coalesce.Models.IFile>(_methodResult);
            _result.Object = _methodResult.Object;
            return _result;
        }

        /// <summary>
        /// Method: UploadImages
        /// </summary>
        [HttpPost("UploadImages")]
        [Authorize]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual async Task<ItemResult> UploadImages(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromForm(Name = "id")] int id,
            System.Collections.Generic.ICollection<Microsoft.AspNetCore.Http.IFormFile> files)
        {
            var _method = GeneratedForClassViewModel!.MethodByName("UploadImages");
            var _params = new
            {
                Id = id,
                Files = files == null ? null : files.Select(f => new IntelliTect.Coalesce.Models.File { Name = f.FileName, ContentType = f.ContentType, Length = f.Length, Content = f.OpenReadStream() }).ToList()
            };

            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Case, Coalesce.Domain.Case>("Default");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult(itemResult);
            }
            var item = itemResult.Object;
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return _validationResult;
            }

            await item.UploadImages(
                Db,
                _params.Files?.Cast<IntelliTect.Coalesce.Models.IFile>()?.ToList()
            );
            var _result = new ItemResult();
            return _result;
        }

        public class UploadImagesParameters
        {
            public int Id { get; set; }
            public ICollection<IntelliTect.Coalesce.Models.FileParameter> Files { get; set; }
        }

        /// <summary>
        /// Method: UploadImages
        /// </summary>
        [HttpPost("UploadImages")]
        [Authorize]
        [Consumes("application/json")]
        public virtual async Task<ItemResult> UploadImages(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromBody] UploadImagesParameters _params
        )
        {
            var _method = GeneratedForClassViewModel!.MethodByName("UploadImages");
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Case, Coalesce.Domain.Case>("Default");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult(itemResult);
            }
            var item = itemResult.Object;
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return _validationResult;
            }

            await item.UploadImages(
                Db,
                _params.Files?.Cast<IntelliTect.Coalesce.Models.IFile>()?.ToList()
            );
            var _result = new ItemResult();
            return _result;
        }

        /// <summary>
        /// Method: UploadByteArray
        /// </summary>
        [HttpPost("UploadByteArray")]
        [Authorize]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual async Task<ItemResult> UploadByteArray(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromForm(Name = "id")] int id,
            [FromForm(Name = "file")] byte[] @file)
        {
            var _method = GeneratedForClassViewModel!.MethodByName("UploadByteArray");
            var _params = new
            {
                Id = id,
                File = @file ?? await ((await Request.ReadFormAsync()).Files["file"]?.OpenReadStream().ReadAllBytesAsync(true) ?? Task.FromResult<byte[]>(null))
            };

            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Case, Coalesce.Domain.Case>("Default");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult(itemResult);
            }
            var item = itemResult.Object;
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return _validationResult;
            }

            item.UploadByteArray(
                Db,
                _params.File
            );
            var _result = new ItemResult();
            return _result;
        }

        public class UploadByteArrayParameters
        {
            public int Id { get; set; }
            public byte[] File { get; set; }
        }

        /// <summary>
        /// Method: UploadByteArray
        /// </summary>
        [HttpPost("UploadByteArray")]
        [Authorize]
        [Consumes("application/json")]
        public virtual async Task<ItemResult> UploadByteArray(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromBody] UploadByteArrayParameters _params
        )
        {
            var _method = GeneratedForClassViewModel!.MethodByName("UploadByteArray");
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Case, Coalesce.Domain.Case>("Default");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult(itemResult);
            }
            var item = itemResult.Object;
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return _validationResult;
            }

            item.UploadByteArray(
                Db,
                _params.File
            );
            var _result = new ItemResult();
            return _result;
        }

        /// <summary>
        /// Method: GetCaseSummary
        /// </summary>
        [HttpPost("GetCaseSummary")]
        [Authorize]
        public virtual ItemResult<CaseSummaryResponse> GetCaseSummary()
        {
            var _method = GeneratedForClassViewModel!.MethodByName("GetCaseSummary");
            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = Coalesce.Domain.Case.GetCaseSummary(
                Db
            );
            var _result = new ItemResult<CaseSummaryResponse>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.CaseSummary, CaseSummaryResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        }
    }
}
