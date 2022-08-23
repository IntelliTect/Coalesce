
using Coalesce.Web.Vue2.Models;
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

namespace Coalesce.Web.Vue2.Api
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

        // Methods from data class exposed through API Controller.

        /// <summary>
        /// Method: Rename
        /// </summary>
        [HttpPost("Rename")]
        [Authorize]
        public virtual async Task<ItemResult<PersonDtoGen>> Rename([FromServices] IDataSourceFactory dataSourceFactory, int id, string name)
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("Default");
            var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<PersonDtoGen>(itemResult);
            }
            var item = itemResult.Object;
            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(User);
            var _methodResult = item.Rename(name, out includeTree);
            await Db.SaveChangesAsync();
            var _result = new ItemResult<PersonDtoGen>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.Person, PersonDtoGen>(_methodResult, _mappingContext, includeTree);
            return _result;
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
            var _methodResult = item.ChangeSpacesToDashesInName();
            await Db.SaveChangesAsync();
            var _result = new ItemResult(_methodResult);
            return _result;
        }

        /// <summary>
        /// Method: Add
        /// </summary>
        [HttpPost("Add")]
        [Authorize]
        public virtual ItemResult<int> Add(int numberOne, int numberTwo)
        {
            var _methodResult = Coalesce.Domain.Person.Add(numberOne, numberTwo);
            var _result = new ItemResult<int>(_methodResult);
            _result.Object = _methodResult.Object;
            return _result;
        }

        /// <summary>
        /// Method: GetUser
        /// </summary>
        [HttpPost("GetUser")]
        [Authorize(Roles = "Admin")]
        public virtual ItemResult<string> GetUser()
        {
            var _methodResult = Coalesce.Domain.Person.GetUser(User);
            var _result = new ItemResult<string>();
            _result.Object = _methodResult;
            return _result;
        }

        /// <summary>
        /// Method: GetBirthdate
        /// </summary>
        [HttpPost("GetBirthdate")]
        [Authorize]
        public virtual async Task<ItemResult<System.DateTime>> GetBirthdate([FromServices] IDataSourceFactory dataSourceFactory, int id)
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("Default");
            var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<System.DateTime>(itemResult);
            }
            var item = itemResult.Object;
            var _methodResult = item.GetBirthdate();
            await Db.SaveChangesAsync();
            var _result = new ItemResult<System.DateTime>();
            _result.Object = _methodResult;
            return _result;
        }

        /// <summary>
        /// Method: PersonCount
        /// </summary>
        [HttpGet("PersonCount")]
        [Authorize]
        public virtual ItemResult<long> PersonCount(string lastNameStartsWith = "")
        {
            var _methodResult = Coalesce.Domain.Person.PersonCount(Db, lastNameStartsWith);
            var _result = new ItemResult<long>();
            _result.Object = _methodResult;
            return _result;
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
            var _methodResult = item.FullNameAndAge(Db);
            await Db.SaveChangesAsync();
            var _result = new ItemResult<string>();
            _result.Object = _methodResult;
            return _result;
        }

        /// <summary>
        /// Method: RemovePersonById
        /// </summary>
        [HttpDelete("RemovePersonById")]
        [Authorize]
        public virtual ItemResult<bool> RemovePersonById(int id)
        {
            var _methodResult = Coalesce.Domain.Person.RemovePersonById(Db, id);
            var _result = new ItemResult<bool>();
            _result.Object = _methodResult;
            return _result;
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
            var _methodResult = item.ObfuscateEmail(Db);
            await Db.SaveChangesAsync();
            var _result = new ItemResult<string>();
            _result.Object = _methodResult;
            return _result;
        }

        /// <summary>
        /// Method: ChangeFirstName
        /// </summary>
        [HttpPatch("ChangeFirstName")]
        [Authorize]
        public virtual async Task<ItemResult<PersonDtoGen>> ChangeFirstName([FromServices] IDataSourceFactory dataSourceFactory, int id, string firstName, Coalesce.Domain.Person.Titles? title)
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("Default");
            var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<PersonDtoGen>(itemResult);
            }
            var item = itemResult.Object;
            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(User);
            var _methodResult = item.ChangeFirstName(firstName, title);
            await Db.SaveChangesAsync();
            var _result = new ItemResult<PersonDtoGen>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.Person, PersonDtoGen>(_methodResult, _mappingContext, includeTree);
            return _result;
        }

        /// <summary>
        /// Method: GetUserPublic
        /// </summary>
        [HttpPost("GetUserPublic")]
        [Authorize]
        public virtual ItemResult<string> GetUserPublic()
        {
            var _methodResult = Coalesce.Domain.Person.GetUserPublic(User);
            var _result = new ItemResult<string>();
            _result.Object = _methodResult;
            return _result;
        }

        /// <summary>
        /// Method: NamesStartingWith
        /// </summary>
        [HttpPost("NamesStartingWith")]
        [Authorize]
        public virtual ItemResult<System.Collections.Generic.ICollection<string>> NamesStartingWith(string characters)
        {
            var _methodResult = Coalesce.Domain.Person.NamesStartingWith(Db, characters);
            var _result = new ItemResult<System.Collections.Generic.ICollection<string>>();
            _result.Object = _methodResult?.ToList();
            return _result;
        }

        /// <summary>
        /// Method: MethodWithStringArrayParameter
        /// </summary>
        [HttpPost("MethodWithStringArrayParameter")]
        [Authorize]
        public virtual ItemResult<string[]> MethodWithStringArrayParameter(string[] strings)
        {
            var _methodResult = Coalesce.Domain.Person.MethodWithStringArrayParameter(Db, strings.ToArray());
            var _result = new ItemResult<string[]>();
            _result.Object = _methodResult?.ToArray();
            return _result;
        }

        /// <summary>
        /// Method: MethodWithEntityParameter
        /// </summary>
        [HttpPost("MethodWithEntityParameter")]
        [Authorize]
        public virtual ItemResult<PersonDtoGen> MethodWithEntityParameter(PersonDtoGen person)
        {
            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(User);
            var _methodResult = Coalesce.Domain.Person.MethodWithEntityParameter(Db, person.MapToModel(new Coalesce.Domain.Person(), _mappingContext));
            var _result = new ItemResult<PersonDtoGen>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.Person, PersonDtoGen>(_methodResult, _mappingContext, includeTree);
            return _result;
        }

        /// <summary>
        /// Method: SearchPeople
        /// </summary>
        [HttpPost("SearchPeople")]
        [Authorize]
        public virtual ListResult<PersonDtoGen> SearchPeople(PersonCriteriaDtoGen criteria, int page)
        {
            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(User);
            var _methodResult = Coalesce.Domain.Person.SearchPeople(Db, criteria.MapToModel(new Coalesce.Domain.PersonCriteria(), _mappingContext), page);
            var _result = new ListResult<PersonDtoGen>(_methodResult);
            _result.List = _methodResult.List?.ToList().Select(o => Mapper.MapToDto<Coalesce.Domain.Person, PersonDtoGen>(o, _mappingContext, includeTree)).ToList();
            return _result;
        }
    }
}
