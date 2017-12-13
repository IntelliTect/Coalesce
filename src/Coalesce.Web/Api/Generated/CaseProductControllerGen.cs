
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
    public partial class CaseProductController
    : LocalBaseApiController<Coalesce.Domain.CaseProduct, CaseProductDtoGen>
    {
        public CaseProductController(Coalesce.Domain.AppDbContext db) : base(db)
        {
        }


        [HttpGet("list")]
        [Authorize]
        public virtual Task<ListResult<CaseProductDtoGen>> List(ListParameters parameters, IDataSource<Coalesce.Domain.CaseProduct> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [Authorize]
        public virtual Task<int> Count(FilterParameters parameters, IDataSource<Coalesce.Domain.CaseProduct> dataSource)
            => CountImplementation(parameters, dataSource);

        [HttpGet("propertyValues")]
        [Authorize]
        public virtual IEnumerable<string> PropertyValues(string property, int page = 1, string search = "")
            => PropertyValuesImplementation(property, page, search);

        [HttpGet("get/{id}")]
        [Authorize]
        public virtual Task<CaseProductDtoGen> Get(string id, DataSourceParameters parameters, IDataSource<Coalesce.Domain.CaseProduct> dataSource)
            => GetImplementation(id, parameters, dataSource);


        [HttpPost("delete/{id}")]
        [Authorize]
        public virtual bool Delete(string id)
            => DeleteImplementation(id);


        [HttpPost("save")]
        [Authorize]
        public virtual async Task<ItemResult<CaseProductDtoGen>> Save(CaseProductDtoGen dto, [FromQuery] DataSourceParameters parameters, IDataSource<Coalesce.Domain.CaseProduct> dataSource, bool returnObject = true)
        {

            if (!dto.CaseProductId.HasValue && !ClassViewModel.SecurityInfo.IsCreateAllowed(User))
            {
                var result = new ItemResult<CaseProductDtoGen>("Create not allowed on CaseProduct objects.");
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return result;
            }
            else if (dto.CaseProductId.HasValue && !ClassViewModel.SecurityInfo.IsEditAllowed(User))
            {
                var result = new ItemResult<CaseProductDtoGen>("Edit not allowed on CaseProduct objects.");
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return result;
            }

            return await SaveImplementation(dto, parameters, dataSource, returnObject);
        }

        [HttpPost("AddToCollection")]
        [Authorize]
        public virtual ItemResult<CaseProductDtoGen> AddToCollection(int id, string propertyName, int childId)
        {
            return ChangeCollection(id, propertyName, childId, "Add");
        }
        [HttpPost("RemoveFromCollection")]
        [Authorize]
        public virtual ItemResult<CaseProductDtoGen> RemoveFromCollection(int id, string propertyName, int childId)
        {
            return ChangeCollection(id, propertyName, childId, "Remove");
        }

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
        public virtual async Task<IEnumerable<ItemResult<CaseProductDtoGen>>> CsvUpload(Microsoft.AspNetCore.Http.IFormFile file, IDataSource<Coalesce.Domain.CaseProduct> dataSource, bool hasHeader = true)
        {
            if (file == null || file.Length == 0) throw new ArgumentException("No files uploaded");

            using (var stream = file.OpenReadStream())
            {
                using (var reader = new System.IO.StreamReader(stream))
                {
                    var csv = reader.ReadToEnd();
                    return await CsvSave(csv, dataSource, hasHeader);
                }
            }
        }

        /// <summary>
        /// Saves CSV data as a posted string
        /// </summary>
        [HttpPost("CsvSave")]
        [Authorize]
        public virtual async Task<IEnumerable<ItemResult<CaseProductDtoGen>>> CsvSave(string csv, IDataSource<Coalesce.Domain.CaseProduct> dataSource, bool hasHeader = true)
        {
            // Get list from CSV
            var list = IntelliTect.Coalesce.Helpers.CsvHelper.ReadCsv<CaseProductDtoGen>(csv, hasHeader);
            var resultList = new List<ItemResult<CaseProductDtoGen>>();
            foreach (var dto in list)
            {
                // Check if creates/edits aren't allowed
                if (!dto.CaseProductId.HasValue && !ClassViewModel.SecurityInfo.IsCreateAllowed(User))
                {
                    var result = new ItemResult<CaseProductDtoGen>("Create not allowed on CaseProduct objects.");
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    resultList.Add(result);
                }
                else if (dto.CaseProductId.HasValue && !ClassViewModel.SecurityInfo.IsEditAllowed(User))
                {
                    var result = new ItemResult<CaseProductDtoGen>("Edit not allowed on CaseProduct objects.");
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    resultList.Add(result);
                }
                else
                {
                    var parameters = new DataSourceParameters() { Includes = "none" };
                    var result = await SaveImplementation(dto, parameters, dataSource, false);
                    resultList.Add(result);
                }
            }
            return resultList;
        }

        // Methods from data class exposed through API Controller.
    }
}
