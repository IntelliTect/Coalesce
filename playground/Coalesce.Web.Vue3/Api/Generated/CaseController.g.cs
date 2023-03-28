
using Coalesce.Web.Vue3.Models;
using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Api;
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
        : BaseApiController<Coalesce.Domain.Case, CaseDtoGen, Coalesce.Domain.AppDbContext>
    {
        public CaseController(CrudContext<Coalesce.Domain.AppDbContext> context) : base(context)
        {
            GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<Coalesce.Domain.Case>();
        }

        [HttpGet("get/{id}")]
        [AllowAnonymous]
        public virtual Task<ItemResult<CaseDtoGen>> Get(
            int id,
            DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.Case> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [AllowAnonymous]
        public virtual Task<ListResult<CaseDtoGen>> List(
            ListParameters parameters,
            IDataSource<Coalesce.Domain.Case> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [AllowAnonymous]
        public virtual Task<ItemResult<int>> Count(
            FilterParameters parameters,
            IDataSource<Coalesce.Domain.Case> dataSource)
            => CountImplementation(parameters, dataSource);

        [HttpPost("save")]
        [AllowAnonymous]
        public virtual Task<ItemResult<CaseDtoGen>> Save(
            [FromForm] CaseDtoGen dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.Case> dataSource,
            IBehaviors<Coalesce.Domain.Case> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

        [HttpPost("delete/{id}")]
        [Authorize]
        public virtual Task<ItemResult<CaseDtoGen>> Delete(
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
        public virtual ItemResult<System.Collections.Generic.ICollection<string>> GetCaseTitles(
            [FromForm(Name = "search")] string search)
        {
            var _params = new
            {
                search = search
            };

            if (Context.CoalesceOptions.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("GetCaseTitles"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<System.Collections.Generic.ICollection<string>>(_validationResult);
            }

            var _methodResult = Coalesce.Domain.Case.GetCaseTitles(
                Db,
                _params.search
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
        public virtual ItemResult<System.Collections.Generic.ICollection<CaseDtoGen>> GetSomeCases()
        {
            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(User);
            var _methodResult = Coalesce.Domain.Case.GetSomeCases(
                Db
            );
            var _result = new ItemResult<System.Collections.Generic.ICollection<CaseDtoGen>>();
            _result.Object = _methodResult?.ToList().Select(o => Mapper.MapToDto<Coalesce.Domain.Case, CaseDtoGen>(o, _mappingContext, includeTree)).ToList();
            return _result;
        }

        /// <summary>
        /// Method: GetAllOpenCasesCount
        /// </summary>
        [HttpPost("GetAllOpenCasesCount")]
        [Authorize]
        public virtual ItemResult<int> GetAllOpenCasesCount()
        {
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
        public virtual async Task<ItemResult> UploadImage(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromForm(Name = "id")] int id,
            Microsoft.AspNetCore.Http.IFormFile file)
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Case, Coalesce.Domain.Case>("Default");
            var (itemResult, _) = await dataSource.GetItemAsync(id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult(itemResult);
            }
            var item = itemResult.Object;
            var _params = new
            {
                file = file == null ? null : new IntelliTect.Coalesce.Models.File { Name = file.FileName, ContentType = file.ContentType, Length = file.Length, Content = file.OpenReadStream() }
            };

            if (Context.CoalesceOptions.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("UploadImage"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return _validationResult;
            }

            await item.UploadImage(
                Db,
                _params.file
            );
            await Db.SaveChangesAsync();
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
            int id,
            byte[] etag)
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Case, Coalesce.Domain.Case>("Default");
            var (itemResult, _) = await dataSource.GetItemAsync(id, new DataSourceParameters());
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
            await Db.SaveChangesAsync();
            if (_methodResult != null)
            {
                string _contentType = _methodResult.ContentType;
                if (string.IsNullOrWhiteSpace(_contentType) && (
                    string.IsNullOrWhiteSpace(_methodResult.Name) ||
                    !(new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider().TryGetContentType(_methodResult.Name, out _contentType))
                ))
                {
                    _contentType = "application/octet-stream";
                }
                return File(_methodResult.Content, _contentType, _methodResult.Name, !(_methodResult.Content is System.IO.MemoryStream));
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
        public virtual async Task<ActionResult<ItemResult<IntelliTect.Coalesce.Models.IFile>>> UploadAndDownload(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromForm(Name = "id")] int id,
            Microsoft.AspNetCore.Http.IFormFile file)
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Case, Coalesce.Domain.Case>("Default");
            var (itemResult, _) = await dataSource.GetItemAsync(id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<IntelliTect.Coalesce.Models.IFile>(itemResult);
            }
            var item = itemResult.Object;
            var _params = new
            {
                file = file == null ? null : new IntelliTect.Coalesce.Models.File { Name = file.FileName, ContentType = file.ContentType, Length = file.Length, Content = file.OpenReadStream() }
            };

            if (Context.CoalesceOptions.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("UploadAndDownload"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<IntelliTect.Coalesce.Models.IFile>(_validationResult);
            }

            var _methodResult = await item.UploadAndDownload(
                Db,
                _params.file
            );
            await Db.SaveChangesAsync();
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

        /// <summary>
        /// Method: UploadImages
        /// </summary>
        [HttpPost("UploadImages")]
        [Authorize]
        public virtual async Task<ItemResult> UploadImages(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromForm(Name = "id")] int id,
            System.Collections.Generic.ICollection<Microsoft.AspNetCore.Http.IFormFile> files)
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Case, Coalesce.Domain.Case>("Default");
            var (itemResult, _) = await dataSource.GetItemAsync(id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult(itemResult);
            }
            var item = itemResult.Object;
            var _params = new
            {
                files = files == null ? null : files.Select(f => (IntelliTect.Coalesce.Models.IFile)new IntelliTect.Coalesce.Models.File { Name = f.FileName, ContentType = f.ContentType, Length = f.Length, Content = f.OpenReadStream() }).ToList()
            };

            if (Context.CoalesceOptions.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("UploadImages"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return _validationResult;
            }

            await item.UploadImages(
                Db,
                _params.files.ToList()
            );
            await Db.SaveChangesAsync();
            var _result = new ItemResult();
            return _result;
        }

        /// <summary>
        /// Method: UploadByteArray
        /// </summary>
        [HttpPost("UploadByteArray")]
        [Authorize]
        public virtual async Task<ItemResult> UploadByteArray(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromForm(Name = "id")] int id,
            [FromForm(Name = "file")] byte[] file)
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Case, Coalesce.Domain.Case>("Default");
            var (itemResult, _) = await dataSource.GetItemAsync(id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult(itemResult);
            }
            var item = itemResult.Object;
            var _params = new
            {
                file = file ?? await ((await Request.ReadFormAsync()).Files[nameof(file)]?.OpenReadStream().ReadAllBytesAsync(true) ?? Task.FromResult<byte[]>(null))
            };

            if (Context.CoalesceOptions.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("UploadByteArray"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return _validationResult;
            }

            item.UploadByteArray(
                _params.file
            );
            await Db.SaveChangesAsync();
            var _result = new ItemResult();
            return _result;
        }

        /// <summary>
        /// Method: GetCaseSummary
        /// </summary>
        [HttpPost("GetCaseSummary")]
        [Authorize]
        public virtual ItemResult<CaseSummaryDtoGen> GetCaseSummary()
        {
            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(User);
            var _methodResult = Coalesce.Domain.Case.GetCaseSummary(
                Db
            );
            var _result = new ItemResult<CaseSummaryDtoGen>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.CaseSummary, CaseSummaryDtoGen>(_methodResult, _mappingContext, includeTree);
            return _result;
        }
    }
}
