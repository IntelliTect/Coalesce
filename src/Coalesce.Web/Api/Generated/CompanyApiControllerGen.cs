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
    public partial class CompanyController 
         : LocalBaseApiController<Coalesce.Domain.Company, CompanyDtoGen> 
    {
        private ClassViewModel _model;

        public CompanyController() 
        { 
             _model = ReflectionRepository.Models.Single(m => m.Name == "Company");
        }
      

        /// <summary>
        /// Returns CompanyDtoGen
        /// </summary>
        [HttpGet("list")]
        [Authorize]
        public virtual async Task<GenericListResult<Coalesce.Domain.Company, CompanyDtoGen>> List(
            string includes = null, 
            string orderBy = null, string orderByDescending = null,
            int? page = null, int? pageSize = null, 
            string where = null, 
            string listDataSource = null, 
            string search = null, 
            // Custom fields for this object.
            string companyId = null,string name = null,string address1 = null,string address2 = null,string city = null,string state = null,string zipCode = null,string altName = null)
        {
            
            ListParameters parameters = new ListParameters(null, includes, orderBy, orderByDescending, page, pageSize, where, listDataSource, search);

            // Add custom filters
            parameters.AddFilter("CompanyId", companyId);
            parameters.AddFilter("Name", name);
            parameters.AddFilter("Address1", address1);
            parameters.AddFilter("Address2", address2);
            parameters.AddFilter("City", city);
            parameters.AddFilter("State", state);
            parameters.AddFilter("ZipCode", zipCode);
            parameters.AddFilter("AltName", altName);
        
            var listResult = await ListImplementation(parameters);
            return new GenericListResult<Coalesce.Domain.Company, CompanyDtoGen>(listResult);
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
            string listDataSource = null, 
            string search = null, 
            // Custom fields for this object.
            string companyId = null,string name = null,string address1 = null,string address2 = null,string city = null,string state = null,string zipCode = null,string altName = null)
        {

            ListParameters parameters = new ListParameters(fields, includes, orderBy, orderByDescending, page, pageSize, where, listDataSource, search);

            // Add custom filters
            parameters.AddFilter("CompanyId", companyId);
            parameters.AddFilter("Name", name);
            parameters.AddFilter("Address1", address1);
            parameters.AddFilter("Address2", address2);
            parameters.AddFilter("City", city);
            parameters.AddFilter("State", state);
            parameters.AddFilter("ZipCode", zipCode);
            parameters.AddFilter("AltName", altName);
        
            return await ListImplementation(parameters);
        }

        [HttpGet("count")]
        [Authorize]
        public virtual async Task<int> Count(
            string where = null, 
            string listDataSource = null,
            string search = null,
            // Custom fields for this object.
            string companyId = null,string name = null,string address1 = null,string address2 = null,string city = null,string state = null,string zipCode = null,string altName = null)
        {
            
            ListParameters parameters = new ListParameters(where: where, listDataSource: listDataSource, search: search, fields: null);

            // Add custom filters
            parameters.AddFilter("CompanyId", companyId);
            parameters.AddFilter("Name", name);
            parameters.AddFilter("Address1", address1);
            parameters.AddFilter("Address2", address2);
            parameters.AddFilter("City", city);
            parameters.AddFilter("State", state);
            parameters.AddFilter("ZipCode", zipCode);
            parameters.AddFilter("AltName", altName);
            
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
        public virtual async Task<CompanyDtoGen> Get(string id, string includes = null)
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
        public virtual SaveResult<CompanyDtoGen> Save(CompanyDtoGen dto, string includes = null, bool returnObject = true)
        {
            
            // Check if creates/edits aren't allowed
            
            if (!dto.CompanyId.HasValue && !_model.SecurityInfo.IsCreateAllowed(User)) {
                var result = new SaveResult<CompanyDtoGen>();
                result.WasSuccessful = false;
                result.Message = "Create not allowed on Company objects.";
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return result;
            }
            else if (dto.CompanyId.HasValue && !_model.SecurityInfo.IsEditAllowed(User)) {
                var result = new SaveResult<CompanyDtoGen>();
                result.WasSuccessful = false;
                result.Message = "Edit not allowed on Company objects.";
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return result;
            }

            return SaveImplementation(dto, includes, returnObject);
        }
        
        [HttpPost("AddToCollection")]
        [Authorize]
        public virtual SaveResult<CompanyDtoGen> AddToCollection(int id, string propertyName, int childId)
        {
            return ChangeCollection(id, propertyName, childId, "Add");
        }
        [HttpPost("RemoveFromCollection")]
        [Authorize]
        public virtual SaveResult<CompanyDtoGen> RemoveFromCollection(int id, string propertyName, int childId)
        {
            return ChangeCollection(id, propertyName, childId, "Remove");
        }
        
        [Authorize]
        protected override IQueryable<Coalesce.Domain.Company> GetListDataSource(ListParameters parameters)
        {

            return base.GetListDataSource(parameters);
        }

        // Methods
    }
}
