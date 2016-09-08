using IntelliTect.Coalesce.Controllers;
using IntelliTect.Coalesce.Data;
using IntelliTect.Coalesce.Mapping;
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
    public partial class CaseController 
         : LocalBaseApiController<Coalesce.Domain.Case, CaseDtoGen> 
    {
        private ClassViewModel _model;

        public CaseController() 
        { 
             _model = ReflectionRepository.Models.Single(m => m.Name == "Case");
        }
      

        /// <summary>
        /// Returns CaseDtoGen
        /// </summary>
        [HttpGet("list")]
        [AllowAnonymous]
        public virtual async Task<GenericListResult<Coalesce.Domain.Case, CaseDtoGen>> List(
            string includes = null, 
            string orderBy = null, string orderByDescending = null,
            int? page = null, int? pageSize = null, 
            string where = null, 
            string listDataSource = null, 
            string search = null, 
            // Custom fields for this object.
            string caseKey = null,string title = null,string description = null,string openedAt = null,string assignedToId = null,string reportedById = null,string severity = null,string status = null,string devTeamAssignedId = null)
        {
            
            ListParameters parameters = new ListParameters(null, includes, orderBy, orderByDescending, page, pageSize, where, listDataSource, search);

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
            return new GenericListResult<Coalesce.Domain.Case, CaseDtoGen>(listResult);
        }

        /// <summary>
        /// Returns custom object based on supplied fields
        /// </summary>
        [HttpGet("customlist")]
        [AllowAnonymous]
        public virtual async Task<ListResult> CustomList(
            string fields = null, 
            string includes = null, 
            string orderBy = null, string orderByDescending = null,
            int? page = null, int? pageSize = null, 
            string where = null, 
            string listDataSource = null, 
            string search = null, 
            // Custom fields for this object.
            string caseKey = null,string title = null,string description = null,string openedAt = null,string assignedToId = null,string reportedById = null,string severity = null,string status = null,string devTeamAssignedId = null)
        {

            ListParameters parameters = new ListParameters(fields, includes, orderBy, orderByDescending, page, pageSize, where, listDataSource, search);

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
            string listDataSource = null,
            string search = null,
            // Custom fields for this object.
            string caseKey = null,string title = null,string description = null,string openedAt = null,string assignedToId = null,string reportedById = null,string severity = null,string status = null,string devTeamAssignedId = null)
        {
            
            ListParameters parameters = new ListParameters(where: where, listDataSource: listDataSource, search: search, fields: null);

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
        public virtual async Task<CaseDtoGen> Get(string id, string includes = null)
        {
            
            return await GetImplementation(id, includes);
        }
        


        [HttpPost("delete/{id}")]
        [Authorize]
        public virtual bool Delete(string id)
        {
            
            return DeleteImplementation(id);
        }
        

        [HttpPost("save")]
        [AllowAnonymous]
        public virtual SaveResult<CaseDtoGen> Save(CaseDtoGen dto, string includes = null, bool returnObject = true)
        {
            
            // Check if creates/edits aren't allowed
            
            if (!dto.CaseKey.HasValue && !_model.SecurityInfo.IsCreateAllowed(User)) {
                var result = new SaveResult<CaseDtoGen>();
                result.WasSuccessful = false;
                result.Message = "Create not allowed on Case objects.";
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return result;
            }
            else if (dto.CaseKey.HasValue && !_model.SecurityInfo.IsEditAllowed(User)) {
                var result = new SaveResult<CaseDtoGen>();
                result.WasSuccessful = false;
                result.Message = "Edit not allowed on Case objects.";
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return result;
            }

            return SaveImplementation(dto, includes, returnObject);
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
        
        [AllowAnonymous]
        protected override IQueryable<Coalesce.Domain.Case> GetListDataSource(ListParameters parameters)
        {
            if (parameters.ListDataSource == "GetAllOpenCases")
            {
                return Coalesce.Domain.Case.GetAllOpenCases(Db);
            }

            return base.GetListDataSource(parameters);
        }

        // Methods

        // Method: GetAllOpenCasesCount
        [HttpPost("GetAllOpenCasesCount")]
        
        public virtual SaveResult<Int32> GetAllOpenCasesCount (){
            var result = new SaveResult<Int32>();
            try{
                var objResult = Coalesce.Domain.Case.GetAllOpenCasesCount(Db);
                                result.Object = objResult;
                result.WasSuccessful = true;
                result.Message = null;
            }catch(Exception ex){
                result.WasSuccessful = false;
                result.Message = ex.Message;
            }
            return result;
        }
        
        // Method: RandomizeDatesAndStatus
        [HttpPost("RandomizeDatesAndStatus")]
        
        public virtual SaveResult<object> RandomizeDatesAndStatus (){
            var result = new SaveResult<object>();
            try{
                object objResult = null;
                Case.RandomizeDatesAndStatus(Db);
                                result.Object = objResult;
                result.WasSuccessful = true;
                result.Message = null;
            }catch(Exception ex){
                result.WasSuccessful = false;
                result.Message = ex.Message;
            }
            return result;
        }
        
        // Method: GetAllOpenCases
        [HttpPost("GetAllOpenCases")]
        
        public virtual SaveResult<IEnumerable<CaseDtoGen>> GetAllOpenCases (){
            var result = new SaveResult<IEnumerable<CaseDtoGen>>();
            try{
                var objResult = Coalesce.Domain.Case.GetAllOpenCases(Db);
                                result.Object = objResult.ToList().Select(o => Mapper<Coalesce.Domain.Case, CaseDtoGen>.ObjToDtoMapper(o, User, ""));
                result.WasSuccessful = true;
                result.Message = null;
            }catch(Exception ex){
                result.WasSuccessful = false;
                result.Message = ex.Message;
            }
            return result;
        }
        
        // Method: GetCaseSummary
        [HttpPost("GetCaseSummary")]
        
        public virtual SaveResult<CaseSummary> GetCaseSummary (){
            var result = new SaveResult<CaseSummary>();
            try{
                var objResult = Coalesce.Domain.Case.GetCaseSummary(Db);
                                result.Object = objResult;
                result.WasSuccessful = true;
                result.Message = null;
            }catch(Exception ex){
                result.WasSuccessful = false;
                result.Message = ex.Message;
            }
            return result;
        }
            }
}
