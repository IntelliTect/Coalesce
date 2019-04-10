
using Coalesce.Web.Vue.Models;
using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Api.Controllers;
using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Mapping.IncludeTrees;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Coalesce.Web.Vue.Api
{
    [Route("api/Person")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class PersonController
        : BaseApiController<Coalesce.Domain.Person, PersonDtoGen, Coalesce.Domain.AppDbContext>
    {
        public PersonController(Coalesce.Domain.AppDbContext db) : base(db)
        {
            GeneratedForClassViewModel = ReflectionRepository.Global.GetClassViewModel<Coalesce.Domain.Person>();
        }

        [HttpGet("get/{id}")]
        [AllowAnonymous]
        public virtual Task<ItemResult<PersonDtoGen>> Get(
            int id,
            DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.Person> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [AllowAnonymous]
        public virtual Task<ListResult<PersonDtoGen>> List(
            ListParameters parameters,
            IDataSource<Coalesce.Domain.Person> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [AllowAnonymous]
        public virtual Task<ItemResult<int>> Count(
            FilterParameters parameters,
            IDataSource<Coalesce.Domain.Person> dataSource)
            => CountImplementation(parameters, dataSource);

        [HttpPost("save")]
        [AllowAnonymous]
        public virtual Task<ItemResult<PersonDtoGen>> Save(
            PersonDtoGen dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.Person> dataSource,
            IBehaviors<Coalesce.Domain.Person> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

        [HttpPost("delete/{id}")]
        [Authorize]
        public virtual Task<ItemResult<PersonDtoGen>> Delete(
            int id,
            IBehaviors<Coalesce.Domain.Person> behaviors,
            IDataSource<Coalesce.Domain.Person> dataSource)
            => DeleteImplementation(id, new DataSourceParameters(), dataSource, behaviors);

        /// <summary>
        /// Downloads CSV of PersonDtoGen
        /// </summary>
        [HttpGet("csvDownload")]
        [AllowAnonymous]
        public virtual Task<FileResult> CsvDownload(
            ListParameters parameters,
            IDataSource<Coalesce.Domain.Person> dataSource)
            => CsvDownloadImplementation(parameters, dataSource);

        /// <summary>
        /// Returns CSV text of PersonDtoGen
        /// </summary>
        [HttpGet("csvText")]
        [AllowAnonymous]
        public virtual Task<string> CsvText(
            ListParameters parameters,
            IDataSource<Coalesce.Domain.Person> dataSource)
            => CsvTextImplementation(parameters, dataSource);

        /// <summary>
        /// Saves CSV data as an uploaded file
        /// </summary>
        [HttpPost("csvUpload")]
        [AllowAnonymous]
        public virtual Task<IEnumerable<ItemResult>> CsvUpload(
            IFormFile file,
            IDataSource<Coalesce.Domain.Person> dataSource,
            IBehaviors<Coalesce.Domain.Person> behaviors,
            bool hasHeader = true)
            => CsvUploadImplementation(file, dataSource, behaviors, hasHeader);

        /// <summary>
        /// Saves CSV data as a posted string
        /// </summary>
        [HttpPost("csvSave")]
        [AllowAnonymous]
        public virtual Task<IEnumerable<ItemResult>> CsvSave(
            string csv,
            IDataSource<Coalesce.Domain.Person> dataSource,
            IBehaviors<Coalesce.Domain.Person> behaviors,
            bool hasHeader = true)
            => CsvSaveImplementation(csv, dataSource, behaviors, hasHeader);

        // Methods from data class exposed through API Controller.

        /// <summary>
        /// Method: Rename
        /// </summary>
        [HttpPost("Rename")]
        [Authorize]
        public virtual async Task<ItemResult<PersonDtoGen>> Rename([FromServices] IDataSourceFactory dataSourceFactory, int id, string name)
        {
            IncludeTree includeTree = null;
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("Default");
            var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<PersonDtoGen>(itemResult);
            }
            var item = itemResult.Object;
            var methodResult = item.Rename(name);
            await Db.SaveChangesAsync();
            var result = new ItemResult<PersonDtoGen>();
            var mappingContext = new MappingContext(User, "");
            result.Object = Mapper.MapToDto<Coalesce.Domain.Person, PersonDtoGen>(methodResult, mappingContext, includeTree);
            return result;
        }

        /// <summary>
        /// Method: ChangeSpacesToDashesInName
        /// </summary>
        [HttpPost("ChangeSpacesToDashesInName")]
        [Authorize]
        public virtual async Task<ItemResult> ChangeSpacesToDashesInName([FromServices] IDataSourceFactory dataSourceFactory, int id)
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("WithoutCases");
            var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult(itemResult);
            }
            var item = itemResult.Object;
            var methodResult = item.ChangeSpacesToDashesInName();
            await Db.SaveChangesAsync();
            var result = new ItemResult(methodResult);
            return result;
        }

        /// <summary>
        /// Method: Add
        /// </summary>
        [HttpPost("Add")]
        [Authorize]
        public virtual ItemResult<int> Add(int numberOne, int numberTwo)
        {
            var methodResult = Coalesce.Domain.Person.Add(numberOne, numberTwo);
            var result = new ItemResult<int>(methodResult);
            result.Object = methodResult.Object;
            return result;
        }

        /// <summary>
        /// Method: GetUser
        /// </summary>
        [HttpPost("GetUser")]
        [Authorize(Roles = "Admin")]
        public virtual ItemResult<string> GetUser()
        {
            var methodResult = Coalesce.Domain.Person.GetUser(User);
            var result = new ItemResult<string>();
            result.Object = methodResult;
            return result;
        }

        /// <summary>
        /// Method: PersonCount
        /// </summary>
        [HttpGet("PersonCount")]
        [Authorize]
        public virtual ItemResult<long> PersonCount(string lastNameStartsWith = "")
        {
            var methodResult = Coalesce.Domain.Person.PersonCount(Db, lastNameStartsWith);
            var result = new ItemResult<long>();
            result.Object = methodResult;
            return result;
        }

        /// <summary>
        /// Method: FullNameAndAge
        /// </summary>
        [HttpGet("FullNameAndAge")]
        [Authorize]
        public virtual async Task<ItemResult<string>> FullNameAndAge([FromServices] IDataSourceFactory dataSourceFactory, int id)
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("Default");
            var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<string>(itemResult);
            }
            var item = itemResult.Object;
            var methodResult = item.FullNameAndAge(Db);
            await Db.SaveChangesAsync();
            var result = new ItemResult<string>();
            result.Object = methodResult;
            return result;
        }

        /// <summary>
        /// Method: RemovePersonById
        /// </summary>
        [HttpDelete("RemovePersonById")]
        [Authorize]
        public virtual ItemResult<bool> RemovePersonById(int id)
        {
            var methodResult = Coalesce.Domain.Person.RemovePersonById(Db, id);
            var result = new ItemResult<bool>();
            result.Object = methodResult;
            return result;
        }

        /// <summary>
        /// Method: ObfuscateEmail
        /// </summary>
        [HttpPut("ObfuscateEmail")]
        [Authorize]
        public virtual async Task<ItemResult<string>> ObfuscateEmail([FromServices] IDataSourceFactory dataSourceFactory, int id)
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("Default");
            var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<string>(itemResult);
            }
            var item = itemResult.Object;
            var methodResult = item.ObfuscateEmail(Db);
            await Db.SaveChangesAsync();
            var result = new ItemResult<string>();
            result.Object = methodResult;
            return result;
        }

        /// <summary>
        /// Method: ChangeFirstName
        /// </summary>
        [HttpPatch("ChangeFirstName")]
        [Authorize]
        public virtual async Task<ItemResult<PersonDtoGen>> ChangeFirstName([FromServices] IDataSourceFactory dataSourceFactory, int id, string firstName, Coalesce.Domain.Person.Titles? title)
        {
            IncludeTree includeTree = null;
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("Default");
            var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<PersonDtoGen>(itemResult);
            }
            var item = itemResult.Object;
            var methodResult = item.ChangeFirstName(firstName, title);
            await Db.SaveChangesAsync();
            var result = new ItemResult<PersonDtoGen>();
            var mappingContext = new MappingContext(User, "");
            result.Object = Mapper.MapToDto<Coalesce.Domain.Person, PersonDtoGen>(methodResult, mappingContext, includeTree);
            return result;
        }

        /// <summary>
        /// Method: GetUserPublic
        /// </summary>
        [HttpPost("GetUserPublic")]
        [Authorize]
        public virtual ItemResult<string> GetUserPublic()
        {
            var methodResult = Coalesce.Domain.Person.GetUserPublic(User);
            var result = new ItemResult<string>();
            result.Object = methodResult;
            return result;
        }

        /// <summary>
        /// Method: NamesStartingWith
        /// </summary>
        [HttpPost("NamesStartingWith")]
        [Authorize]
        public virtual ItemResult<ICollection<string>> NamesStartingWith(string characters)
        {
            var methodResult = Coalesce.Domain.Person.NamesStartingWith(Db, characters);
            var result = new ItemResult<ICollection<string>>();
            result.Object = methodResult?.ToList();
            return result;
        }

        /// <summary>
        /// Method: SearchPeople
        /// </summary>
        [HttpPost("SearchPeople")]
        [Authorize]
        public virtual ListResult<PersonDtoGen> SearchPeople(PersonCriteriaDtoGen criteria, int page)
        {
            IncludeTree includeTree = null;
            var methodResult = Coalesce.Domain.Person.SearchPeople(Db, criteria.MapToModel(new Coalesce.Domain.PersonCriteria(), new MappingContext(User)), page);
            var result = new ListResult<PersonDtoGen>(methodResult);
            var mappingContext = new MappingContext(User, "");
            result.List = methodResult.List?.ToList().Select(o => Mapper.MapToDto<Coalesce.Domain.Person, PersonDtoGen>(o, mappingContext, includeTree)).ToList();
            return result;
        }
    }
}
