
using Coalesce.Web.Models;
using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Data;
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
    public partial class CaseDtoController
    : LocalBaseApiController<Coalesce.Domain.Case, Coalesce.Domain.CaseDto>
    {
        public CaseDtoController(Coalesce.Domain.AppDbContext db) : base(db)
        {
        }


        [HttpGet("list")]
        [Authorize]
        public virtual Task<ListResult<Coalesce.Domain.CaseDto>> List(ListParameters parameters, IDataSource<Coalesce.Domain.Case> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [Authorize]
        public virtual Task<int> Count(FilterParameters parameters, IDataSource<Coalesce.Domain.Case> dataSource)
            => CountImplementation(parameters, dataSource);

        [HttpGet("propertyValues")]
        [Authorize]
        public virtual IEnumerable<string> PropertyValues(string property, int page = 1, string search = "")
            => PropertyValuesImplementation(property, page, search);

        [HttpGet("get/{id}")]
        [Authorize]
        public virtual Task<Coalesce.Domain.CaseDto> Get(int id, DataSourceParameters parameters, IDataSource<Coalesce.Domain.Case> dataSource)
            => GetImplementation(id, parameters, dataSource);


        [HttpPost("delete/{id}")]
        [Authorize]
        public virtual Task<ItemResult> Delete(int id, IBehaviors<Coalesce.Domain.Case> behaviors)
            => DeleteImplementation(id, behaviors);


        [HttpPost("save")]
        [Authorize]
        public virtual async Task<ItemResult<Coalesce.Domain.CaseDto>> Save(Coalesce.Domain.CaseDto dto, [FromQuery] DataSourceParameters parameters, IDataSource<Coalesce.Domain.Case> dataSource, IBehaviors<Coalesce.Domain.Case> behaviors)
        {

            if (dto.CaseId == 0 && !ClassViewModel.SecurityInfo.IsCreateAllowed(User))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "Create not allowed on CaseDto objects.";
            }
            else if (dto.CaseId != 0 && !ClassViewModel.SecurityInfo.IsEditAllowed(User))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "Edit not allowed on CaseDto objects.";
            }

            return await SaveImplementation(dto, parameters, dataSource, behaviors);
        }

        /// <summary>
        /// Downloads CSV of Coalesce.Domain.CaseDto
        /// </summary>
        [HttpGet("csvDownload")]
        [Authorize]
        public virtual async Task<FileResult> CsvDownload(ListParameters parameters, IDataSource<Coalesce.Domain.Case> dataSource)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(await CsvText(parameters, dataSource));
            return File(bytes, "application/x-msdownload", "Case.csv");
        }

        /// <summary>
        /// Returns CSV text of Coalesce.Domain.CaseDto
        /// </summary>
        [HttpGet("csvText")]
        [Authorize]
        public virtual async Task<string> CsvText(ListParameters parameters, IDataSource<Coalesce.Domain.Case> dataSource)
        {
            var listResult = await ListImplementation(parameters, dataSource);
            return IntelliTect.Coalesce.Helpers.CsvHelper.CreateCsv(listResult.List);
        }



        /// <summary>
        /// Saves CSV data as an uploaded file
        /// </summary>
        [HttpPost("CsvUpload")]
        [Authorize]
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
        [Authorize]
        public virtual async Task<IEnumerable<ItemResult>> CsvSave(
            string csv,
            IDataSource<Coalesce.Domain.Case> dataSource,
            IBehaviors<Coalesce.Domain.Case> behaviors,
            bool hasHeader = true)
        {
            // Get list from CSV
            var list = IntelliTect.Coalesce.Helpers.CsvHelper.ReadCsv<Coalesce.Domain.CaseDto>(csv, hasHeader);
            var resultList = new List<ItemResult>();
            foreach (var dto in list)
            {
                // Check if creates/edits aren't allowed
                if (dto.CaseId == 0 && !ClassViewModel.SecurityInfo.IsCreateAllowed(User))
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    resultList.Add("Create not allowed on CaseDto objects.");
                }
                else if (dto.CaseId != 0 && !ClassViewModel.SecurityInfo.IsEditAllowed(User))
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    resultList.Add("Edit not allowed on CaseDto objects.");
                }
                else
                {
                    var parameters = new DataSourceParameters() { Includes = "none" };
                    var result = await SaveImplementation(dto, parameters, dataSource, behaviors);
                    resultList.Add(new ItemResult { WasSuccessful = result.WasSuccessful, Message = result.Message });
                }
            }
            return resultList;
        }

        // Methods from data class exposed through API Controller.
    }
}
