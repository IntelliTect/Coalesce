
using Coalesce.Web.Vue.Models;
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

namespace Coalesce.Web.Vue.Api
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

        /// <summary>
        /// Downloads CSV of CaseDtoGen
        /// </summary>
        [HttpGet("csvDownload")]
        [AllowAnonymous]
        public virtual Task<FileResult> CsvDownload(
            ListParameters parameters,
            IDataSource<Coalesce.Domain.Case> dataSource)
            => CsvDownloadImplementation(parameters, dataSource);

        /// <summary>
        /// Returns CSV text of CaseDtoGen
        /// </summary>
        [HttpGet("csvText")]
        [AllowAnonymous]
        public virtual Task<string> CsvText(
            ListParameters parameters,
            IDataSource<Coalesce.Domain.Case> dataSource)
            => CsvTextImplementation(parameters, dataSource);

        /// <summary>
        /// Saves CSV data as an uploaded file
        /// </summary>
        [HttpPost("csvUpload")]
        [AllowAnonymous]
        public virtual Task<IEnumerable<ItemResult>> CsvUpload(
            IFormFile file,
            IDataSource<Coalesce.Domain.Case> dataSource,
            IBehaviors<Coalesce.Domain.Case> behaviors,
            bool hasHeader = true)
            => CsvUploadImplementation(file, dataSource, behaviors, hasHeader);

        /// <summary>
        /// Saves CSV data as a posted string
        /// </summary>
        [HttpPost("csvSave")]
        [AllowAnonymous]
        public virtual Task<IEnumerable<ItemResult>> CsvSave(
            string csv,
            IDataSource<Coalesce.Domain.Case> dataSource,
            IBehaviors<Coalesce.Domain.Case> behaviors,
            bool hasHeader = true)
            => CsvSaveImplementation(csv, dataSource, behaviors, hasHeader);

        // Methods from data class exposed through API Controller.

        /// <summary>
        /// Method: GetSomeCases
        /// </summary>
        [HttpPost("GetSomeCases")]
        [Authorize]
        public virtual ItemResult<ICollection<CaseDtoGen>> GetSomeCases()
        {
            IncludeTree includeTree = null;
            var methodResult = Coalesce.Domain.Case.GetSomeCases(Db);
            var result = new ItemResult<ICollection<CaseDtoGen>>();
            var mappingContext = new MappingContext(User, "");
            result.Object = methodResult?.ToList().Select(o => Mapper.MapToDto<Coalesce.Domain.Case, CaseDtoGen>(o, mappingContext, includeTree)).ToList();
            return result;
        }

        /// <summary>
        /// Method: GetAllOpenCasesCount
        /// </summary>
        [HttpPost("GetAllOpenCasesCount")]
        [Authorize]
        public virtual ItemResult<int> GetAllOpenCasesCount()
        {
            var methodResult = Coalesce.Domain.Case.GetAllOpenCasesCount(Db);
            var result = new ItemResult<int>();
            result.Object = methodResult;
            return result;
        }

        /// <summary>
        /// Method: RandomizeDatesAndStatus
        /// </summary>
        [HttpPost("RandomizeDatesAndStatus")]
        [Authorize]
        public virtual ItemResult RandomizeDatesAndStatus()
        {
            Coalesce.Domain.Case.RandomizeDatesAndStatus(Db);
            var result = new ItemResult();
            return result;
        }

        /// <summary>
        /// Method: GetCaseSummary
        /// </summary>
        [HttpPost("GetCaseSummary")]
        [Authorize]
        public virtual ItemResult<CaseSummaryDtoGen> GetCaseSummary()
        {
            IncludeTree includeTree = null;
            var methodResult = Coalesce.Domain.Case.GetCaseSummary(Db);
            var result = new ItemResult<CaseSummaryDtoGen>();
            var mappingContext = new MappingContext(User, "");
            result.Object = Mapper.MapToDto<Coalesce.Domain.CaseSummary, CaseSummaryDtoGen>(methodResult, mappingContext, includeTree);
            return result;
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
        /// File Download: Attachment
        /// </summary>
        [Authorize]
        [HttpGet("Attachment")]
        public virtual async Task<IActionResult> AttachmentGet(int id, IDataSource<Coalesce.Domain.Case> dataSource)
        {
            var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());
            if (itemResult.Object?.Attachment == null) return NotFound();
            string contentType = "application/octet-stream";
            if (!(new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider().TryGetContentType(itemResult.Object.AttachmentName, out contentType)))
            {
                contentType = "application/octet-stream";
            }
            return File(itemResult.Object.Attachment, contentType, itemResult.Object.AttachmentName);
        }

        /// <summary>
        /// File Upload: Attachment
        /// </summary>
        [Authorize]
        [HttpPut("Attachment")]
        public virtual async Task<ItemResult<CaseDtoGen>> AttachmentPut(int id, IFormFile file, IDataSource<Coalesce.Domain.Case> dataSource, IBehaviors<Coalesce.Domain.Case> behaviors)
        {
            var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());
            if (!itemResult.WasSuccessful) return new ItemResult<CaseDtoGen>(itemResult);
            using (var stream = new System.IO.MemoryStream())
            {
                await file.CopyToAsync(stream);
                itemResult.Object.Attachment = stream.ToArray();
                itemResult.Object.AttachmentName = file.FileName;
                await Db.SaveChangesAsync();
            }
            var result = new ItemResult<CaseDtoGen>();
            var mappingContext = new MappingContext(User);
            result.Object = Mapper.MapToDto<Coalesce.Domain.Case, CaseDtoGen>(itemResult.Object, mappingContext, null);
            return result;
        }

        /// <summary>
        /// File Delete: Attachment
        /// </summary>
        [Authorize]
        [HttpDelete("Attachment")]
        public virtual async Task<IActionResult> AttachmentDelete(int id, IDataSource<Coalesce.Domain.Case> dataSource, IBehaviors<Coalesce.Domain.Case> behaviors)
        {
            var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());
            if (!itemResult.WasSuccessful) return NotFound();
            itemResult.Object.AttachmentName = null;
            itemResult.Object.Attachment = null;
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
        [Authorize(Roles = "Admin, SuperUser")]
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
        [Authorize(Roles = "Admin")]
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
