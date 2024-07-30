
using Coalesce.Web.Ko.Models;
using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Api.Behaviors;
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

namespace Coalesce.Web.Ko.Api
{
    [Route("api/Person")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class PersonController
        : BaseApiController<Coalesce.Domain.Person, PersonParameter, PersonResponse, Coalesce.Domain.AppDbContext>
    {
        public PersonController(CrudContext<Coalesce.Domain.AppDbContext> context) : base(context)
        {
            GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<Coalesce.Domain.Person>();
        }

        [HttpGet("get/{id}")]
        [AllowAnonymous]
        public virtual Task<ItemResult<PersonResponse>> Get(
            int id,
            DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.Person> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [AllowAnonymous]
        public virtual Task<ListResult<PersonResponse>> List(
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
        public virtual Task<ItemResult<PersonResponse>> Save(
            [FromForm] PersonParameter dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.Person> dataSource,
            IBehaviors<Coalesce.Domain.Person> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

        [HttpPost("bulkSave")]
        [AllowAnonymous]
        public virtual Task<ItemResult<PersonResponse>> BulkSave(
            [FromBody] BulkSaveRequest dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.Person> dataSource,
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromServices] IBehaviorsFactory behaviorsFactory)
            => BulkSaveImplementation(dto, parameters, dataSource, dataSourceFactory, behaviorsFactory);

        [HttpPost("delete/{id}")]
        [Authorize]
        public virtual Task<ItemResult<PersonResponse>> Delete(
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
        public virtual async Task<ItemResult<PersonResponse>> Rename(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromForm(Name = "id")] int id,
            [FromForm(Name = "name")] string name)
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("Default");
            var itemResult = await dataSource.GetItemAsync(id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<PersonResponse>(itemResult);
            }
            var item = itemResult.Object;
            var _params = new
            {
                name = name
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("Rename"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<PersonResponse>(_validationResult);
            }

            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = item.Rename(
                _params.name,
                out includeTree
            );
            await Db.SaveChangesAsync();
            var _result = new ItemResult<PersonResponse>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.Person, PersonResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        }

        /// <summary>
        /// Method: ChangeSpacesToDashesInName
        /// </summary>
        [HttpPost("ChangeSpacesToDashesInName")]
        [Authorize]
        public virtual async Task<ItemResult> ChangeSpacesToDashesInName(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromForm(Name = "id")] int id)
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("WithoutCases");
            var itemResult = await dataSource.GetItemAsync(id, new DataSourceParameters());
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
        public virtual ItemResult<int> Add(
            [FromForm(Name = "numberOne")] int numberOne,
            [FromForm(Name = "numberTwo")] int numberTwo = 42)
        {
            var _params = new
            {
                numberOne = numberOne,
                numberTwo = numberTwo
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("Add"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<int>(_validationResult);
            }

            var _methodResult = Coalesce.Domain.Person.Add(
                _params.numberOne,
                _params.numberTwo
            );
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
            var _methodResult = Coalesce.Domain.Person.GetUser(
                User
            );
            var _result = new ItemResult<string>();
            _result.Object = _methodResult;
            return _result;
        }

        /// <summary>
        /// Method: GetBirthdate
        /// </summary>
        [HttpPost("GetBirthdate")]
        [Authorize]
        public virtual async Task<ItemResult<System.DateTime>> GetBirthdate(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromForm(Name = "id")] int id)
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("Default");
            var itemResult = await dataSource.GetItemAsync(id, new DataSourceParameters());
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
        /// Method: SetBirthDate
        /// </summary>
        [HttpPost("SetBirthDate")]
        [Authorize]
        public virtual async Task<ItemResult> SetBirthDate(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromForm(Name = "id")] int id,
            [FromForm(Name = "date")] System.DateOnly date,
            [FromForm(Name = "time")] System.TimeOnly time)
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("Default");
            var itemResult = await dataSource.GetItemAsync(id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult(itemResult);
            }
            var item = itemResult.Object;
            var _params = new
            {
                date = date,
                time = time
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("SetBirthDate"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return _validationResult;
            }

            item.SetBirthDate(
                _params.date,
                _params.time
            );
            await Db.SaveChangesAsync();
            var _result = new ItemResult();
            return _result;
        }

        /// <summary>
        /// Method: PersonCount
        /// </summary>
        [HttpGet("PersonCount")]
        [Authorize]
        public virtual ItemResult<long> PersonCount(
            string lastNameStartsWith = "")
        {
            var _params = new
            {
                lastNameStartsWith = lastNameStartsWith
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("PersonCount"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<long>(_validationResult);
            }

            var _methodResult = Coalesce.Domain.Person.PersonCount(
                Db,
                _params.lastNameStartsWith
            );
            var _result = new ItemResult<long>();
            _result.Object = _methodResult;
            return _result;
        }

        /// <summary>
        /// Method: FullNameAndAge
        /// </summary>
        [HttpGet("FullNameAndAge")]
        [Authorize]
        public virtual async Task<ItemResult<string>> FullNameAndAge(
            [FromServices] IDataSourceFactory dataSourceFactory,
            int id)
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("Default");
            var itemResult = await dataSource.GetItemAsync(id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<string>(itemResult);
            }
            var item = itemResult.Object;
            var _methodResult = item.FullNameAndAge(
                Db
            );
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
        public virtual ItemResult<bool> RemovePersonById(
            int id)
        {
            var _params = new
            {
                id = id
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("RemovePersonById"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<bool>(_validationResult);
            }

            var _methodResult = Coalesce.Domain.Person.RemovePersonById(
                Db,
                _params.id
            );
            var _result = new ItemResult<bool>();
            _result.Object = _methodResult;
            return _result;
        }

        /// <summary>
        /// Method: ObfuscateEmail
        /// </summary>
        [HttpPut("ObfuscateEmail")]
        [Authorize]
        public virtual async Task<ItemResult<string>> ObfuscateEmail(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromForm(Name = "id")] int id)
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("Default");
            var itemResult = await dataSource.GetItemAsync(id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<string>(itemResult);
            }
            var item = itemResult.Object;
            var _methodResult = item.ObfuscateEmail(
                Db
            );
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
        public virtual async Task<ItemResult<PersonResponse>> ChangeFirstName(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromForm(Name = "id")] int id,
            [FromForm(Name = "firstName")] string firstName,
            [FromForm(Name = "title")] Coalesce.Domain.Person.Titles? title)
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("Default");
            var itemResult = await dataSource.GetItemAsync(id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<PersonResponse>(itemResult);
            }
            var item = itemResult.Object;
            var _params = new
            {
                firstName = firstName,
                title = title
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("ChangeFirstName"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<PersonResponse>(_validationResult);
            }

            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = item.ChangeFirstName(
                _params.firstName,
                _params.title
            );
            await Db.SaveChangesAsync();
            var _result = new ItemResult<PersonResponse>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.Person, PersonResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        }

        /// <summary>
        /// Method: GetUserPublic
        /// </summary>
        [HttpPost("GetUserPublic")]
        [Authorize]
        public virtual ItemResult<string> GetUserPublic()
        {
            var _methodResult = Coalesce.Domain.Person.GetUserPublic(
                User
            );
            var _result = new ItemResult<string>();
            _result.Object = _methodResult;
            return _result;
        }

        /// <summary>
        /// Method: NamesStartingWith
        /// </summary>
        [HttpPost("NamesStartingWith")]
        [Authorize]
        public virtual ItemResult<System.Collections.Generic.ICollection<string>> NamesStartingWith(
            [FromForm(Name = "characters")] string characters)
        {
            var _params = new
            {
                characters = characters
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("NamesStartingWith"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<System.Collections.Generic.ICollection<string>>(_validationResult);
            }

            var _methodResult = Coalesce.Domain.Person.NamesStartingWith(
                Db,
                _params.characters
            );
            var _result = new ItemResult<System.Collections.Generic.ICollection<string>>();
            _result.Object = _methodResult?.ToList();
            return _result;
        }

        /// <summary>
        /// Method: MethodWithStringArrayParameter
        /// </summary>
        [HttpPost("MethodWithStringArrayParameter")]
        [Authorize]
        public virtual ItemResult<string[]> MethodWithStringArrayParameter(
            [FromForm(Name = "strings")] string[] strings)
        {
            var _params = new
            {
                strings = strings.ToList()
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("MethodWithStringArrayParameter"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<string[]>(_validationResult);
            }

            var _methodResult = Coalesce.Domain.Person.MethodWithStringArrayParameter(
                Db,
                _params.strings.ToArray()
            );
            var _result = new ItemResult<string[]>();
            _result.Object = _methodResult?.ToArray();
            return _result;
        }

        /// <summary>
        /// Method: MethodWithEntityParameter
        /// </summary>
        [HttpPost("MethodWithEntityParameter")]
        [Authorize]
        public virtual ItemResult<PersonResponse> MethodWithEntityParameter(
            [FromForm(Name = "person")] PersonParameter person)
        {
            var _params = new
            {
                person = person
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("MethodWithEntityParameter"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<PersonResponse>(_validationResult);
            }

            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = Coalesce.Domain.Person.MethodWithEntityParameter(
                Db,
                _params.person.MapToNew(_mappingContext)
            );
            var _result = new ItemResult<PersonResponse>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.Person, PersonResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        }

        /// <summary>
        /// Method: SearchPeople
        /// </summary>
        [HttpPost("SearchPeople")]
        [Authorize]
        public virtual ListResult<PersonResponse> SearchPeople(
            [FromForm(Name = "criteria")] PersonCriteriaParameter criteria,
            [FromForm(Name = "page")] int page)
        {
            var _params = new
            {
                criteria = criteria,
                page = page
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("SearchPeople"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ListResult<PersonResponse>(_validationResult);
            }

            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = Coalesce.Domain.Person.SearchPeople(
                Db,
                _params.criteria.MapToNew(_mappingContext),
                _params.page
            );
            var _result = new ListResult<PersonResponse>(_methodResult);
            _result.List = _methodResult.List?.ToList().Select(o => Mapper.MapToDto<Coalesce.Domain.Person, PersonResponse>(o, _mappingContext, includeTree ?? _methodResult.IncludeTree)).ToList();
            return _result;
        }
    }
}
