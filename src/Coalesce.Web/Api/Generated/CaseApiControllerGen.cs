
using Intellitect.ComponentModel.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Intellitect.ComponentModel.Models;
using System.Threading.Tasks;
using System;
using System.Linq;
using Intellitect.ComponentModel.Data;
using Intellitect.ComponentModel.Mapping;
// Model Namespaces 
using Coalesce.Domain;
using Coalesce.Domain.External;

namespace Coalesce.Web.Api
{
    [Route("api/[controller]")]
    [Authorize]
    public partial class CaseController 
         : LocalBaseApiController<Case> 
    {
        public CaseController() { }
        
        
        [HttpGet("list")]
        [Authorize]
        public virtual async Task<ListResult> List(
            string fields = null, 
            string include = null, 
            string includes = null, 
            string orderBy = null, string orderByDescending = null,
            int? page = null, int? pageSize = null, 
            string where = null, 
            string listDataSource = null, 
            string search = null, 
            // Custom fields for this object.
            string caseKey = null,string title = null,string description = null,string openedAt = null,string assignedToId = null,string reportedById = null,string severity = null,string status = null,string devTeamAssignedId = null)
        {
            ListParameters parameters = new ListParameters(fields, include, includes, orderBy, orderByDescending, page, pageSize, where, listDataSource, search);

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
        [Authorize]
        public virtual async Task<int> Count(
            string where = null, 
            string listDataSource = null,
            string search = null,
            // Custom fields for this object.
            string caseKey = null,string title = null,string description = null,string openedAt = null,string assignedToId = null,string reportedById = null,string severity = null,string status = null,string devTeamAssignedId = null)
        {
            ListParameters parameters = new ListParameters(where: where, listDataSource: listDataSource, search: search);

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
        [Authorize]
        public virtual IEnumerable<string> PropertyValues(string property, int page = 1, string search = "")
        {
            return PropertyValuesImplementation(property, page, search);
        }

        [HttpGet("get/{id}")]
        [Authorize]
        public virtual async Task<Case> Get(string id, string includes = null)
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
        [Authorize]
        public virtual SaveResult<Case> Save(Case dto, string includes = null, bool returnObject = true)
        {
            return SaveImplementation(dto, includes, returnObject);
        }

        [HttpPost("AddToCollection")]
        [Authorize]
        public virtual SaveResult<Case> AddToCollection(int id, string propertyName, int childId)
        {
            return ChangeCollection(id, propertyName, childId, "Add");
        }
        [HttpPost("RemoveFromCollection")]
        [Authorize]
        public virtual SaveResult<Case> RemoveFromCollection(int id, string propertyName, int childId)
        {
            return ChangeCollection(id, propertyName, childId, "Remove");
        }
        

        protected override IQueryable<Case> GetListDataSource(ListParameters parameters)
        {
            if (parameters.ListDataSource == "GetAllOpenCases")
            {
                Int32 x = 0;
                if (parameters.Filters.ContainsKey("x"))
                {
                    x = Convert.ToInt32(parameters.Filters["x"]);
                }
                Int32 y = 0;
                if (parameters.Filters.ContainsKey("y"))
                {
                    y = Convert.ToInt32(parameters.Filters["y"]);
                }
                return Coalesce.Domain.Case.GetAllOpenCases(x, y, Db);
            }

            return base.GetListDataSource(parameters);
        }

        // Methods

        // Method: GetAllOpenCasesCount
        [HttpPost("GetAllOpenCasesCount")]
        
        public virtual SaveResult<Int32> GetAllOpenCasesCount (){
            var result = new SaveResult<Int32>();
            try{
                var objResult = Case.GetAllOpenCasesCount(Db);
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
        
        public virtual SaveResult<IEnumerable<Case>> GetAllOpenCases (Int32 x, Int32 y){
            var result = new SaveResult<IEnumerable<Case>>();
            try{
                var objResult = Case.GetAllOpenCases(x, y, Db);
                result.Object = objResult.ToList().Select(Mapper.ObjToDtoMapper(User).Map<Case>);
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
