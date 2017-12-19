
using Coalesce.Web.Models;
using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Api;
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
    public partial class ProductController
    : LocalBaseApiController<Coalesce.Domain.Product, ProductDtoGen>
    {
        public ProductController(Coalesce.Domain.AppDbContext db) : base(db)
        {
            GeneratedForClassViewModel = ReflectionRepository.Global.GetClassViewModel<Coalesce.Domain.Product>();
        }


        [HttpGet("get/{id}")]
        [Authorize]
        public virtual Task<ProductDtoGen> Get(int id, DataSourceParameters parameters, IDataSource<Coalesce.Domain.Product> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [Authorize]
        public virtual Task<ListResult<ProductDtoGen>> List(ListParameters parameters, IDataSource<Coalesce.Domain.Product> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [Authorize]
        public virtual Task<int> Count(FilterParameters parameters, IDataSource<Coalesce.Domain.Product> dataSource)
            => CountImplementation(parameters, dataSource);


        [HttpPost("delete/{id}")]
        [Authorize]
        public virtual Task<ItemResult> Delete(int id, IBehaviors<Coalesce.Domain.Product> behaviors)
            => DeleteImplementation(id, behaviors);


        [HttpPost("save")]
        [Authorize(Roles = "Admin")]
        public virtual Task<ItemResult<ProductDtoGen>> Save(ProductDtoGen dto, [FromQuery] DataSourceParameters parameters, IDataSource<Coalesce.Domain.Product> dataSource, IBehaviors<Coalesce.Domain.Product> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

        /// <summary>
        /// Downloads CSV of ProductDtoGen
        /// </summary>
        [HttpGet("csvDownload")]
        [Authorize]
        public virtual async Task<FileResult> CsvDownload(ListParameters parameters, IDataSource<Coalesce.Domain.Product> dataSource)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(await CsvText(parameters, dataSource));
            return File(bytes, "application/x-msdownload", "Product.csv");
        }

        /// <summary>
        /// Returns CSV text of ProductDtoGen
        /// </summary>
        [HttpGet("csvText")]
        [Authorize]
        public virtual async Task<string> CsvText(ListParameters parameters, IDataSource<Coalesce.Domain.Product> dataSource)
        {
            var listResult = await ListImplementation(parameters, dataSource);
            return IntelliTect.Coalesce.Helpers.CsvHelper.CreateCsv(listResult.List);
        }



        /// <summary>
        /// Saves CSV data as an uploaded file
        /// </summary>
        [HttpPost("CsvUpload")]
        [Authorize(Roles = "Admin")]
        public virtual async Task<IEnumerable<ItemResult>> CsvUpload(
            Microsoft.AspNetCore.Http.IFormFile file,
            IDataSource<Coalesce.Domain.Product> dataSource,
            IBehaviors<Coalesce.Domain.Product> behaviors,
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
        [Authorize(Roles = "Admin")]
        public virtual async Task<IEnumerable<ItemResult>> CsvSave(
            string csv,
            IDataSource<Coalesce.Domain.Product> dataSource,
            IBehaviors<Coalesce.Domain.Product> behaviors,
            bool hasHeader = true)
        {
            // Get list from CSV
            var list = IntelliTect.Coalesce.Helpers.CsvHelper.ReadCsv<ProductDtoGen>(csv, hasHeader);
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
