
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

namespace Coalesce.Web.TestArea.Api
{
    [Route("TestArea/api/[controller]")]
    [Authorize]
    public partial class ProductController 
         : LocalBaseApiController<Product> 
    {
        public ProductController() { }
        
        
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
            string productId = null,string name = null)
        {
            ListParameters parameters = new ListParameters(fields, include, includes, orderBy, orderByDescending, page, pageSize, where, listDataSource, search);

            // Add custom filters
            parameters.AddFilter("ProductId", productId);
            parameters.AddFilter("Name", name);
        
            return await ListImplementation(parameters);
        }


        [HttpGet("count")]
        [Authorize]
        public virtual async Task<int> Count(
            string where = null, 
            string listDataSource = null,
            string search = null,
            // Custom fields for this object.
            string productId = null,string name = null)
        {
            ListParameters parameters = new ListParameters(where: where, listDataSource: listDataSource, search: search);

            // Add custom filters
            parameters.AddFilter("ProductId", productId);
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
        public virtual async Task<Product> Get(string id, string includes = null)
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
        public virtual SaveResult<Product> Save(Product dto, string includes = null, bool returnObject = true)
        {
            return SaveImplementation(dto, includes, returnObject);
        }

        [HttpPost("AddToCollection")]
        [Authorize]
        public virtual SaveResult<Product> AddToCollection(int id, string propertyName, int childId)
        {
            return ChangeCollection(id, propertyName, childId, "Add");
        }
        [HttpPost("RemoveFromCollection")]
        [Authorize]
        public virtual SaveResult<Product> RemoveFromCollection(int id, string propertyName, int childId)
        {
            return ChangeCollection(id, propertyName, childId, "Remove");
        }
        

        protected override IQueryable<Product> GetListDataSource(ListParameters parameters)
        {

            return base.GetListDataSource(parameters);
        }

        // Methods
    }
}
