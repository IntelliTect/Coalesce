
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
    public partial class CaseProductController
        : BaseApiController<Coalesce.Domain.CaseProduct, CaseProductDtoGen, Coalesce.Domain.AppDbContext>
    {
        public CaseProductController(Coalesce.Domain.AppDbContext db) : base(db)
        {
            GeneratedForClassViewModel = ReflectionRepository.Global.GetClassViewModel<Coalesce.Domain.CaseProduct>();
        }


        [HttpGet("get/{id}")]
        [Authorize]
        public virtual Task<CaseProductDtoGen> Get(int id, DataSourceParameters parameters, IDataSource<Coalesce.Domain.CaseProduct> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [Authorize]
        public virtual Task<ListResult<CaseProductDtoGen>> List(ListParameters parameters, IDataSource<Coalesce.Domain.CaseProduct> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [Authorize]
        public virtual Task<int> Count(FilterParameters parameters, IDataSource<Coalesce.Domain.CaseProduct> dataSource)
            => CountImplementation(parameters, dataSource);


        [HttpPost("delete/{id}")]
        [Authorize]
        public virtual Task<ItemResult> Delete(int id, IBehaviors<Coalesce.Domain.CaseProduct> behaviors)
            => DeleteImplementation(id, behaviors);


        [HttpPost("save")]
        [Authorize]
        public virtual Task<ItemResult<CaseProductDtoGen>> Save(CaseProductDtoGen dto, [FromQuery] DataSourceParameters parameters, IDataSource<Coalesce.Domain.CaseProduct> dataSource, IBehaviors<Coalesce.Domain.CaseProduct> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

        /// <summary>
        /// Downloads CSV of CaseProductDtoGen
        /// </summary>
        [HttpGet("csvDownload")]
        [Authorize]
        public virtual async Task<FileResult> CsvDownload(ListParameters parameters, IDataSource<Coalesce.Domain.CaseProduct> dataSource)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(await CsvText(parameters, dataSource));
            return File(bytes, "application/x-msdownload", "CaseProduct.csv");
        }

        /// <summary>
        /// Returns CSV text of CaseProductDtoGen
        /// </summary>
        [HttpGet("csvText")]
        [Authorize]
        public virtual async Task<string> CsvText(ListParameters parameters, IDataSource<Coalesce.Domain.CaseProduct> dataSource)
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
            IDataSource<Coalesce.Domain.CaseProduct> dataSource,
            IBehaviors<Coalesce.Domain.CaseProduct> behaviors,
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
            IDataSource<Coalesce.Domain.CaseProduct> dataSource,
            IBehaviors<Coalesce.Domain.CaseProduct> behaviors,
            bool hasHeader = true)
        {
            // Get list from CSV
            var list = IntelliTect.Coalesce.Helpers.CsvHelper.ReadCsv<CaseProductDtoGen>(csv, hasHeader);
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
    }
}
