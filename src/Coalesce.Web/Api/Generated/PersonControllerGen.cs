
using Coalesce.Web.Models;
using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Api.Controllers;
using IntelliTect.Coalesce.Api.DataSources;
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
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class PersonController
    : LocalBaseApiController<Coalesce.Domain.Person, PersonDtoGen>
    {
        public PersonController(Coalesce.Domain.AppDbContext db) : base(db)
        {
            GeneratedForClassViewModel = ReflectionRepository.Global.GetClassViewModel<Coalesce.Domain.Person>();
        }


        [HttpGet("get/{id}")]
        [AllowAnonymous]
        public virtual Task<PersonDtoGen> Get(int id, DataSourceParameters parameters, IDataSource<Coalesce.Domain.Person> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [AllowAnonymous]
        public virtual Task<ListResult<PersonDtoGen>> List(ListParameters parameters, IDataSource<Coalesce.Domain.Person> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [AllowAnonymous]
        public virtual Task<int> Count(FilterParameters parameters, IDataSource<Coalesce.Domain.Person> dataSource)
            => CountImplementation(parameters, dataSource);


        [HttpPost("delete/{id}")]
        [Authorize]
        public virtual Task<ItemResult> Delete(int id, IBehaviors<Coalesce.Domain.Person> behaviors)
            => DeleteImplementation(id, behaviors);


        [HttpPost("save")]
        [AllowAnonymous]
        public virtual Task<ItemResult<PersonDtoGen>> Save(PersonDtoGen dto, [FromQuery] DataSourceParameters parameters, IDataSource<Coalesce.Domain.Person> dataSource, IBehaviors<Coalesce.Domain.Person> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

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
        public virtual async Task<IEnumerable<ItemResult>> CsvUpload(
            Microsoft.AspNetCore.Http.IFormFile file,
            IDataSource<Coalesce.Domain.Person> dataSource,
            IBehaviors<Coalesce.Domain.Person> behaviors,
            bool hasHeader = true)
        {
            if (file == null || file.Length == 0) throw new ArgumentException("No files uploaded");

            using (var stream = file.OpenReadStream())
            {
                using (var reader = new System.IO.StreamReader(stream))
                {
                    var csv = await reader.ReadToEndAsync();
                    return await CsvSave(csv, dataSource, behaviors, hasHeader);
                }
            }
        }

        /// <summary>
        /// Saves CSV data as a posted string
        /// </summary>
        [HttpPost("CsvSave")]
        [AllowAnonymous]
        public virtual async Task<IEnumerable<ItemResult>> CsvSave(
            string csv,
            IDataSource<Coalesce.Domain.Person> dataSource,
            IBehaviors<Coalesce.Domain.Person> behaviors,
            bool hasHeader = true)
        {
            // Get list from CSV
            var list = IntelliTect.Coalesce.Helpers.CsvHelper.ReadCsv<PersonDtoGen>(csv, hasHeader);
            var resultList = new List<ItemResult>();
            foreach (var dto in list)
            {
                var parameters = new DataSourceParameters() { Includes = "none" };

                // Security: SaveImplementation is responsible for checking specific save/edit attribute permissions.
                var result = await SaveImplementation(dto, parameters, dataSource, behaviors);
                resultList.Add(new ItemResult { WasSuccessful = result.WasSuccessful, Message = result.Message });
            }
            return resultList;
        }

        // Methods from data class exposed through API Controller.

        /// <summary>
        /// Method: Rename
        /// </summary>
        [HttpPost("Rename")]

        public virtual async Task<ItemResult<PersonDtoGen>> Rename([FromServices] IDataSourceFactory dataSourceFactory, int id, string name)
        {
            var dataSource = dataSourceFactory.GetDefaultDataSource<Coalesce.Domain.Person>();
            var result = new ItemResult<PersonDtoGen>();
            try
            {
                IncludeTree includeTree = null;
                var (item, _) = await dataSource.GetItemAsync(id, new ListParameters());
                var objResult = item.Rename(name);
                Db.SaveChanges();
                var mappingContext = new MappingContext(User, "");
                result.Object = Mapper.MapToDto<Coalesce.Domain.Person, PersonDtoGen>(objResult, mappingContext, includeTree);

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

        public virtual async Task<ItemResult<object>> ChangeSpacesToDashesInName([FromServices] IDataSourceFactory dataSourceFactory, int id)
        {
            var dataSource = dataSourceFactory.GetDefaultDataSource<Coalesce.Domain.Person>();
            var result = new ItemResult<object>();
            try
            {
                var (item, _) = await dataSource.GetItemAsync(id, new ListParameters());
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

        public virtual ItemResult<int> Add([FromServices] IDataSourceFactory dataSourceFactory, int numberOne, int numberTwo)
        {
            var dataSource = dataSourceFactory.GetDefaultDataSource<Coalesce.Domain.Person>();
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
        [Authorize(Roles = "Admin")]
        public virtual ItemResult<string> GetUser([FromServices] IDataSourceFactory dataSourceFactory)
        {
            var dataSource = dataSourceFactory.GetDefaultDataSource<Coalesce.Domain.Person>();
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

        public virtual ItemResult<string> GetUserPublic([FromServices] IDataSourceFactory dataSourceFactory)
        {
            var dataSource = dataSourceFactory.GetDefaultDataSource<Coalesce.Domain.Person>();
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
        public virtual ItemResult<System.Collections.Generic.IEnumerable<string>> NamesStartingWith([FromServices] IDataSourceFactory dataSourceFactory, string characters)
        {
            var dataSource = dataSourceFactory.GetDefaultDataSource<Coalesce.Domain.Person>();
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
