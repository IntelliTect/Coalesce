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
    public partial class PersonController 
         : LocalBaseApiController<Coalesce.Domain.Person, PersonDtoGen> 
    {
        private ClassViewModel _model;

        public PersonController() 
        { 
             _model = ReflectionRepository.Models.Single(m => m.Name == "Person");
        }
      

        /// <summary>
        /// Returns PersonDtoGen
        /// </summary>
        [HttpGet("list")]
        [AllowAnonymous]
        public virtual async Task<GenericListResult<Coalesce.Domain.Person, PersonDtoGen>> List(
            string includes = null, 
            string orderBy = null, string orderByDescending = null,
            int? page = null, int? pageSize = null, 
            string where = null, 
            string listDataSource = null, 
            string search = null, 
            // Custom fields for this object.
            string personId = null,string title = null,string firstName = null,string lastName = null,string email = null,string gender = null,string personStatsId = null,string name = null,string companyId = null)
        {
            
            ListParameters parameters = new ListParameters(null, includes, orderBy, orderByDescending, page, pageSize, where, listDataSource, search);

            // Add custom filters
            parameters.AddFilter("PersonId", personId);
            parameters.AddFilter("Title", title);
            parameters.AddFilter("FirstName", firstName);
            parameters.AddFilter("LastName", lastName);
            parameters.AddFilter("Email", email);
            parameters.AddFilter("Gender", gender);
            parameters.AddFilter("PersonStatsId", personStatsId);
            parameters.AddFilter("Name", name);
            parameters.AddFilter("CompanyId", companyId);
        
            var listResult = await ListImplementation(parameters);
            return new GenericListResult<Coalesce.Domain.Person, PersonDtoGen>(listResult);
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
            string personId = null,string title = null,string firstName = null,string lastName = null,string email = null,string gender = null,string personStatsId = null,string name = null,string companyId = null)
        {

            ListParameters parameters = new ListParameters(fields, includes, orderBy, orderByDescending, page, pageSize, where, listDataSource, search);

            // Add custom filters
            parameters.AddFilter("PersonId", personId);
            parameters.AddFilter("Title", title);
            parameters.AddFilter("FirstName", firstName);
            parameters.AddFilter("LastName", lastName);
            parameters.AddFilter("Email", email);
            parameters.AddFilter("Gender", gender);
            parameters.AddFilter("PersonStatsId", personStatsId);
            parameters.AddFilter("Name", name);
            parameters.AddFilter("CompanyId", companyId);
        
            return await ListImplementation(parameters);
        }

        [HttpGet("count")]
        [AllowAnonymous]
        public virtual async Task<int> Count(
            string where = null, 
            string listDataSource = null,
            string search = null,
            // Custom fields for this object.
            string personId = null,string title = null,string firstName = null,string lastName = null,string email = null,string gender = null,string personStatsId = null,string name = null,string companyId = null)
        {
            
            ListParameters parameters = new ListParameters(where: where, listDataSource: listDataSource, search: search, fields: null);

            // Add custom filters
            parameters.AddFilter("PersonId", personId);
            parameters.AddFilter("Title", title);
            parameters.AddFilter("FirstName", firstName);
            parameters.AddFilter("LastName", lastName);
            parameters.AddFilter("Email", email);
            parameters.AddFilter("Gender", gender);
            parameters.AddFilter("PersonStatsId", personStatsId);
            parameters.AddFilter("Name", name);
            parameters.AddFilter("CompanyId", companyId);
            
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
        public virtual async Task<PersonDtoGen> Get(string id, string includes = null)
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
        public virtual SaveResult<PersonDtoGen> Save(PersonDtoGen dto, string includes = null, bool returnObject = true)
        {
            
            // Check if creates/edits aren't allowed
            
            if (!dto.PersonId.HasValue && !_model.SecurityInfo.IsCreateAllowed(User)) {
                var result = new SaveResult<PersonDtoGen>();
                result.WasSuccessful = false;
                result.Message = "Create not allowed on Person objects.";
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return result;
            }
            else if (dto.PersonId.HasValue && !_model.SecurityInfo.IsEditAllowed(User)) {
                var result = new SaveResult<PersonDtoGen>();
                result.WasSuccessful = false;
                result.Message = "Edit not allowed on Person objects.";
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return result;
            }

            return SaveImplementation(dto, includes, returnObject);
        }
        
        [HttpPost("AddToCollection")]
        [AllowAnonymous]
        public virtual SaveResult<PersonDtoGen> AddToCollection(int id, string propertyName, int childId)
        {
            return ChangeCollection(id, propertyName, childId, "Add");
        }
        [HttpPost("RemoveFromCollection")]
        [AllowAnonymous]
        public virtual SaveResult<PersonDtoGen> RemoveFromCollection(int id, string propertyName, int childId)
        {
            return ChangeCollection(id, propertyName, childId, "Remove");
        }
        
        [AllowAnonymous]
        protected override IQueryable<Coalesce.Domain.Person> GetListDataSource(ListParameters parameters)
        {
            if (parameters.ListDataSource == "BorCPeople")
            {
                return Coalesce.Domain.Person.BorCPeople(Db);
            }

            return base.GetListDataSource(parameters);
        }

        // Methods

        // Method: Rename
        [HttpPost("Rename")]
        
        public virtual SaveResult<PersonDtoGen> Rename (Int32 id, String addition){
            var result = new SaveResult<PersonDtoGen>();
            try{
                var item = DataSource.Includes().FindItem(id);
                var objResult = item.Rename(addition);
                Db.SaveChanges();
                                result.Object = Mapper<Coalesce.Domain.Person, PersonDtoGen>.ObjToDtoMapper(objResult, User, "");
                result.WasSuccessful = true;
                result.Message = null;
            }catch(Exception ex){
                result.WasSuccessful = false;
                result.Message = ex.Message;
            }
            return result;
        }
        
        // Method: ChangeSpacesToDashesInName
        [HttpPost("ChangeSpacesToDashesInName")]
        
        public virtual SaveResult<object> ChangeSpacesToDashesInName (Int32 id){
            var result = new SaveResult<object>();
            try{
                var item = DataSource.Includes().FindItem(id);
                object objResult = null;
                item.ChangeSpacesToDashesInName();
                Db.SaveChanges();
                                result.Object = objResult;
                result.WasSuccessful = true;
                result.Message = null;
            }catch(Exception ex){
                result.WasSuccessful = false;
                result.Message = ex.Message;
            }
            return result;
        }
        
        // Method: Add
        [HttpPost("Add")]
        
        public virtual SaveResult<Int32> Add (Int32 numberOne, Int32 numberTwo){
            var result = new SaveResult<Int32>();
            try{
                var objResult = Coalesce.Domain.Person.Add(numberOne, numberTwo);
                                result.Object = objResult;
                result.WasSuccessful = true;
                result.Message = null;
            }catch(Exception ex){
                result.WasSuccessful = false;
                result.Message = ex.Message;
            }
            return result;
        }
        
        // Method: GetUser
        [HttpPost("GetUser")]
        [Authorize]
        public virtual SaveResult<String> GetUser (){
            if (!ClassViewModel.MethodByName("GetUser").SecurityInfo.IsExecutable(User)) throw new Exception("Not authorized");
            var result = new SaveResult<String>();
            try{
                var objResult = Coalesce.Domain.Person.GetUser(User);
                                result.Object = objResult;
                result.WasSuccessful = true;
                result.Message = null;
            }catch(Exception ex){
                result.WasSuccessful = false;
                result.Message = ex.Message;
            }
            return result;
        }
        
        // Method: GetUserPublic
        [HttpPost("GetUserPublic")]
        
        public virtual SaveResult<String> GetUserPublic (){
            var result = new SaveResult<String>();
            try{
                var objResult = Coalesce.Domain.Person.GetUserPublic(User);
                                result.Object = objResult;
                result.WasSuccessful = true;
                result.Message = null;
            }catch(Exception ex){
                result.WasSuccessful = false;
                result.Message = ex.Message;
            }
            return result;
        }
        
        // Method: NamesStartingWith
        [HttpPost("NamesStartingWith")]
        [Authorize]
        public virtual SaveResult<IEnumerable<String>> NamesStartingWith (String characters){
            if (!ClassViewModel.MethodByName("NamesStartingWith").SecurityInfo.IsExecutable(User)) throw new Exception("Not authorized");
            var result = new SaveResult<IEnumerable<String>>();
            try{
                var objResult = Coalesce.Domain.Person.NamesStartingWith(characters, Db);
                                result.Object = objResult;
                result.WasSuccessful = true;
                result.Message = null;
            }catch(Exception ex){
                result.WasSuccessful = false;
                result.Message = ex.Message;
            }
            return result;
        }
        
        // Method: NamesStartingWithPublic
        [HttpPost("NamesStartingWithPublic")]
        
        public virtual SaveResult<IEnumerable<String>> NamesStartingWithPublic (String characters){
            var result = new SaveResult<IEnumerable<String>>();
            try{
                var objResult = Coalesce.Domain.Person.NamesStartingWithPublic(characters, Db);
                                result.Object = objResult;
                result.WasSuccessful = true;
                result.Message = null;
            }catch(Exception ex){
                result.WasSuccessful = false;
                result.Message = ex.Message;
            }
            return result;
        }
        
        // Method: BorCPeople
        [HttpPost("BorCPeople")]
        
        public virtual SaveResult<IEnumerable<PersonDtoGen>> BorCPeople (){
            var result = new SaveResult<IEnumerable<PersonDtoGen>>();
            try{
                var objResult = Coalesce.Domain.Person.BorCPeople(Db);
                                result.Object = objResult.ToList().Select(o => Mapper<Coalesce.Domain.Person, PersonDtoGen>.ObjToDtoMapper(o, User, ""));
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
