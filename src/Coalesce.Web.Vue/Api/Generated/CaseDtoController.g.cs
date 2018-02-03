
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
    [Route("api/CaseDto")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class CaseDtoController
        : BaseApiController<Coalesce.Domain.Case, Coalesce.Domain.CaseDto, Coalesce.Domain.AppDbContext>
    {
        public CaseDtoController(Coalesce.Domain.AppDbContext db) : base(db)
        {
            GeneratedForClassViewModel = ReflectionRepository.Global.GetClassViewModel<Coalesce.Domain.CaseDto>();
        }


        [HttpGet("get/{id}")]
        [Authorize]
        public virtual Task<ItemResult<Coalesce.Domain.CaseDto>> Get(
            int id,
            DataSourceParameters parameters,
            [DeclaredFor(typeof(Coalesce.Domain.CaseDto))] IDataSource<Coalesce.Domain.Case> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [Authorize]
        public virtual Task<ListResult<Coalesce.Domain.CaseDto>> List(
            ListParameters parameters,
            [DeclaredFor(typeof(Coalesce.Domain.CaseDto))] IDataSource<Coalesce.Domain.Case> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [Authorize]
        public virtual Task<int> Count(
            FilterParameters parameters,
            [DeclaredFor(typeof(Coalesce.Domain.CaseDto))] IDataSource<Coalesce.Domain.Case> dataSource)
            => CountImplementation(parameters, dataSource);


        [HttpPost("delete/{id}")]
        [Authorize]
        public virtual Task<ItemResult> Delete(
            int id,
            [DeclaredFor(typeof(Coalesce.Domain.CaseDto))] IBehaviors<Coalesce.Domain.Case> behaviors,
            [DeclaredFor(typeof(Coalesce.Domain.CaseDto))] IDataSource<Coalesce.Domain.Case> dataSource)
            => DeleteImplementation(id, new DataSourceParameters(), dataSource, behaviors);


        [HttpPost("save")]
        [Authorize]
        public virtual Task<ItemResult<Coalesce.Domain.CaseDto>> Save(
            Coalesce.Domain.CaseDto dto,
            [FromQuery] DataSourceParameters parameters,
            [DeclaredFor(typeof(Coalesce.Domain.CaseDto))] IDataSource<Coalesce.Domain.Case> dataSource,
            [DeclaredFor(typeof(Coalesce.Domain.CaseDto))] IBehaviors<Coalesce.Domain.Case> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

        /// <summary>
        /// Downloads CSV of Coalesce.Domain.CaseDto
        /// </summary>
        [HttpGet("csvDownload")]
        [Authorize]
        public virtual Task<FileResult> CsvDownload(
            ListParameters parameters,
            [DeclaredFor(typeof(Coalesce.Domain.CaseDto))] IDataSource<Coalesce.Domain.Case> dataSource)
            => CsvDownloadImplementation(parameters, dataSource);

        /// <summary>
        /// Returns CSV text of Coalesce.Domain.CaseDto
        /// </summary>
        [HttpGet("csvText")]
        [Authorize]
        public virtual Task<string> CsvText(
            ListParameters parameters,
            [DeclaredFor(typeof(Coalesce.Domain.CaseDto))] IDataSource<Coalesce.Domain.Case> dataSource)
            => CsvTextImplementation(parameters, dataSource);


        /// <summary>
        /// Saves CSV data as an uploaded file
        /// </summary>
        [HttpPost("csvUpload")]
        [Authorize]
        public virtual Task<IEnumerable<ItemResult>> CsvUpload(
            IFormFile file,
            [DeclaredFor(typeof(Coalesce.Domain.CaseDto))] IDataSource<Coalesce.Domain.Case> dataSource,
            [DeclaredFor(typeof(Coalesce.Domain.CaseDto))] IBehaviors<Coalesce.Domain.Case> behaviors,
            bool hasHeader = true)
            => CsvUploadImplementation(file, dataSource, behaviors, hasHeader);

        /// <summary>
        /// Saves CSV data as a posted string
        /// </summary>
        [HttpPost("csvSave")]
        [Authorize]
        public virtual Task<IEnumerable<ItemResult>> CsvSave(
            string csv,
            [DeclaredFor(typeof(Coalesce.Domain.CaseDto))] IDataSource<Coalesce.Domain.Case> dataSource,
            [DeclaredFor(typeof(Coalesce.Domain.CaseDto))] IBehaviors<Coalesce.Domain.Case> behaviors,
            bool hasHeader = true)
            => CsvSaveImplementation(csv, dataSource, behaviors, hasHeader);

        // Methods from data class exposed through API Controller.
    }
}
