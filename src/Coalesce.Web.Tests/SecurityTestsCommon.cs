using Coalesce.Web.Tests.Helpers;
using IntelliTect.Coalesce.TypeDefinition;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Coalesce.Web.Tests
{
    public class SecurityTestsCommon : IDisposable
    {
        private HttpClientHandler _handler;

        public SecurityTestsCommon(TestModel testModel, StreamWriter output, string userRole, string messagePrefix)
        {
            Model = testModel;
            OutputFile = output;
            MessagePrefix = messagePrefix;

            var roles = string.IsNullOrEmpty(userRole) ? new string[] { } : new string[] { userRole };
            User = new GenericPrincipal(new GenericIdentity("AnonUser"), roles);

            _handler = new HttpClientHandler();
            if (!string.IsNullOrEmpty(userRole))
            {
                var securityCookieContainer = new CookieContainer();
                securityCookieContainer.Add(Model.ApiUrl, new Cookie("SecurityTestRole", userRole));
                _handler.CookieContainer = securityCookieContainer;
            }

            Client = new HttpClient(_handler);
            Client.BaseAddress = Model.ApiUrl;
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void Dispose()
        {
            Client.Dispose();
            _handler.Dispose();
        }

        protected HttpClient Client { get; set; }
        protected string MessagePrefix { get; set; }
        protected StreamWriter OutputFile { get; set; }
        protected ClaimsPrincipal User { get; set; }
        protected TestModel Model { get; set; }

        public virtual async Task<bool> RunTests()
        {
            var results = new List<bool>();

            // The following tests are self-contained.  The only properties that we will attempt to read/edit
            //      are ones the current user can't access for that purpose.
            results.Add(await RestrictedReadPropertiesViaPropertyValues());
            results.Add(await RestrictedEditProperties());

            return results.All(b => b);
        }

        public async Task<bool> RestrictedReadPropertiesViaPropertyValues()
        {
            // Get a list of properties on anonmymous models that have read restrictions
            var models = Model.Models
                            .Where(m => m.SecurityInfo.AllowAnonymousAny)
                            .SelectMany(m => m.Properties
                                                .Where(p => !p.SecurityInfo.IsReadable(User))
                                                .Select(p => new { Model = m, Property = p }))
                            .ToList();

            var tasks = models
                        .Select(async m => await AttemptToGetRestrictedPropertyValues(m.Model, m.Property));
            var results = await Task.WhenAll(tasks);
            return results.All(b => b);
        }

        public async Task<bool> RestrictedEditProperties()
        {
            // Get a list of properties on anonmymous models that have edit restrictions
            var models = Model.Models
                            .Where(m => m.SecurityInfo.Edit.AllowAnonymous)
                            .SelectMany(m => m.Properties
                                                .Where(p => !p.SecurityInfo.IsEditable(User) && p.SecurityInfo.IsReadable(User))
                                                .Select(p => new { Model = m, Property = p }))
                            .ToList();

            var tasks = models
                        .Select(async m => await AttemptToChangeRestrictedProperty(m.Model, m.Property));
            var results = await Task.WhenAll(tasks);
            return results.All(b => b);
        }

        public async Task<bool> RoleRestrictedCreate(HttpStatusCode expectedStatus)
        {
            var response = await CreateProduct();
            return response.StatusCode == expectedStatus;
        }

        public async Task<bool> AuthorizedCreate(HttpStatusCode expectedStatus)
        {
            var response = await CreatePerson();
            return response.StatusCode == expectedStatus;
        }

        public async Task<bool> AnonymousCreate(HttpStatusCode expectedStatus)
        {
            var response = await CreateCase();
            return response.StatusCode == expectedStatus;
        }

        public async Task<bool> NotAllowedCreate(HttpStatusCode expectedStatus)
        {
            var response = await CreateCompany();
            return response.StatusCode == expectedStatus;
        }


        private async Task<HttpResponseMessage> CreateCase()
        {
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("caseKey", ""),
                new KeyValuePair<string, string>("title", "Test Case"),
                new KeyValuePair<string, string>("description", ""),
                new KeyValuePair<string, string>("openedAt", DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture)),
                new KeyValuePair<string, string>("assignedToId", ""),
                new KeyValuePair<string, string>("reportedById", ""),
                new KeyValuePair<string, string>("severity", ""),
                new KeyValuePair<string, string>("status", ""),
                new KeyValuePair<string, string>("devTeamAssignedId", "")
            });

            var model = ReflectionRepository.Global.Models.Single(m => m.Name == "Case");

            return await ApiPost(model, "Save", formData);
        }

        private async Task<HttpResponseMessage> CreatePerson()
        {
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("personId", ""),
                new KeyValuePair<string, string>("title", ""),
                new KeyValuePair<string, string>("firstName", "TestFirstName"),
                new KeyValuePair<string, string>("lastName", ""),
                new KeyValuePair<string, string>("email", ""),
                new KeyValuePair<string, string>("gender", ""),
                new KeyValuePair<string, string>("BirthDate", ""),
                new KeyValuePair<string, string>("LastBath", ""),
                new KeyValuePair<string, string>("NextUpgrade", ""),
                new KeyValuePair<string, string>("personStatsId", ""),
                new KeyValuePair<string, string>("timeZone", ""),
                new KeyValuePair<string, string>("companyId", "1")
            });

            var model = ReflectionRepository.Global.Models.Single(m => m.Name == "Person");

            return await ApiPost(model, "Save", formData);
        }

        private async Task<HttpResponseMessage> CreateProduct()
        {
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("productId", ""),
                new KeyValuePair<string, string>("name", "Test Product")
            });

            var model = ReflectionRepository.Global.Models.Single(m => m.Name == "Product");

            return await ApiPost(model, "Save", formData);
        }

        private async Task<HttpResponseMessage> CreateCompany()
        {
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("companyId", ""),
                new KeyValuePair<string, string>("name", "Test Company"),
                new KeyValuePair<string, string>("address1", ""),
                new KeyValuePair<string, string>("address2", ""),
                new KeyValuePair<string, string>("city", ""),
                new KeyValuePair<string, string>("state", ""),
                new KeyValuePair<string, string>("zipCode", "")
            });

            var model = ReflectionRepository.Global.Models.Single(m => m.Name == "Company");

            return await ApiPost(model, "Save", formData);
        }

        private async Task<HttpResponseMessage> ApiPost(ClassViewModel model, string action, FormUrlEncodedContent formData)
        {
            return await Client.PostAsync($"{model.ApiUrl}/{action}", formData);
        }

        private async Task<bool> AttemptToGetRestrictedPropertyValues(ClassViewModel model, PropertyViewModel property)
        {
            var apiPath = $"{model.ApiUrl}/propertyValues?property={property.Name}";
            var message = $"Attempting to access propertyValues for {property.Name} on model {model.Name}: ";
            var result = false;
            using (var response = await Client.GetAsync(apiPath))
            {
                result = response.StatusCode == HttpStatusCode.Unauthorized ||
                    response.StatusCode == HttpStatusCode.InternalServerError ||
                    response.StatusCode == HttpStatusCode.NotFound ||
                    response.StatusCode == HttpStatusCode.Found ||
                    (response.StatusCode == HttpStatusCode.OK && response.Content.Headers.ContentType != new MediaTypeHeaderValue("application/json"));
                message += result ? "passed" : "failed";
            }

            WriteMessage(message);
            return result;
        }

        private async Task<bool> AttemptToChangeRestrictedProperty(ClassViewModel model, PropertyViewModel property)
        {
            var message = $"Attempting to change {property.Name} on model {model.Name}: ";

            // See if we can get the list
            dynamic list = await GetEntityList(model);
            if (list == null) return true;

            // Get the first item and attempt to change the property
            var entity = list.list[0];
            string originalPropertyValue = ((IDictionary<string, object>)entity)[property.JsonName].ToString();
            ((IDictionary<string, object>)entity)[property.JsonName] = "Changed Value";

            string apiPath = $"{model.ApiUrl}/Save";
            var content = new StringContent(entity.ToString(), Encoding.UTF8, "application/json");
            await Client.PostAsync(apiPath, content);

            // Get the first item again and make sure it hasn't changed
            list = await GetEntityList(model);
            entity = list.list[0];
            var propMatchesOriginal = ((IDictionary<string, object>)entity)[property.JsonName].ToString() == originalPropertyValue;

            message += propMatchesOriginal ? "value not changed" : "value changed";
            WriteMessage(message);

            return propMatchesOriginal;
        }

        private async Task<dynamic> GetEntityList(ClassViewModel model, string fields = "", int page = 1, int pageSize = 10)
        {
            string apiPath = $"{model.ApiUrl}/List?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(fields)) apiPath += $"&fields={fields}";

            using (var response = await Client.GetAsync(apiPath))
            {
                if (!response.IsSuccessStatusCode) return null;

                using (var content = response.Content)
                {
                    if (content != null)
                    {
                        string contents = await content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<ExpandoObject>(contents);
                    }
                }
            }

            return null;
        }

        protected void WriteMessage(string message)
        {
            lock (OutputFile)
            {
                OutputFile.WriteLine($"{MessagePrefix}: {message}");
                OutputFile.Flush();
            }
        }
    }
}
