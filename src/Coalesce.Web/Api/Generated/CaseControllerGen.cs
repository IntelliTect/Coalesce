
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
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Coalesce.Web.Api
{
    [Route("api/[controller]")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class CaseController
    : LocalBaseApiController<Coalesce.Domain.Case, CaseDtoGen>
    {
        public CaseController(Coalesce.Domain.AppDbContext db) : base(db)
        {
            GeneratedForClassViewModel = ReflectionRepository.Global.GetClassViewModel<Coalesce.Domain.Case>();
        }


        [HttpGet("get/{id}")]
        [AllowAnonymous]
        public virtual Task<CaseDtoGen> Get(int id, DataSourceParameters parameters, IDataSource<Coalesce.Domain.Case> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [AllowAnonymous]
        public virtual Task<ListResult<CaseDtoGen>> List(ListParameters parameters, IDataSource<Coalesce.Domain.Case> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [AllowAnonymous]
        public virtual Task<int> Count(FilterParameters parameters, IDataSource<Coalesce.Domain.Case> dataSource)
            => CountImplementation(parameters, dataSource);


        [HttpPost("delete/{id}")]
        [Authorize]
        public virtual Task<ItemResult> Delete(int id, IBehaviors<Coalesce.Domain.Case> behaviors)
            => DeleteImplementation(id, behaviors);


        [HttpPost("save")]
        [AllowAnonymous]
        public virtual Task<ItemResult<CaseDtoGen>> Save(CaseDtoGen dto, [FromQuery] DataSourceParameters parameters, IDataSource<Coalesce.Domain.Case> dataSource, IBehaviors<Coalesce.Domain.Case> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

        /// <summary>
        /// Downloads CSV of CaseDtoGen
        /// </summary>
        [HttpGet("csvDownload")]
        [AllowAnonymous]
        public virtual async Task<FileResult> CsvDownload(ListParameters parameters, IDataSource<Coalesce.Domain.Case> dataSource)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(await CsvText(parameters, dataSource));
            return File(bytes, "application/x-msdownload", "Case.csv");
        }

        /// <summary>
        /// Returns CSV text of CaseDtoGen
        /// </summary>
        [HttpGet("csvText")]
        [AllowAnonymous]
        public virtual async Task<string> CsvText(ListParameters parameters, IDataSource<Coalesce.Domain.Case> dataSource)
        {
            var listResult = await ListImplementation(parameters, dataSource);
            return IntelliTect.Coalesce.Helpers.CsvHelper.CreateCsv(listResult.List);
        }



        /// <summary>
        /// Saves CSV data as an uploaded file
        /// </summary>
        [HttpPost("CsvUpload")]
        [AllowAnonymous]
        public virtual async Task<IEnumerable<ItemResult>> CsvUpload(
            Microsoft.AspNetCore.Http.IFormFile file,
            IDataSource<Coalesce.Domain.Case> dataSource,
            IBehaviors<Coalesce.Domain.Case> behaviors,
            bool hasHeader = true)
        {
            if (file == null || file.Length == 0) throw new ArgumentException("No files uploaded");

            using (var stream = file.OpenReadStream())
            {
                using (var reader = new System.IO.StreamReader(stream))
                {
                    var csv = await reader.ReadToEndAsync();
                    return await CsvSave(csv, dataSource, behaviors, hasHeader);
                }
            }
        }

        /// <summary>
        /// Saves CSV data as a posted string
        /// </summary>
        [HttpPost("CsvSave")]
        [AllowAnonymous]
        public virtual async Task<IEnumerable<ItemResult>> CsvSave(
            string csv,
            IDataSource<Coalesce.Domain.Case> dataSource,
            IBehaviors<Coalesce.Domain.Case> behaviors,
            bool hasHeader = true)
        {
            // Get list from CSV
            var list = IntelliTect.Coalesce.Helpers.CsvHelper.ReadCsv<CaseDtoGen>(csv, hasHeader);
            var resultList = new List<ItemResult>();
            foreach (var dto in list)
            {
                var parameters = new DataSourceParameters() { Includes = "none" };

                // Security: SaveImplementation is responsible for checking specific save/edit attribute permissions.
                var result = await SaveImplementation(dto, parameters, dataSource, behaviors);
                resultList.Add(new ItemResult { WasSuccessful = result.WasSuccessful, Message = result.Message });
            }
            return resultList;
        }

        // Methods from data class exposed through API Controller.

        /// <summary>
        /// Method: GetAllOpenCasesCount
        /// </summary>
        [HttpPost("GetAllOpenCasesCount")]

        public virtual ItemResult<int> GetAllOpenCasesCount([FromServices] IDataSourceFactory dataSourceFactory)
        {
            var dataSource = dataSourceFactory.GetDefaultDataSource<Coalesce.Domain.Case>();
            var result = new ItemResult<int>();
            try
            {
                var objResult = Coalesce.Domain.Case.GetAllOpenCasesCount(Db);
                result.Object = objResult;

                result.WasSuccessful = true;
                result.Message = null;
            }
            catch (Exception ex)
            {
                result.WasSuccessful = false;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Method: RandomizeDatesAndStatus
        /// </summary>
        [HttpPost("RandomizeDatesAndStatus")]

        public virtual ItemResult<object> RandomizeDatesAndStatus([FromServices] IDataSourceFactory dataSourceFactory)
        {
            var dataSource = dataSourceFactory.GetDefaultDataSource<Coalesce.Domain.Case>();
            var result = new ItemResult<object>();
            try
            {
                object objResult = null;
                Coalesce.Domain.Case.RandomizeDatesAndStatus(Db);
                result.Object = objResult;

                result.WasSuccessful = true;
                result.Message = null;
            }
            catch (Exception ex)
            {
                result.WasSuccessful = false;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Method: GetCaseSummary
        /// </summary>
        [HttpPost("GetCaseSummary")]

        public virtual ItemResult<CaseSummaryDtoGen> GetCaseSummary([FromServices] IDataSourceFactory dataSourceFactory)
        {
            var dataSource = dataSourceFactory.GetDefaultDataSource<Coalesce.Domain.Case>();
            var result = new ItemResult<CaseSummaryDtoGen>();
            try
            {
                IncludeTree includeTree = null;
                var objResult = Coalesce.Domain.Case.GetCaseSummary(Db);
                var mappingContext = new MappingContext(User, "");
                result.Object = Mapper.MapToDto<Coalesce.Domain.CaseSummary, CaseSummaryDtoGen>(objResult, mappingContext, includeTree);

                result.WasSuccessful = true;
                result.Message = null;
            }
            catch (Exception ex)
            {
                result.WasSuccessful = false;
                result.Message = ex.Message;
            }
            return result;
        }
    }
}
