
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
    public partial class PersonController
    : LocalBaseApiController<Coalesce.Domain.Person, PersonDtoGen>
    {
        public PersonController(Coalesce.Domain.AppDbContext db) : base(db)
        {
        }


        [HttpGet("list")]
        [AllowAnonymous]
        public virtual Task<ListResult<PersonDtoGen>> List(ListParameters parameters, IDataSource<Coalesce.Domain.Person> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [AllowAnonymous]
        public virtual Task<int> Count(FilterParameters parameters, IDataSource<Coalesce.Domain.Person> dataSource)
            => CountImplementation(parameters, dataSource);

        [HttpGet("propertyValues")]
        [AllowAnonymous]
        public virtual IEnumerable<string> PropertyValues(string property, int page = 1, string search = "")
            => PropertyValuesImplementation(property, page, search);

        [HttpGet("get/{id}")]
        [AllowAnonymous]
        public virtual Task<PersonDtoGen> Get(string id, DataSourceParameters parameters, IDataSource<Coalesce.Domain.Person> dataSource)
            => GetImplementation(id, parameters, dataSource);


        [HttpPost("delete/{id}")]
        [Authorize]
        public virtual bool Delete(string id)
            => DeleteImplementation(id);


        [HttpPost("save")]
        [AllowAnonymous]
        public virtual async Task<ItemResult<PersonDtoGen>> Save(PersonDtoGen dto, [FromQuery] DataSourceParameters parameters, IDataSource<Coalesce.Domain.Person> dataSource, bool returnObject = true)
        {

            if (!dto.PersonId.HasValue && !ClassViewModel.SecurityInfo.IsCreateAllowed(User))
            {
                var result = new ItemResult<PersonDtoGen>("Create not allowed on Person objects.");
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return result;
            }
            else if (dto.PersonId.HasValue && !ClassViewModel.SecurityInfo.IsEditAllowed(User))
            {
                var result = new ItemResult<PersonDtoGen>("Edit not allowed on Person objects.");
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return result;
            }

            return await SaveImplementation(dto, parameters, dataSource, returnObject);
        }

        [HttpPost("AddToCollection")]
        [AllowAnonymous]
        public virtual ItemResult<PersonDtoGen> AddToCollection(int id, string propertyName, int childId)
        {
            return ChangeCollection(id, propertyName, childId, "Add");
        }
        [HttpPost("RemoveFromCollection")]
        [AllowAnonymous]
        public virtual ItemResult<PersonDtoGen> RemoveFromCollection(int id, string propertyName, int childId)
        {
            return ChangeCollection(id, propertyName, childId, "Remove");
        }

        /// <summary>
        /// Downloads CSV of PersonDtoGen
        /// </summary>
        [HttpGet("csvDownload")]
        [AllowAnonymous]
        public virtual async Task<FileResult> CsvDownload(ListParameters parameters, IDataSource<Coalesce.Domain.Person> dataSource)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(await CsvText(parameters, dataSource));
            return File(bytes, "application/x-msdownload", "Person.csv");
        }

        /// <summary>
        /// Returns CSV text of PersonDtoGen
        /// </summary>
        [HttpGet("csvText")]
        [AllowAnonymous]
        public virtual async Task<string> CsvText(ListParameters parameters, IDataSource<Coalesce.Domain.Person> dataSource)
        {
            var listResult = await ListImplementation(parameters, dataSource);
            return IntelliTect.Coalesce.Helpers.CsvHelper.CreateCsv(listResult.List);
        }



        /// <summary>
        /// Saves CSV data as an uploaded file
        /// </summary>
        [HttpPost("CsvUpload")]
        [AllowAnonymous]
        public virtual async Task<IEnumerable<ItemResult<PersonDtoGen>>> CsvUpload(Microsoft.AspNetCore.Http.IFormFile file, IDataSource<Coalesce.Domain.Person> dataSource, bool hasHeader = true)
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
        [AllowAnonymous]
        public virtual async Task<IEnumerable<ItemResult<PersonDtoGen>>> CsvSave(string csv, IDataSource<Coalesce.Domain.Person> dataSource, bool hasHeader = true)
        {
            // Get list from CSV
            var list = IntelliTect.Coalesce.Helpers.CsvHelper.ReadCsv<PersonDtoGen>(csv, hasHeader);
            var resultList = new List<ItemResult<PersonDtoGen>>();
            foreach (var dto in list)
            {
                // Check if creates/edits aren't allowed
                if (!dto.PersonId.HasValue && !ClassViewModel.SecurityInfo.IsCreateAllowed(User))
                {
                    var result = new ItemResult<PersonDtoGen>("Create not allowed on Person objects.");
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    resultList.Add(result);
                }
                else if (dto.PersonId.HasValue && !ClassViewModel.SecurityInfo.IsEditAllowed(User))
                {
                    var result = new ItemResult<PersonDtoGen>("Edit not allowed on Person objects.");
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

        /// <summary>
        /// Method: Rename
        /// </summary>
        [HttpPost("Rename")]
        public virtual async Task<ItemResult<PersonDtoGen>> Rename(int id, string name)
        {
            var result = new ItemResult<PersonDtoGen>();
            try
            {
                IncludeTree includeTree = null;
                var (item, _) = await GetDataSource(new ListParameters()).GetItemAsync(id, new ListParameters());
                var objResult = item.Rename(name);
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
        public virtual async Task<ItemResult<object>> ChangeSpacesToDashesInName(int id)
        {
            var result = new ItemResult<object>();
            try
            {
                var (item, _) = await GetDataSource(new ListParameters()).GetItemAsync(id, new ListParameters());
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
        public virtual ItemResult<int> Add(int numberOne, int numberTwo)
        {
            var result = new ItemResult<int>();
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
        public virtual ItemResult<string> GetUser()
        {
            if (!ClassViewModel.MethodByName("GetUser").SecurityInfo.IsExecutable(User)) throw new Exception("Not authorized");
            var result = new ItemResult<string>();
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
        public virtual ItemResult<string> GetUserPublic()
        {
            var result = new ItemResult<string>();
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
        public virtual ItemResult<System.Collections.Generic.IEnumerable<string>> NamesStartingWith(string characters)
        {
            if (!ClassViewModel.MethodByName("NamesStartingWith").SecurityInfo.IsExecutable(User)) throw new Exception("Not authorized");
            var result = new ItemResult<System.Collections.Generic.IEnumerable<string>>();
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
