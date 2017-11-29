
using Coalesce.Web.Models;
using IntelliTect.Coalesce.Controllers;
using IntelliTect.Coalesce.Data;
using IntelliTect.Coalesce.Helpers.IncludeTree;
using IntelliTect.Coalesce.Mapping;
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
    public partial class CaseController
    : LocalBaseApiController<Coalesce.Domain.Case, CaseDtoGen>
    {
        protected ClassViewModel Model;

        public CaseController()
        {
            Model = ReflectionRepository.Global.Models.Single(m => m.Name == "Case");
        }


        /// <summary>
        /// Returns CaseDtoGen
        /// </summary>
        [HttpGet("list")]
        [AllowAnonymous]
        public virtual async Task<ListResult<CaseDtoGen>> List(
            string includes = null,
            string orderBy = null, string orderByDescending = null,
            int? page = null, int? pageSize = null,
            string where = null,
            string dataSource = null,
            string search = null,
            // Custom fields for this object.
            string caseKey = null, string title = null, string description = null, string openedAt = null, string assignedToId = null, string reportedById = null, string severity = null, string status = null, string devTeamAssignedId = null)
        {

            ListParameters parameters = new ListParameters(null, includes, orderBy, orderByDescending, page, pageSize, where, dataSource, search);

            // Add custom filters
            parameters.AddFilter("CaseKey", caseKey);
            parameters.AddFilter("Title", title);
            parameters.AddFilter("Description", description);
            parameters.AddFilter("OpenedAt", openedAt);
            parameters.AddFilter("AssignedToId", assignedToId);
            parameters.AddFilter("ReportedById", reportedById);
            parameters.AddFilter("Severity", severity);
            parameters.AddFilter("Status", status);
            parameters.AddFilter("DevTeamAssignedId", devTeamAssignedId);

            return await ListImplementation(parameters);
        }

        /// <summary>
        /// Returns custom object based on supplied fields
        /// </summary>
        [HttpGet("customlist")]
        [AllowAnonymous]
        public virtual async Task<ListResult<CaseDtoGen>> CustomList(
            string fields = null,
            string includes = null,
            string orderBy = null, string orderByDescending = null,
            int? page = null, int? pageSize = null,
            string where = null,
            string dataSource = null,
            string search = null,
            // Custom fields for this object.
            string caseKey = null, string title = null, string description = null, string openedAt = null, string assignedToId = null, string reportedById = null, string severity = null, string status = null, string devTeamAssignedId = null)
        {

            ListParameters parameters = new ListParameters(fields, includes, orderBy, orderByDescending, page, pageSize, where, dataSource, search);

            // Add custom filters
            parameters.AddFilter("CaseKey", caseKey);
            parameters.AddFilter("Title", title);
            parameters.AddFilter("Description", description);
            parameters.AddFilter("OpenedAt", openedAt);
            parameters.AddFilter("AssignedToId", assignedToId);
            parameters.AddFilter("ReportedById", reportedById);
            parameters.AddFilter("Severity", severity);
            parameters.AddFilter("Status", status);
            parameters.AddFilter("DevTeamAssignedId", devTeamAssignedId);

            return await ListImplementation(parameters);
        }

        [HttpGet("count")]
        [AllowAnonymous]
        public virtual async Task<int> Count(
            string where = null,
            string dataSource = null,
            string search = null,
            // Custom fields for this object.
            string caseKey = null, string title = null, string description = null, string openedAt = null, string assignedToId = null, string reportedById = null, string severity = null, string status = null, string devTeamAssignedId = null)
        {

            ListParameters parameters = new ListParameters(where: where, dataSource: dataSource, search: search, fields: null);

            // Add custom filters
            parameters.AddFilter("CaseKey", caseKey);
            parameters.AddFilter("Title", title);
            parameters.AddFilter("Description", description);
            parameters.AddFilter("OpenedAt", openedAt);
            parameters.AddFilter("AssignedToId", assignedToId);
            parameters.AddFilter("ReportedById", reportedById);
            parameters.AddFilter("Severity", severity);
            parameters.AddFilter("Status", status);
            parameters.AddFilter("DevTeamAssignedId", devTeamAssignedId);

            return await CountImplementation(parameters);
        }

        [HttpGet("propertyValues")]
        [AllowAnonymous]
        public virtual IEnumerable<string> PropertyValues(string property, int page = 1, string search = "")
        {

            return PropertyValuesImplementation(property, page, search);
        }

        [HttpGet("get/{id}")]
        [AllowAnonymous]
        public virtual async Task<CaseDtoGen> Get(string id, string includes = null, string dataSource = null)
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
        [AllowAnonymous]
        public virtual async Task<SaveResult<CaseDtoGen>> Save(CaseDtoGen dto, string includes = null, string dataSource = null, bool returnObject = true)
        {

            if (!dto.CaseKey.HasValue && !Model.SecurityInfo.IsCreateAllowed(User))
            {
                var result = new SaveResult<CaseDtoGen>();
                result.WasSuccessful = false;
                result.Message = "Create not allowed on Case objects.";
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return result;
            }
            else if (dto.CaseKey.HasValue && !Model.SecurityInfo.IsEditAllowed(User))
            {
                var result = new SaveResult<CaseDtoGen>();
                result.WasSuccessful = false;
                result.Message = "Edit not allowed on Case objects.";
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return result;
            }

            return await SaveImplementation(dto, includes, dataSource, returnObject);
        }

        [HttpPost("AddToCollection")]
        [Authorize]
        public virtual SaveResult<CaseDtoGen> AddToCollection(int id, string propertyName, int childId)
        {
            return ChangeCollection(id, propertyName, childId, "Add");
        }
        [HttpPost("RemoveFromCollection")]
        [Authorize]
        public virtual SaveResult<CaseDtoGen> RemoveFromCollection(int id, string propertyName, int childId)
        {
            return ChangeCollection(id, propertyName, childId, "Remove");
        }

        /// <summary>
        /// Downloads CSV of CaseDtoGen
        /// </summary>
        [HttpGet("csvDownload")]
        [AllowAnonymous]
        public virtual async Task<FileResult> CsvDownload(
            string orderBy = null, string orderByDescending = null,
            int? page = 1, int? pageSize = 10000,
            string where = null,
            string dataSource = null,
            string search = null,
            // Custom fields for this object.
            string caseKey = null, string title = null, string description = null, string openedAt = null, string assignedToId = null, string reportedById = null, string severity = null, string status = null, string devTeamAssignedId = null)
        {
            ListParameters parameters = new ListParameters(null, "none", orderBy, orderByDescending, page, pageSize, where, dataSource, search);

            // Add custom filters
            parameters.AddFilter("CaseKey", caseKey);
            parameters.AddFilter("Title", title);
            parameters.AddFilter("Description", description);
            parameters.AddFilter("OpenedAt", openedAt);
            parameters.AddFilter("AssignedToId", assignedToId);
            parameters.AddFilter("ReportedById", reportedById);
            parameters.AddFilter("Severity", severity);
            parameters.AddFilter("Status", status);
            parameters.AddFilter("DevTeamAssignedId", devTeamAssignedId);

            var listResult = await ListImplementation(parameters);
            var list = listResult.List.Cast<CaseDtoGen>();
            var csv = IntelliTect.Coalesce.Helpers.CsvHelper.CreateCsv(list);

            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(csv);
            return File(bytes, "application/x-msdownload", "Case.csv");
        }

        /// <summary>
        /// Returns CSV text of CaseDtoGen
        /// </summary>
        [HttpGet("csvText")]
        [AllowAnonymous]
        public virtual async Task<string> CsvText(
            string orderBy = null, string orderByDescending = null,
            int? page = 1, int? pageSize = 10000,
            string where = null,
            string dataSource = null,
            string search = null,
            // Custom fields for this object.
            string caseKey = null, string title = null, string description = null, string openedAt = null, string assignedToId = null, string reportedById = null, string severity = null, string status = null, string devTeamAssignedId = null)
        {
            ListParameters parameters = new ListParameters(null, "none", orderBy, orderByDescending, page, pageSize, where, dataSource, search);

            // Add custom filters
            parameters.AddFilter("CaseKey", caseKey);
            parameters.AddFilter("Title", title);
            parameters.AddFilter("Description", description);
            parameters.AddFilter("OpenedAt", openedAt);
            parameters.AddFilter("AssignedToId", assignedToId);
            parameters.AddFilter("ReportedById", reportedById);
            parameters.AddFilter("Severity", severity);
            parameters.AddFilter("Status", status);
            parameters.AddFilter("DevTeamAssignedId", devTeamAssignedId);

            var listResult = await ListImplementation(parameters);
            var list = listResult.List.Cast<CaseDtoGen>();
            var csv = IntelliTect.Coalesce.Helpers.CsvHelper.CreateCsv(list);

            return csv;
        }



        /// <summary>
        /// Saves CSV data as an uploaded file
        /// </summary>
        [HttpPost("CsvUpload")]
        [AllowAnonymous]
        public virtual async Task<IEnumerable<SaveResult<CaseDtoGen>>> CsvUpload(Microsoft.AspNetCore.Http.IFormFile file, bool hasHeader = true)
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
        [AllowAnonymous]
        public virtual async Task<IEnumerable<SaveResult<CaseDtoGen>>> CsvSave(string csv, bool hasHeader = true)
        {
            // Get list from CSV
            var list = IntelliTect.Coalesce.Helpers.CsvHelper.ReadCsv<CaseDtoGen>(csv, hasHeader);
            var resultList = new List<SaveResult<CaseDtoGen>>();
            foreach (var dto in list)
            {
                // Check if creates/edits aren't allowed
                if (!dto.CaseKey.HasValue && !Model.SecurityInfo.IsCreateAllowed(User))
                {
                    var result = new SaveResult<CaseDtoGen>();
                    result.WasSuccessful = false;
                    result.Message = "Create not allowed on Case objects.";
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    resultList.Add(result);
                }
                else if (dto.CaseKey.HasValue && !Model.SecurityInfo.IsEditAllowed(User))
                {
                    var result = new SaveResult<CaseDtoGen>();
                    result.WasSuccessful = false;
                    result.Message = "Edit not allowed on Case objects.";
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

        protected override IQueryable<Coalesce.Domain.Case> GetDataSource(ListParameters parameters)
        {
            if (parameters.DataSource == "GetAllOpenCases")
            {
                return Coalesce.Domain.Case.GetAllOpenCases(Db);
            }

            return base.GetDataSource(parameters);
        }

        // Methods from data class exposed through API Controller.

        /// <summary>
        /// Method: GetAllOpenCasesCount
        /// </summary>
        [HttpPost("GetAllOpenCasesCount")]
        public virtual SaveResult<int> GetAllOpenCasesCount()
        {
            var result = new SaveResult<int>();
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
        public virtual SaveResult<object> RandomizeDatesAndStatus()
        {
            var result = new SaveResult<object>();
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
        public virtual SaveResult<CaseSummaryDtoGen> GetCaseSummary()
        {
            var result = new SaveResult<CaseSummaryDtoGen>();
            try
            {
                IncludeTree includeTree = null;
                var objResult = Coalesce.Domain.Case.GetCaseSummary(Db);
                var mappingContext = new MappingContext(User, "");
                result.Object = Mapper<Coalesce.Domain.CaseSummary, CaseSummaryDtoGen>.ObjToDtoMapper(objResult, mappingContext, includeTree);

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
