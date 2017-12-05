
using Coalesce.Web.Models;
using IntelliTect.Coalesce;
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
    public partial class PersonController
    : LocalBaseApiController<Coalesce.Domain.Person, PersonDtoGen>
    {
        protected ClassViewModel Model;

        public PersonController()
        {
            Model = ReflectionRepository.Global.Models.Single(m => m.Name == "Person");
        }


        /// <summary>
        /// Returns PersonDtoGen
        /// </summary>
        [HttpGet("list")]
        [AllowAnonymous]
        public virtual async Task<ListResult<PersonDtoGen>> List(
            string includes = null,
            string orderBy = null, string orderByDescending = null,
            int? page = null, int? pageSize = null,
            string where = null,
            string dataSource = null,
            string search = null,
            // Custom fields for this object.
            string personId = null, string title = null, string firstName = null, string lastName = null, string email = null, string gender = null, string name = null, string companyId = null)
        {

            ListParameters parameters = new ListParameters(null, includes, orderBy, orderByDescending, page, pageSize, where, dataSource, search);

            // Add custom filters
            parameters.AddFilter("PersonId", personId);
            parameters.AddFilter("Title", title);
            parameters.AddFilter("FirstName", firstName);
            parameters.AddFilter("LastName", lastName);
            parameters.AddFilter("Email", email);
            parameters.AddFilter("Gender", gender);
            parameters.AddFilter("Name", name);
            parameters.AddFilter("CompanyId", companyId);

            return await ListImplementation(parameters);
        }

        /// <summary>
        /// Returns custom object based on supplied fields
        /// </summary>
        [HttpGet("customlist")]
        [AllowAnonymous]
        public virtual async Task<ListResult<PersonDtoGen>> CustomList(
            string fields = null,
            string includes = null,
            string orderBy = null, string orderByDescending = null,
            int? page = null, int? pageSize = null,
            string where = null,
            string dataSource = null,
            string search = null,
            // Custom fields for this object.
            string personId = null, string title = null, string firstName = null, string lastName = null, string email = null, string gender = null, string name = null, string companyId = null)
        {

            ListParameters parameters = new ListParameters(fields, includes, orderBy, orderByDescending, page, pageSize, where, dataSource, search);

            // Add custom filters
            parameters.AddFilter("PersonId", personId);
            parameters.AddFilter("Title", title);
            parameters.AddFilter("FirstName", firstName);
            parameters.AddFilter("LastName", lastName);
            parameters.AddFilter("Email", email);
            parameters.AddFilter("Gender", gender);
            parameters.AddFilter("Name", name);
            parameters.AddFilter("CompanyId", companyId);

            return await ListImplementation(parameters);
        }

        [HttpGet("count")]
        [AllowAnonymous]
        public virtual async Task<int> Count(
            string where = null,
            string dataSource = null,
            string search = null,
            // Custom fields for this object.
            string personId = null, string title = null, string firstName = null, string lastName = null, string email = null, string gender = null, string name = null, string companyId = null)
        {

            ListParameters parameters = new ListParameters(where: where, dataSource: dataSource, search: search, fields: null);

            // Add custom filters
            parameters.AddFilter("PersonId", personId);
            parameters.AddFilter("Title", title);
            parameters.AddFilter("FirstName", firstName);
            parameters.AddFilter("LastName", lastName);
            parameters.AddFilter("Email", email);
            parameters.AddFilter("Gender", gender);
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
        public virtual async Task<PersonDtoGen> Get(string id, string includes = null, string dataSource = null)
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
        public virtual async Task<SaveResult<PersonDtoGen>> Save(PersonDtoGen dto, string includes = null, string dataSource = null, bool returnObject = true)
        {

            if (!dto.PersonId.HasValue && !Model.SecurityInfo.IsCreateAllowed(User))
            {
                var result = new SaveResult<PersonDtoGen>();
                result.WasSuccessful = false;
                result.Message = "Create not allowed on Person objects.";
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return result;
            }
            else if (dto.PersonId.HasValue && !Model.SecurityInfo.IsEditAllowed(User))
            {
                var result = new SaveResult<PersonDtoGen>();
                result.WasSuccessful = false;
                result.Message = "Edit not allowed on Person objects.";
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return result;
            }

            return await SaveImplementation(dto, includes, dataSource, returnObject);
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

        /// <summary>
        /// Downloads CSV of PersonDtoGen
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
            string personId = null, string title = null, string firstName = null, string lastName = null, string email = null, string gender = null, string name = null, string companyId = null)
        {
            ListParameters parameters = new ListParameters(null, "none", orderBy, orderByDescending, page, pageSize, where, dataSource, search);

            // Add custom filters
            parameters.AddFilter("PersonId", personId);
            parameters.AddFilter("Title", title);
            parameters.AddFilter("FirstName", firstName);
            parameters.AddFilter("LastName", lastName);
            parameters.AddFilter("Email", email);
            parameters.AddFilter("Gender", gender);
            parameters.AddFilter("Name", name);
            parameters.AddFilter("CompanyId", companyId);

            var listResult = await ListImplementation(parameters);
            var list = listResult.List.Cast<PersonDtoGen>();
            var csv = IntelliTect.Coalesce.Helpers.CsvHelper.CreateCsv(list);

            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(csv);
            return File(bytes, "application/x-msdownload", "Person.csv");
        }

        /// <summary>
        /// Returns CSV text of PersonDtoGen
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
            string personId = null, string title = null, string firstName = null, string lastName = null, string email = null, string gender = null, string name = null, string companyId = null)
        {
            ListParameters parameters = new ListParameters(null, "none", orderBy, orderByDescending, page, pageSize, where, dataSource, search);

            // Add custom filters
            parameters.AddFilter("PersonId", personId);
            parameters.AddFilter("Title", title);
            parameters.AddFilter("FirstName", firstName);
            parameters.AddFilter("LastName", lastName);
            parameters.AddFilter("Email", email);
            parameters.AddFilter("Gender", gender);
            parameters.AddFilter("Name", name);
            parameters.AddFilter("CompanyId", companyId);

            var listResult = await ListImplementation(parameters);
            var list = listResult.List.Cast<PersonDtoGen>();
            var csv = IntelliTect.Coalesce.Helpers.CsvHelper.CreateCsv(list);

            return csv;
        }



        /// <summary>
        /// Saves CSV data as an uploaded file
        /// </summary>
        [HttpPost("CsvUpload")]
        [AllowAnonymous]
        public virtual async Task<IEnumerable<SaveResult<PersonDtoGen>>> CsvUpload(Microsoft.AspNetCore.Http.IFormFile file, bool hasHeader = true)
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
        public virtual async Task<IEnumerable<SaveResult<PersonDtoGen>>> CsvSave(string csv, bool hasHeader = true)
        {
            // Get list from CSV
            var list = IntelliTect.Coalesce.Helpers.CsvHelper.ReadCsv<PersonDtoGen>(csv, hasHeader);
            var resultList = new List<SaveResult<PersonDtoGen>>();
            foreach (var dto in list)
            {
                // Check if creates/edits aren't allowed
                if (!dto.PersonId.HasValue && !Model.SecurityInfo.IsCreateAllowed(User))
                {
                    var result = new SaveResult<PersonDtoGen>();
                    result.WasSuccessful = false;
                    result.Message = "Create not allowed on Person objects.";
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    resultList.Add(result);
                }
                else if (dto.PersonId.HasValue && !Model.SecurityInfo.IsEditAllowed(User))
                {
                    var result = new SaveResult<PersonDtoGen>();
                    result.WasSuccessful = false;
                    result.Message = "Edit not allowed on Person objects.";
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


        protected override IDataSource<Coalesce.Domain.Person> GetDataSource(ListParameters parameters)
        {
            IDataSource<Coalesce.Domain.Person> source = null;
            if (parameters.DataSource == "NamesStartingWithAWithCases")
            {
                source = ActivateDataSource<Coalesce.Domain.NamesStartingWithAWithCases>();
            }
            if (parameters.DataSource == "BorCPeople")
            {
                source = ActivateDataSource<Coalesce.Domain.BorCPeople>();
            }

            if (source != null)
            {
                source.Context.ListParameters = parameters;
                return source;
            }

            return base.GetDataSource(parameters);
        }

        // Methods from data class exposed through API Controller.

        /// <summary>
        /// Method: Rename
        /// </summary>
        [HttpPost("Rename")]
        public virtual async Task<SaveResult<PersonDtoGen>> Rename(int id, string addition)
        {
            var result = new SaveResult<PersonDtoGen>();
            try
            {
                IncludeTree includeTree = null;
                var (item, _) = await GetDataSource(new ListParameters()).GetItemAsync(id);
                var objResult = item.Rename(addition);
                Db.SaveChanges();
                var mappingContext = new MappingContext(User, "");
                result.Object = Mapper<Coalesce.Domain.Person, PersonDtoGen>.ObjToDtoMapper(objResult, mappingContext, includeTree);

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
        /// Method: ChangeSpacesToDashesInName
        /// </summary>
        [HttpPost("ChangeSpacesToDashesInName")]
        public virtual async Task<SaveResult<object>> ChangeSpacesToDashesInName(int id)
        {
            var result = new SaveResult<object>();
            try
            {
                var (item, _) = await GetDataSource(new ListParameters()).GetItemAsync(id);
                object objResult = null;
                item.ChangeSpacesToDashesInName();
                Db.SaveChanges();
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
        /// Method: Add
        /// </summary>
        [HttpPost("Add")]
        public virtual SaveResult<int> Add(int numberOne, int numberTwo)
        {
            var result = new SaveResult<int>();
            try
            {
                var objResult = Coalesce.Domain.Person.Add(numberOne, numberTwo);
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
        /// Method: GetUser
        /// </summary>
        [HttpPost("GetUser")]
        [Authorize]
        public virtual SaveResult<string> GetUser()
        {
            if (!ClassViewModel.MethodByName("GetUser").SecurityInfo.IsExecutable(User)) throw new Exception("Not authorized");
            var result = new SaveResult<string>();
            try
            {
                var objResult = Coalesce.Domain.Person.GetUser(User);
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
        /// Method: GetUserPublic
        /// </summary>
        [HttpPost("GetUserPublic")]
        public virtual SaveResult<string> GetUserPublic()
        {
            var result = new SaveResult<string>();
            try
            {
                var objResult = Coalesce.Domain.Person.GetUserPublic(User);
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
        /// Method: NamesStartingWith
        /// </summary>
        [HttpPost("NamesStartingWith")]
        [Authorize]
        public virtual SaveResult<System.Collections.Generic.IEnumerable<string>> NamesStartingWith(string characters)
        {
            if (!ClassViewModel.MethodByName("NamesStartingWith").SecurityInfo.IsExecutable(User)) throw new Exception("Not authorized");
            var result = new SaveResult<System.Collections.Generic.IEnumerable<string>>();
            try
            {
                var objResult = Coalesce.Domain.Person.NamesStartingWith(characters, Db);
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
    }
}
