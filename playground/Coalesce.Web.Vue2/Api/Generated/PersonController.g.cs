
using Coalesce.Web.Vue2.Models;
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

namespace Coalesce.Web.Vue2.Api
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
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.Person> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [AllowAnonymous]
        public virtual Task<ListResult<PersonResponse>> List(
            [FromQuery] ListParameters parameters,
            IDataSource<Coalesce.Domain.Person> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [AllowAnonymous]
        public virtual Task<ItemResult<int>> Count(
            [FromQuery] FilterParameters parameters,
            IDataSource<Coalesce.Domain.Person> dataSource)
            => CountImplementation(parameters, dataSource);

        [HttpPost("save")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        [AllowAnonymous]
        public virtual Task<ItemResult<PersonResponse>> Save(
            [FromForm] PersonParameter dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.Person> dataSource,
            IBehaviors<Coalesce.Domain.Person> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

        [HttpPost("save")]
        [Consumes("application/json")]
        [AllowAnonymous]
        public virtual Task<ItemResult<PersonResponse>> SaveFromJson(
            [FromBody] PersonParameter dto,
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
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual async Task<ItemResult<PersonResponse>> Rename(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromForm(Name = "id")] int id,
            [FromForm(Name = "name")] string name)
        {
            var _params = new
            {
                Id = id,
                Name = name
            };

            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("Default");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<PersonResponse>(itemResult);
            }
            var item = itemResult.Object;
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("Rename"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<PersonResponse>(_validationResult);
            }

            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = item.Rename(
                Db,
                _params.Name,
                out includeTree
            );
            var _result = new ItemResult<PersonResponse>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.Person, PersonResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        }

        public class RenameParameters
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        /// <summary>
        /// Method: Rename
        /// </summary>
        [HttpPost("Rename")]
        [Authorize]
        [Consumes("application/json")]
        public virtual async Task<ItemResult<PersonResponse>> Rename(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromBody] RenameParameters _params
        )
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("Default");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<PersonResponse>(itemResult);
            }
            var item = itemResult.Object;
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("Rename"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<PersonResponse>(_validationResult);
            }

            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = item.Rename(
                Db,
                _params.Name,
                out includeTree
            );
            var _result = new ItemResult<PersonResponse>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.Person, PersonResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        }

        /// <summary>
        /// Method: ChangeSpacesToDashesInName
        /// </summary>
        [HttpPost("ChangeSpacesToDashesInName")]
        [Authorize]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual async Task<ItemResult> ChangeSpacesToDashesInName(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromForm(Name = "id")] int id)
        {
            var _params = new
            {
                Id = id
            };

            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("WithoutCases");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult(itemResult);
            }
            var item = itemResult.Object;
            var _methodResult = item.ChangeSpacesToDashesInName(
                Db
            );
            var _result = new ItemResult(_methodResult);
            return _result;
        }

        public class ChangeSpacesToDashesInNameParameters
        {
            public int Id { get; set; }
        }

        /// <summary>
        /// Method: ChangeSpacesToDashesInName
        /// </summary>
        [HttpPost("ChangeSpacesToDashesInName")]
        [Authorize]
        [Consumes("application/json")]
        public virtual async Task<ItemResult> ChangeSpacesToDashesInName(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromBody] ChangeSpacesToDashesInNameParameters _params
        )
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("WithoutCases");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult(itemResult);
            }
            var item = itemResult.Object;
            var _methodResult = item.ChangeSpacesToDashesInName(
                Db
            );
            var _result = new ItemResult(_methodResult);
            return _result;
        }

        /// <summary>
        /// Method: Add
        /// </summary>
        [HttpPost("Add")]
        [Authorize]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual ItemResult<int> Add(
            [FromForm(Name = "numberOne")] int numberOne,
            [FromForm(Name = "numberTwo")] int numberTwo = 42)
        {
            var _params = new
            {
                NumberOne = numberOne,
                NumberTwo = numberTwo
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("Add"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<int>(_validationResult);
            }

            var _methodResult = Coalesce.Domain.Person.Add(
                _params.NumberOne,
                _params.NumberTwo
            );
            var _result = new ItemResult<int>(_methodResult);
            _result.Object = _methodResult.Object;
            return _result;
        }

        public class AddParameters
        {
            public int NumberOne { get; set; }
            public int NumberTwo { get; set; } = 42;
        }

        /// <summary>
        /// Method: Add
        /// </summary>
        [HttpPost("Add")]
        [Authorize]
        [Consumes("application/json")]
        public virtual ItemResult<int> Add(
            [FromBody] AddParameters _params
        )
        {
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("Add"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<int>(_validationResult);
            }

            var _methodResult = Coalesce.Domain.Person.Add(
                _params.NumberOne,
                _params.NumberTwo
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
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual async Task<ItemResult<System.DateTime>> GetBirthdate(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromForm(Name = "id")] int id)
        {
            var _params = new
            {
                Id = id
            };

            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("Default");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<System.DateTime>(itemResult);
            }
            var item = itemResult.Object;
            var _methodResult = item.GetBirthdate();
            var _result = new ItemResult<System.DateTime>();
            _result.Object = _methodResult;
            return _result;
        }

        public class GetBirthdateParameters
        {
            public int Id { get; set; }
        }

        /// <summary>
        /// Method: GetBirthdate
        /// </summary>
        [HttpPost("GetBirthdate")]
        [Authorize]
        [Consumes("application/json")]
        public virtual async Task<ItemResult<System.DateTime>> GetBirthdate(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromBody] GetBirthdateParameters _params
        )
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("Default");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<System.DateTime>(itemResult);
            }
            var item = itemResult.Object;
            var _methodResult = item.GetBirthdate();
            var _result = new ItemResult<System.DateTime>();
            _result.Object = _methodResult;
            return _result;
        }

        /// <summary>
        /// Method: SetBirthDate
        /// </summary>
        [HttpPost("SetBirthDate")]
        [Authorize]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual async Task<ItemResult> SetBirthDate(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromForm(Name = "id")] int id,
            [FromForm(Name = "date")] System.DateOnly date,
            [FromForm(Name = "time")] System.TimeOnly time)
        {
            var _params = new
            {
                Id = id,
                Date = date,
                Time = time
            };

            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("Default");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult(itemResult);
            }
            var item = itemResult.Object;
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("SetBirthDate"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return _validationResult;
            }

            item.SetBirthDate(
                Db,
                _params.Date,
                _params.Time
            );
            var _result = new ItemResult();
            return _result;
        }

        public class SetBirthDateParameters
        {
            public int Id { get; set; }
            public System.DateOnly Date { get; set; }
            public System.TimeOnly Time { get; set; }
        }

        /// <summary>
        /// Method: SetBirthDate
        /// </summary>
        [HttpPost("SetBirthDate")]
        [Authorize]
        [Consumes("application/json")]
        public virtual async Task<ItemResult> SetBirthDate(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromBody] SetBirthDateParameters _params
        )
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("Default");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult(itemResult);
            }
            var item = itemResult.Object;
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("SetBirthDate"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return _validationResult;
            }

            item.SetBirthDate(
                Db,
                _params.Date,
                _params.Time
            );
            var _result = new ItemResult();
            return _result;
        }

        /// <summary>
        /// Method: PersonCount
        /// </summary>
        [HttpGet("PersonCount")]
        [Authorize]
        public virtual ItemResult<long> PersonCount(
            [FromQuery] string lastNameStartsWith = "")
        {
            var _params = new
            {
                LastNameStartsWith = lastNameStartsWith
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("PersonCount"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<long>(_validationResult);
            }

            var _methodResult = Coalesce.Domain.Person.PersonCount(
                Db,
                _params.LastNameStartsWith
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
            [FromQuery] int id)
        {
            var _params = new
            {
                Id = id
            };

            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("Default");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<string>(itemResult);
            }
            var item = itemResult.Object;
            var _methodResult = item.FullNameAndAge(
                Db
            );
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
            [FromQuery] int id)
        {
            var _params = new
            {
                Id = id
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("RemovePersonById"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<bool>(_validationResult);
            }

            var _methodResult = Coalesce.Domain.Person.RemovePersonById(
                Db,
                _params.Id
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
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual async Task<ItemResult<string>> ObfuscateEmail(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromForm(Name = "id")] int id)
        {
            var _params = new
            {
                Id = id
            };

            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("Default");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<string>(itemResult);
            }
            var item = itemResult.Object;
            var _methodResult = item.ObfuscateEmail(
                Db
            );
            var _result = new ItemResult<string>();
            _result.Object = _methodResult;
            return _result;
        }

        public class ObfuscateEmailParameters
        {
            public int Id { get; set; }
        }

        /// <summary>
        /// Method: ObfuscateEmail
        /// </summary>
        [HttpPut("ObfuscateEmail")]
        [Authorize]
        [Consumes("application/json")]
        public virtual async Task<ItemResult<string>> ObfuscateEmail(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromBody] ObfuscateEmailParameters _params
        )
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("Default");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<string>(itemResult);
            }
            var item = itemResult.Object;
            var _methodResult = item.ObfuscateEmail(
                Db
            );
            var _result = new ItemResult<string>();
            _result.Object = _methodResult;
            return _result;
        }

        /// <summary>
        /// Method: ChangeFirstName
        /// </summary>
        [HttpPatch("ChangeFirstName")]
        [Authorize]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual async Task<ItemResult<PersonResponse>> ChangeFirstName(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromForm(Name = "id")] int id,
            [FromForm(Name = "firstName")] string firstName,
            [FromForm(Name = "title")] Coalesce.Domain.Person.Titles? title)
        {
            var _params = new
            {
                Id = id,
                FirstName = firstName,
                Title = title
            };

            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("Default");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<PersonResponse>(itemResult);
            }
            var item = itemResult.Object;
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("ChangeFirstName"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<PersonResponse>(_validationResult);
            }

            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = item.ChangeFirstName(
                Db,
                _params.FirstName,
                _params.Title
            );
            var _result = new ItemResult<PersonResponse>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.Person, PersonResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        }

        public class ChangeFirstNameParameters
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public Coalesce.Domain.Person.Titles? Title { get; set; }
        }

        /// <summary>
        /// Method: ChangeFirstName
        /// </summary>
        [HttpPatch("ChangeFirstName")]
        [Authorize]
        [Consumes("application/json")]
        public virtual async Task<ItemResult<PersonResponse>> ChangeFirstName(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromBody] ChangeFirstNameParameters _params
        )
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Person, Coalesce.Domain.Person>("Default");
            var itemResult = await dataSource.GetItemAsync(_params.Id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<PersonResponse>(itemResult);
            }
            var item = itemResult.Object;
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("ChangeFirstName"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<PersonResponse>(_validationResult);
            }

            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = item.ChangeFirstName(
                Db,
                _params.FirstName,
                _params.Title
            );
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
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual ItemResult<System.Collections.Generic.ICollection<string>> NamesStartingWith(
            [FromForm(Name = "characters")] string characters)
        {
            var _params = new
            {
                Characters = characters
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("NamesStartingWith"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<System.Collections.Generic.ICollection<string>>(_validationResult);
            }

            var _methodResult = Coalesce.Domain.Person.NamesStartingWith(
                Db,
                _params.Characters
            );
            var _result = new ItemResult<System.Collections.Generic.ICollection<string>>();
            _result.Object = _methodResult?.ToList();
            return _result;
        }

        public class NamesStartingWithParameters
        {
            public string Characters { get; set; }
        }

        /// <summary>
        /// Method: NamesStartingWith
        /// </summary>
        [HttpPost("NamesStartingWith")]
        [Authorize]
        [Consumes("application/json")]
        public virtual ItemResult<System.Collections.Generic.ICollection<string>> NamesStartingWith(
            [FromBody] NamesStartingWithParameters _params
        )
        {
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("NamesStartingWith"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<System.Collections.Generic.ICollection<string>>(_validationResult);
            }

            var _methodResult = Coalesce.Domain.Person.NamesStartingWith(
                Db,
                _params.Characters
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
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual ItemResult<string[]> MethodWithStringArrayParameter(
            [FromForm(Name = "strings")] string[] strings)
        {
            var _params = new
            {
                Strings = strings.ToList()
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("MethodWithStringArrayParameter"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<string[]>(_validationResult);
            }

            var _methodResult = Coalesce.Domain.Person.MethodWithStringArrayParameter(
                Db,
                _params.Strings.ToArray()
            );
            var _result = new ItemResult<string[]>();
            _result.Object = _methodResult?.ToArray();
            return _result;
        }

        public class MethodWithStringArrayParameterParameters
        {
            public string[] Strings { get; set; }
        }

        /// <summary>
        /// Method: MethodWithStringArrayParameter
        /// </summary>
        [HttpPost("MethodWithStringArrayParameter")]
        [Authorize]
        [Consumes("application/json")]
        public virtual ItemResult<string[]> MethodWithStringArrayParameter(
            [FromBody] MethodWithStringArrayParameterParameters _params
        )
        {
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("MethodWithStringArrayParameter"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<string[]>(_validationResult);
            }

            var _methodResult = Coalesce.Domain.Person.MethodWithStringArrayParameter(
                Db,
                _params.Strings.ToArray()
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
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual ItemResult<PersonResponse> MethodWithEntityParameter(
            [FromForm(Name = "person")] PersonParameter person)
        {
            var _params = new
            {
                Person = !Request.Form.HasAnyValue(nameof(person)) ? null : person
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
                _params.Person?.MapToNew(_mappingContext)
            );
            var _result = new ItemResult<PersonResponse>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.Person, PersonResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        }

        public class MethodWithEntityParameterParameters
        {
            public PersonParameter Person { get; set; }
        }

        /// <summary>
        /// Method: MethodWithEntityParameter
        /// </summary>
        [HttpPost("MethodWithEntityParameter")]
        [Authorize]
        [Consumes("application/json")]
        public virtual ItemResult<PersonResponse> MethodWithEntityParameter(
            [FromBody] MethodWithEntityParameterParameters _params
        )
        {
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
                _params.Person?.MapToNew(_mappingContext)
            );
            var _result = new ItemResult<PersonResponse>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.Person, PersonResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        }

        /// <summary>
        /// Method: MethodWithOptionalEntityParameter
        /// </summary>
        [HttpPost("MethodWithOptionalEntityParameter")]
        [Authorize]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual ItemResult<PersonResponse> MethodWithOptionalEntityParameter(
            [FromForm(Name = "person")] PersonParameter person)
        {
            var _params = new
            {
                Person = !Request.Form.HasAnyValue(nameof(person)) ? null : person
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("MethodWithOptionalEntityParameter"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<PersonResponse>(_validationResult);
            }

            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = Coalesce.Domain.Person.MethodWithOptionalEntityParameter(
                Db,
                _params.Person?.MapToNew(_mappingContext)
            );
            var _result = new ItemResult<PersonResponse>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.Person, PersonResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        }

        public class MethodWithOptionalEntityParameterParameters
        {
            public PersonParameter Person { get; set; }
        }

        /// <summary>
        /// Method: MethodWithOptionalEntityParameter
        /// </summary>
        [HttpPost("MethodWithOptionalEntityParameter")]
        [Authorize]
        [Consumes("application/json")]
        public virtual ItemResult<PersonResponse> MethodWithOptionalEntityParameter(
            [FromBody] MethodWithOptionalEntityParameterParameters _params
        )
        {
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("MethodWithOptionalEntityParameter"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<PersonResponse>(_validationResult);
            }

            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = Coalesce.Domain.Person.MethodWithOptionalEntityParameter(
                Db,
                _params.Person?.MapToNew(_mappingContext)
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
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual ListResult<PersonResponse> SearchPeople(
            [FromForm(Name = "criteria")] PersonCriteriaParameter criteria,
            [FromForm(Name = "page")] int page)
        {
            var _params = new
            {
                Criteria = !Request.Form.HasAnyValue(nameof(criteria)) ? null : criteria,
                Page = page
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
                _params.Criteria?.MapToNew(_mappingContext),
                _params.Page
            );
            var _result = new ListResult<PersonResponse>(_methodResult);
            _result.List = _methodResult.List?.ToList().Select(o => Mapper.MapToDto<Coalesce.Domain.Person, PersonResponse>(o, _mappingContext, includeTree ?? _methodResult.IncludeTree)).ToList();
            return _result;
        }

        public class SearchPeopleParameters
        {
            public PersonCriteriaParameter Criteria { get; set; }
            public int Page { get; set; }
        }

        /// <summary>
        /// Method: SearchPeople
        /// </summary>
        [HttpPost("SearchPeople")]
        [Authorize]
        [Consumes("application/json")]
        public virtual ListResult<PersonResponse> SearchPeople(
            [FromBody] SearchPeopleParameters _params
        )
        {
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
                _params.Criteria?.MapToNew(_mappingContext),
                _params.Page
            );
            var _result = new ListResult<PersonResponse>(_methodResult);
            _result.List = _methodResult.List?.ToList().Select(o => Mapper.MapToDto<Coalesce.Domain.Person, PersonResponse>(o, _mappingContext, includeTree ?? _methodResult.IncludeTree)).ToList();
            return _result;
        }
    }
}
