
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
    [Route("api/Company")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class CompanyController
        : BaseApiController<Coalesce.Domain.Company, CompanyDtoGen, Coalesce.Domain.AppDbContext>
    {
        public CompanyController(Coalesce.Domain.AppDbContext db) : base(db)
        {
            GeneratedForClassViewModel = ReflectionRepository.Global.GetClassViewModel<Coalesce.Domain.Company>();
        }


        [HttpGet("get/{id}")]
        [Authorize]
        public virtual Task<ItemResult<CompanyDtoGen>> Get(int id, DataSourceParameters parameters, IDataSource<Coalesce.Domain.Company> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [Authorize]
        public virtual Task<ListResult<CompanyDtoGen>> List(ListParameters parameters, IDataSource<Coalesce.Domain.Company> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [Authorize]
        public virtual Task<int> Count(FilterParameters parameters, IDataSource<Coalesce.Domain.Company> dataSource)
            => CountImplementation(parameters, dataSource);


        [HttpPost("delete/{id}")]
        [Authorize]
        public virtual Task<ItemResult> Delete(int id, IBehaviors<Coalesce.Domain.Company> behaviors, IDataSource<Coalesce.Domain.Company> dataSource)
            => DeleteImplementation(id, new DataSourceParameters(), dataSource, behaviors);


        [HttpPost("save")]
        [Authorize]
        public virtual Task<ItemResult<CompanyDtoGen>> Save(CompanyDtoGen dto, [FromQuery] DataSourceParameters parameters, IDataSource<Coalesce.Domain.Company> dataSource, IBehaviors<Coalesce.Domain.Company> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

        /// <summary>
        /// Downloads CSV of CompanyDtoGen
        /// </summary>
        [HttpGet("csvDownload")]
        [Authorize]
        public virtual Task<FileResult> CsvDownload(ListParameters parameters, IDataSource<Coalesce.Domain.Company> dataSource)
            => CsvDownloadImplementation(parameters, dataSource);

        /// <summary>
        /// Returns CSV text of CompanyDtoGen
        /// </summary>
        [HttpGet("csvText")]
        [Authorize]
        public virtual Task<string> CsvText(ListParameters parameters, IDataSource<Coalesce.Domain.Company> dataSource)
            => CsvTextImplementation(parameters, dataSource);


        /// <summary>
        /// Saves CSV data as an uploaded file
        /// </summary>
        [HttpPost("csvUpload")]
        [Authorize]
        public virtual Task<IEnumerable<ItemResult>> CsvUpload(IFormFile file, IDataSource<Coalesce.Domain.Company> dataSource, IBehaviors<Coalesce.Domain.Company> behaviors, bool hasHeader = true)
            => CsvUploadImplementation(file, dataSource, behaviors, hasHeader);

        /// <summary>
        /// Saves CSV data as a posted string
        /// </summary>
        [HttpPost("csvSave")]
        [Authorize]
        public virtual Task<IEnumerable<ItemResult>> CsvSave(string csv, IDataSource<Coalesce.Domain.Company> dataSource, IBehaviors<Coalesce.Domain.Company> behaviors, bool hasHeader = true)
            => CsvSaveImplementation(csv, dataSource, behaviors, hasHeader);

        // Methods from data class exposed through API Controller.
    }
}
