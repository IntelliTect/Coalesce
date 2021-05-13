
using Coalesce.Web.Models;
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

namespace Coalesce.Web.Api
{
    [Route("api/Case")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class CaseController
        : BaseApiController<Coalesce.Domain.Case, CaseDtoGen, Coalesce.Domain.AppDbContext>
    {
        public CaseController(Coalesce.Domain.AppDbContext db) : base(db)
        {
            GeneratedForClassViewModel = ReflectionRepository.Global.GetClassViewModel<Coalesce.Domain.Case>();
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
            CaseDtoGen dto,
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
        /// Method: GetSomeCases
        /// </summary>
        [HttpPost("GetSomeCases")]
        [Authorize]
        public virtual ItemResult<System.Collections.Generic.ICollection<CaseDtoGen>> GetSomeCases()
        {
            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(User);
            var _methodResult = Coalesce.Domain.Case.GetSomeCases(Db);
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
            var _methodResult = Coalesce.Domain.Case.GetAllOpenCasesCount(Db);
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
            Coalesce.Domain.Case.RandomizeDatesAndStatus(Db);
            var _result = new ItemResult();
            return _result;
        }

        /// <summary>
        /// Method: UploadAttachment
        /// </summary>
        [HttpPost("UploadAttachment")]
        [Authorize]
        public virtual async Task<ItemResult> UploadAttachment([FromServices] IDataSourceFactory dataSourceFactory, int id, Microsoft.AspNetCore.Http.IFormFile file)
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Case, Coalesce.Domain.Case>("Default");
            var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult(itemResult);
            }
            var item = itemResult.Object;
            await item.UploadAttachment(file == null ? null : new File { Name = file.FileName, ContentType = file.ContentType, Length = file.Length, Content = file.OpenReadStream() });
            await Db.SaveChangesAsync();
            var _result = new ItemResult();
            return _result;
        }

        /// <summary>
        /// Method: UploadByteArray
        /// </summary>
        [HttpPost("UploadByteArray")]
        [Authorize]
        public virtual async Task<ItemResult> UploadByteArray([FromServices] IDataSourceFactory dataSourceFactory, int id, byte[] file)
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Case, Coalesce.Domain.Case>("Default");
            var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult(itemResult);
            }
            var item = itemResult.Object;
            item.UploadByteArray(file ?? await ((await Request.ReadFormAsync()).Files[nameof(file)]?.OpenReadStream().ReadAllBytesAsync(true) ?? Task.FromResult<byte[]>(null)));
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
            var _methodResult = Coalesce.Domain.Case.GetCaseSummary(Db);
            var _result = new ItemResult<CaseSummaryDtoGen>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.CaseSummary, CaseSummaryDtoGen>(_methodResult, _mappingContext, includeTree);
            return _result;
        }

        /// <summary>
        /// File Download: Image
        /// </summary>
        [Authorize]
        [HttpGet("Image")]
        public virtual async Task<IActionResult> ImageGet(int id, IDataSource<Coalesce.Domain.Case> dataSource)
        {
            var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());
            if (itemResult.Object?.Image == null) return NotFound();
            string contentType = "image/jpeg";
            if (!(new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider().TryGetContentType(itemResult.Object.ImageName, out contentType)))
            {
                contentType = "image/jpeg";
            }
            return File(itemResult.Object.Image, contentType, itemResult.Object.ImageName);
        }

        /// <summary>
        /// File Upload: Image
        /// </summary>
        [Authorize]
        [HttpPut("Image")]
        public virtual async Task<ItemResult<CaseDtoGen>> ImagePut(int id, IFormFile file, IDataSource<Coalesce.Domain.Case> dataSource, IBehaviors<Coalesce.Domain.Case> behaviors)
        {
            var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());
            if (!itemResult.WasSuccessful) return new ItemResult<CaseDtoGen>(itemResult);
            using (var stream = new System.IO.MemoryStream())
            {
                await file.CopyToAsync(stream);
                itemResult.Object.Image = stream.ToArray();
                using (var sha256Hash = System.Security.Cryptography.SHA256.Create())
                {
                    var hash = sha256Hash.ComputeHash(itemResult.Object.Image);
                    itemResult.Object.ImageHash = Convert.ToBase64String(hash);
                }
                itemResult.Object.ImageSize = file.Length;
                await Db.SaveChangesAsync();
            }
            var result = new ItemResult<CaseDtoGen>();
            var mappingContext = new MappingContext(User);
            result.Object = Mapper.MapToDto<Coalesce.Domain.Case, CaseDtoGen>(itemResult.Object, mappingContext, null);
            return result;
        }

        /// <summary>
        /// File Delete: Image
        /// </summary>
        [Authorize]
        [HttpDelete("Image")]
        public virtual async Task<IActionResult> ImageDelete(int id, IDataSource<Coalesce.Domain.Case> dataSource, IBehaviors<Coalesce.Domain.Case> behaviors)
        {
            var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());
            if (!itemResult.WasSuccessful) return NotFound();
            itemResult.Object.ImageHash = null;
            itemResult.Object.ImageSize = 0;
            itemResult.Object.Image = null;
            await Db.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// File Download: PlainAttachment
        /// </summary>
        [AllowAnonymous]
        [HttpGet("PlainAttachment")]
        public virtual async Task<IActionResult> PlainAttachmentGet(int id, IDataSource<Coalesce.Domain.Case> dataSource)
        {
            var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());
            if (itemResult.Object?.PlainAttachment == null) return NotFound();
            string contentType = "text/plain";
            return File(itemResult.Object.PlainAttachment, contentType);
        }

        /// <summary>
        /// File Upload: PlainAttachment
        /// </summary>
        [Authorize]
        [HttpPut("PlainAttachment")]
        public virtual async Task<ItemResult<CaseDtoGen>> PlainAttachmentPut(int id, IFormFile file, IDataSource<Coalesce.Domain.Case> dataSource, IBehaviors<Coalesce.Domain.Case> behaviors)
        {
            var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());
            if (!itemResult.WasSuccessful) return new ItemResult<CaseDtoGen>(itemResult);
            using (var stream = new System.IO.MemoryStream())
            {
                await file.CopyToAsync(stream);
                itemResult.Object.PlainAttachment = stream.ToArray();
                await Db.SaveChangesAsync();
            }
            var result = new ItemResult<CaseDtoGen>();
            var mappingContext = new MappingContext(User);
            result.Object = Mapper.MapToDto<Coalesce.Domain.Case, CaseDtoGen>(itemResult.Object, mappingContext, null);
            return result;
        }

        /// <summary>
        /// File Delete: PlainAttachment
        /// </summary>
        [Authorize]
        [HttpDelete("PlainAttachment")]
        public virtual async Task<IActionResult> PlainAttachmentDelete(int id, IDataSource<Coalesce.Domain.Case> dataSource, IBehaviors<Coalesce.Domain.Case> behaviors)
        {
            var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());
            if (!itemResult.WasSuccessful) return NotFound();
            itemResult.Object.PlainAttachment = null;
            await Db.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// File Download: RestrictedUploadAttachment
        /// </summary>
        [Authorize]
        [HttpGet("RestrictedUploadAttachment")]
        public virtual async Task<IActionResult> RestrictedUploadAttachmentGet(int id, IDataSource<Coalesce.Domain.Case> dataSource)
        {
            var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());
            if (itemResult.Object?.RestrictedUploadAttachment == null) return NotFound();
            string contentType = "application/octet-stream";
            return File(itemResult.Object.RestrictedUploadAttachment, contentType);
        }

        /// <summary>
        /// File Upload: RestrictedUploadAttachment
        /// </summary>
        [Authorize(Roles = "Admin,SuperUser")]
        [HttpPut("RestrictedUploadAttachment")]
        public virtual async Task<ItemResult<CaseDtoGen>> RestrictedUploadAttachmentPut(int id, IFormFile file, IDataSource<Coalesce.Domain.Case> dataSource, IBehaviors<Coalesce.Domain.Case> behaviors)
        {
            var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());
            if (!itemResult.WasSuccessful) return new ItemResult<CaseDtoGen>(itemResult);
            using (var stream = new System.IO.MemoryStream())
            {
                await file.CopyToAsync(stream);
                itemResult.Object.RestrictedUploadAttachment = stream.ToArray();
                await Db.SaveChangesAsync();
            }
            var result = new ItemResult<CaseDtoGen>();
            var mappingContext = new MappingContext(User);
            result.Object = Mapper.MapToDto<Coalesce.Domain.Case, CaseDtoGen>(itemResult.Object, mappingContext, null);
            return result;
        }

        /// <summary>
        /// File Delete: RestrictedUploadAttachment
        /// </summary>
        [Authorize]
        [HttpDelete("RestrictedUploadAttachment")]
        public virtual async Task<IActionResult> RestrictedUploadAttachmentDelete(int id, IDataSource<Coalesce.Domain.Case> dataSource, IBehaviors<Coalesce.Domain.Case> behaviors)
        {
            var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());
            if (!itemResult.WasSuccessful) return NotFound();
            itemResult.Object.RestrictedUploadAttachment = null;
            await Db.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// File Download: RestrictedDownloadAttachment
        /// </summary>
        [Authorize(Roles = "Admin,OtherRole")]
        [HttpGet("RestrictedDownloadAttachment")]
        public virtual async Task<IActionResult> RestrictedDownloadAttachmentGet(int id, IDataSource<Coalesce.Domain.Case> dataSource)
        {
            var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());
            if (itemResult.Object?.RestrictedDownloadAttachment == null) return NotFound();
            string contentType = "application/octet-stream";
            return File(itemResult.Object.RestrictedDownloadAttachment, contentType);
        }

        /// <summary>
        /// File Upload: RestrictedDownloadAttachment
        /// </summary>
        [Authorize]
        [HttpPut("RestrictedDownloadAttachment")]
        public virtual async Task<ItemResult<CaseDtoGen>> RestrictedDownloadAttachmentPut(int id, IFormFile file, IDataSource<Coalesce.Domain.Case> dataSource, IBehaviors<Coalesce.Domain.Case> behaviors)
        {
            var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());
            if (!itemResult.WasSuccessful) return new ItemResult<CaseDtoGen>(itemResult);
            using (var stream = new System.IO.MemoryStream())
            {
                await file.CopyToAsync(stream);
                itemResult.Object.RestrictedDownloadAttachment = stream.ToArray();
                await Db.SaveChangesAsync();
            }
            var result = new ItemResult<CaseDtoGen>();
            var mappingContext = new MappingContext(User);
            result.Object = Mapper.MapToDto<Coalesce.Domain.Case, CaseDtoGen>(itemResult.Object, mappingContext, null);
            return result;
        }

        /// <summary>
        /// File Delete: RestrictedDownloadAttachment
        /// </summary>
        [Authorize]
        [HttpDelete("RestrictedDownloadAttachment")]
        public virtual async Task<IActionResult> RestrictedDownloadAttachmentDelete(int id, IDataSource<Coalesce.Domain.Case> dataSource, IBehaviors<Coalesce.Domain.Case> behaviors)
        {
            var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());
            if (!itemResult.WasSuccessful) return NotFound();
            itemResult.Object.RestrictedDownloadAttachment = null;
            await Db.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// File Download: RestrictedMetaAttachment
        /// </summary>
        [Authorize]
        [HttpGet("RestrictedMetaAttachment")]
        public virtual async Task<IActionResult> RestrictedMetaAttachmentGet(int id, IDataSource<Coalesce.Domain.Case> dataSource)
        {
            var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());
            if (itemResult.Object?.RestrictedMetaAttachment == null) return NotFound();
            string contentType = "application/octet-stream";
            if (!(new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider().TryGetContentType(itemResult.Object.InternalUseFileName, out contentType)))
            {
                contentType = "application/octet-stream";
            }
            return File(itemResult.Object.RestrictedMetaAttachment, contentType, itemResult.Object.InternalUseFileName);
        }

        /// <summary>
        /// File Upload: RestrictedMetaAttachment
        /// </summary>
        [Authorize]
        [HttpPut("RestrictedMetaAttachment")]
        public virtual async Task<ItemResult<CaseDtoGen>> RestrictedMetaAttachmentPut(int id, IFormFile file, IDataSource<Coalesce.Domain.Case> dataSource, IBehaviors<Coalesce.Domain.Case> behaviors)
        {
            var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());
            if (!itemResult.WasSuccessful) return new ItemResult<CaseDtoGen>(itemResult);
            using (var stream = new System.IO.MemoryStream())
            {
                await file.CopyToAsync(stream);
                itemResult.Object.RestrictedMetaAttachment = stream.ToArray();
                itemResult.Object.InternalUseFileName = file.FileName;
                itemResult.Object.InternalUseFileSize = file.Length;
                await Db.SaveChangesAsync();
            }
            var result = new ItemResult<CaseDtoGen>();
            var mappingContext = new MappingContext(User);
            result.Object = Mapper.MapToDto<Coalesce.Domain.Case, CaseDtoGen>(itemResult.Object, mappingContext, null);
            return result;
        }

        /// <summary>
        /// File Delete: RestrictedMetaAttachment
        /// </summary>
        [Authorize]
        [HttpDelete("RestrictedMetaAttachment")]
        public virtual async Task<IActionResult> RestrictedMetaAttachmentDelete(int id, IDataSource<Coalesce.Domain.Case> dataSource, IBehaviors<Coalesce.Domain.Case> behaviors)
        {
            var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());
            if (!itemResult.WasSuccessful) return NotFound();
            itemResult.Object.InternalUseFileName = null;
            itemResult.Object.InternalUseFileSize = 0;
            itemResult.Object.RestrictedMetaAttachment = null;
            await Db.SaveChangesAsync();
            return Ok();
        }
    }
}
