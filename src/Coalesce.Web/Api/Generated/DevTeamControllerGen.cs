using IntelliTect.Coalesce.Controllers;
using IntelliTect.Coalesce.Data;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Helpers.IncludeTree;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using Coalesce.Web.Models;
using Coalesce.Domain;
using Coalesce.Domain.External;

namespace Coalesce.Web.Api
{
    [Route("api/[controller]")]
    [Authorize]
    public partial class DevTeamController
    : LocalBaseApiController<Coalesce.Domain.External.DevTeam, DevTeamDtoGen>
    {
        protected ClassViewModel Model;

        public DevTeamController()
        {
            Model = ReflectionRepository.Models.Single(m => m.Name == "DevTeam");
        }


        /// <summary>
        /// Returns DevTeamDtoGen
        /// </summary>
        [HttpGet("list")]
        [Authorize]
        public virtual async Task<GenericListResult<Coalesce.Domain.External.DevTeam, DevTeamDtoGen>> List(
            string includes = null,
            string orderBy = null, string orderByDescending = null,
            int? page = null, int? pageSize = null,
            string where = null,
            string dataSource = null,
            string search = null,
            // Custom fields for this object.
            string devTeamId = null, string name = null)
        {

            ListParameters parameters = new ListParameters(null, includes, orderBy, orderByDescending, page, pageSize, where, dataSource, search);

            // Add custom filters
            parameters.AddFilter("DevTeamId", devTeamId);
            parameters.AddFilter("Name", name);

            var listResult = await ListImplementation(parameters);
            return new GenericListResult<Coalesce.Domain.External.DevTeam, DevTeamDtoGen>(listResult);
        }

        /// <summary>
        /// Returns custom object based on supplied fields
        /// </summary>
        [HttpGet("customlist")]
        [Authorize]
        public virtual async Task<ListResult> CustomList(
            string fields = null,
            string includes = null,
            string orderBy = null, string orderByDescending = null,
            int? page = null, int? pageSize = null,
            string where = null,
            string dataSource = null,
            string search = null,
            // Custom fields for this object.
            string devTeamId = null, string name = null)
        {

            ListParameters parameters = new ListParameters(fields, includes, orderBy, orderByDescending, page, pageSize, where, dataSource, search);

            // Add custom filters
            parameters.AddFilter("DevTeamId", devTeamId);
            parameters.AddFilter("Name", name);

            return await ListImplementation(parameters);
        }

        [HttpGet("count")]
        [Authorize]
        public virtual async Task<int> Count(
            string where = null,
            string dataSource = null,
            string search = null,
            // Custom fields for this object.
            string devTeamId = null, string name = null)
        {

            ListParameters parameters = new ListParameters(where: where, dataSource: dataSource, search: search, fields: null);

            // Add custom filters
            parameters.AddFilter("DevTeamId", devTeamId);
            parameters.AddFilter("Name", name);

            return await CountImplementation(parameters);
        }

        [HttpGet("propertyValues")]
        [Authorize]
        public virtual IEnumerable<string> PropertyValues(string property, int page = 1, string search = "")
        {

            return PropertyValuesImplementation(property, page, search);
        }

        [HttpGet("get/{id}")]
        [Authorize]
        public virtual async Task<DevTeamDtoGen> Get(string id, string includes = null, string dataSource = null)
        {

            ListParameters listParams = new ListParameters(includes: includes, dataSource: dataSource);
            listParams.AddFilter("id", id);
            return await GetImplementation(id, listParams);
        }



        [HttpPost("delete/{id}")]
        [Authorize]
        public virtual bool Delete(string id)
        {

            return DeleteImplementation(id);
        }


        [HttpPost("save")]
        [Authorize]
        public virtual async Task<SaveResult<DevTeamDtoGen>> Save(DevTeamDtoGen dto, string includes = null, string dataSource = null, bool returnObject = true)
        {

            if (!dto.DevTeamId.HasValue && !Model.SecurityInfo.IsCreateAllowed(User))
            {
                var result = new SaveResult<DevTeamDtoGen>();
                result.WasSuccessful = false;
                result.Message = "Create not allowed on DevTeam objects.";
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return result;
            }
            else if (dto.DevTeamId.HasValue && !Model.SecurityInfo.IsEditAllowed(User))
            {
                var result = new SaveResult<DevTeamDtoGen>();
                result.WasSuccessful = false;
                result.Message = "Edit not allowed on DevTeam objects.";
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return result;
            }

            return await SaveImplementation(dto, includes, dataSource, returnObject);
        }

        [HttpPost("AddToCollection")]
        [Authorize]
        public virtual SaveResult<DevTeamDtoGen> AddToCollection(int id, string propertyName, int childId)
        {
            return ChangeCollection(id, propertyName, childId, "Add");
        }
        [HttpPost("RemoveFromCollection")]
        [Authorize]
        public virtual SaveResult<DevTeamDtoGen> RemoveFromCollection(int id, string propertyName, int childId)
        {
            return ChangeCollection(id, propertyName, childId, "Remove");
        }

        /// <summary>
        /// Downloads CSV of DevTeamDtoGen
        /// </summary>
        [HttpGet("csvDownload")]
        [Authorize]
        public virtual async Task<FileResult> CsvDownload(
            string orderBy = null, string orderByDescending = null,
            int? page = 1, int? pageSize = 10000,
            string where = null,
            string dataSource = null,
            string search = null,
            // Custom fields for this object.
            string devTeamId = null, string name = null)
        {
            ListParameters parameters = new ListParameters(null, "none", orderBy, orderByDescending, page, pageSize, where, dataSource, search);

            // Add custom filters
            parameters.AddFilter("DevTeamId", devTeamId);
            parameters.AddFilter("Name", name);

            var listResult = await ListImplementation(parameters);
            var list = listResult.List.Cast<DevTeamDtoGen>();
            var csv = IntelliTect.Coalesce.Helpers.CsvHelper.CreateCsv(list);

            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(csv);
            return File(bytes, "application/x-msdownload", "DevTeam.csv");
        }

        /// <summary>
        /// Returns CSV text of DevTeamDtoGen
        /// </summary>
        [HttpGet("csvText")]
        [Authorize]
        public virtual async Task<string> CsvText(
            string orderBy = null, string orderByDescending = null,
            int? page = 1, int? pageSize = 10000,
            string where = null,
            string dataSource = null,
            string search = null,
            // Custom fields for this object.
            string devTeamId = null, string name = null)
        {
            ListParameters parameters = new ListParameters(null, "none", orderBy, orderByDescending, page, pageSize, where, dataSource, search);

            // Add custom filters
            parameters.AddFilter("DevTeamId", devTeamId);
            parameters.AddFilter("Name", name);

            var listResult = await ListImplementation(parameters);
            var list = listResult.List.Cast<DevTeamDtoGen>();
            var csv = IntelliTect.Coalesce.Helpers.CsvHelper.CreateCsv(list);

            return csv;
        }



        /// <summary>
        /// Saves CSV data as an uploaded file
        /// </summary>
        [HttpPost("CsvUpload")]
        [Authorize]
        public virtual async Task<IEnumerable<SaveResult<DevTeamDtoGen>>> CsvUpload(Microsoft.AspNetCore.Http.IFormFile file, bool hasHeader = true)
        {
            if (file != null && file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    using (var reader = new System.IO.StreamReader(stream))
                    {
                        var csv = reader.ReadToEnd();
                        return await CsvSave(csv, hasHeader);
                    }
                }
            }
            throw new ArgumentException("No files uploaded");
        }

        /// <summary>
        /// Saves CSV data as a posted string
        /// </summary>
        [HttpPost("CsvSave")]
        [Authorize]
        public virtual async Task<IEnumerable<SaveResult<DevTeamDtoGen>>> CsvSave(string csv, bool hasHeader = true)
        {
            // Get list from CSV
            var list = IntelliTect.Coalesce.Helpers.CsvHelper.ReadCsv<DevTeamDtoGen>(csv, hasHeader);
            var resultList = new List<SaveResult<DevTeamDtoGen>>();
            foreach (var dto in list)
            {
                // Check if creates/edits aren't allowed
                if (!dto.DevTeamId.HasValue && !Model.SecurityInfo.IsCreateAllowed(User))
                {
                    var result = new SaveResult<DevTeamDtoGen>();
                    result.WasSuccessful = false;
                    result.Message = "Create not allowed on DevTeam objects.";
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    resultList.Add(result);
                }
                else if (dto.DevTeamId.HasValue && !Model.SecurityInfo.IsEditAllowed(User))
                {
                    var result = new SaveResult<DevTeamDtoGen>();
                    result.WasSuccessful = false;
                    result.Message = "Edit not allowed on DevTeam objects.";
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    resultList.Add(result);
                }
                else
                {
                    var result = await SaveImplementation(dto, "none", null, false);
                    resultList.Add(result);
                }
            }
            return resultList;
        }

        protected override IQueryable<Coalesce.Domain.External.DevTeam> GetDataSource(ListParameters parameters)
        {

            return base.GetDataSource(parameters);
        }

        // Methods from data class exposed through API Controller.
    }
}
