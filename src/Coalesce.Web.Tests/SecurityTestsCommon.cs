using Coalesce.Web.Tests.Helpers;
using IntelliTect.Coalesce.TypeDefinition;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
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

            results.Add(await RestrictedReadPropertiesViaPropertyValues());
            results.Add(await RestrictedEditProperties());

            return results.All(b => b);
        }

        public async Task<bool> RestrictedReadPropertiesViaPropertyValues()
        {
            // Get a list of properties on anonmymous models that have read restrictions
            var models = Model.Models
                            .Where(m => m.SecurityInfo.AllowAnonymousEdit || m.SecurityInfo.AllowAnonymousRead)
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
                            .Where(m => m.SecurityInfo.AllowAnonymousEdit)
                            .SelectMany(m => m.Properties
                                                .Where(p => !p.SecurityInfo.IsEditable(User) && p.SecurityInfo.IsReadable(User))
                                                .Select(p => new { Model = m, Property = p }))
                            .ToList();

            var tasks = models
                        .Select(async m => await AttemptToChangeRestrictedProperty(m.Model, m.Property));
            var results = await Task.WhenAll(tasks);
            return results.All(b => b);
        }

        //public async Task<bool> RestrictedEditProperties()
        //{
        //    // Get a list of properties on anonmymous models that have edit restrictions
        //    var models = Model.Models
        //                    .Where(m => m.SecurityInfo.AllowAnonymousEdit)
        //                    .SelectMany(m => m.Properties
        //                                        .Where(p => !p.SecurityInfo.IsEditable(User) && p.SecurityInfo.IsReadable(User))
        //                                        .Select(p => new { Model = m, Property = p }))
        //                    .ToList();

        //    var tasks = models
        //                .Select(async m => await AttemptToChangeRestrictedProperty(m.Model, m.Property));
        //    var results = await Task.WhenAll(tasks);
        //    return results.All(b => b);
        //}


        private async Task<bool> AttemptToGetRestrictedPropertyValues(ClassViewModel model, PropertyViewModel property)
        {
            var apiPath = $"{model.ApiUrl}/propertyValues?property={property.Name}";
            var message = $"Attempting to access propertyValues for {property.Name} on model {model.Name}: ";
            var result = false;
            using (var response = await Client.GetAsync(apiPath))
            {
                result = response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.InternalServerError;
                message += result ? "passed" : "failed";
            }

            WriteMessage(message);
            return result;
        }

        private async Task<bool> AttemptToChangeRestrictedProperty(ClassViewModel model, PropertyViewModel property)
        {
            var message = $"Attempting to change {property.Name} on model {model.Name}: ";

            // Get the first item and attempt to change the property
            dynamic list = await GetEntityList(model);
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

            message += propMatchesOriginal ? "passed" : "failed";
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

        private void WriteMessage(string message)
        {
            lock (OutputFile)
            {
                OutputFile.WriteLine($"{MessagePrefix}: {message}");
                OutputFile.Flush();
            }
        }
    }
}
