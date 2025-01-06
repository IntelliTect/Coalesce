using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using IntelliTect.Coalesce.Utilities;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using IntelliTect.Coalesce.Api.Controllers;

namespace IntelliTect.Coalesce.CodeGeneration.Tests
{
    [ServiceFilter(typeof(IApiActionFilter))]
    public class TestController : Controller
    {
        [HttpGet]
        public object Test(IDataSource<Person> dataSource)
        {
            // Return the parameters back so they can be inspected in our test
            if (dataSource is ParameterTestsSource s) return new
            {
                s.PersonCriterion,
                s.PersonCriteriaArray,
                s.PersonCriteriaICollection,
                s.PersonCriteriaList,
                s.IntArray,
                s.IntList,
                s.IntICollection,
                s.Bytes
            };
            return new object();
        }
    }

    public class DataSourceModelBinderTests
    {
        [Fact]
        public async Task Binding_ExternalTypeParameter_DeserializesFromJson()
        {
            HttpClient client = GetClient();
            var res = await client.GetStringAsync("""/test/test?dataSource=ParameterTestsSource&dataSource.PersonCriterion={"name":"bob","Gender":1,"Date":"2024-01-03T08:20:00-07:00","PersonIds":[1,2,3,4,5],"subCriteria":[{"Name": "Grace"}]}""");

            Assert.Equal(
                """{"personCriterion":{"personIds":[1,2,3,4,5],"name":"bob","subCriteria":[{"name":"Grace","gender":0,"date":"0001-01-01T00:00:00+00:00"}],"gender":1,"date":"2024-01-03T08:20:00-07:00"}}""",
                res);
        }

        [Theory]
        [InlineData(nameof(ParameterTestsSource.PersonCriteriaArray))]
        [InlineData(nameof(ParameterTestsSource.PersonCriteriaICollection))]
        [InlineData(nameof(ParameterTestsSource.PersonCriteriaList))]
        public async Task Binding_ExternalTypeCollection_DeserializesFromJson(string collectionName)
        {
            HttpClient client = GetClient();
            var res = await client.GetStringAsync($$"""/test/test?dataSource=ParameterTestsSource&dataSource.{{collectionName}}=[{"name":"bob","Gender":1,"Date":"2024-01-03T08:20:00-07:00","PersonIds":[1,2,3,4,5],"subCriteria":[{"Name": "Grace"}]}]""");

            Assert.Equal(
                $$"""{"{{collectionName.ToCamelCase()}}":[{"personIds":[1,2,3,4,5],"name":"bob","subCriteria":[{"name":"Grace","gender":0,"date":"0001-01-01T00:00:00+00:00"}],"gender":1,"date":"2024-01-03T08:20:00-07:00"}]}""",
                res);
        }

        [Theory]
        [InlineData(nameof(ParameterTestsSource.IntArray))]
        [InlineData(nameof(ParameterTestsSource.IntICollection))]
        [InlineData(nameof(ParameterTestsSource.IntList))]
        public async Task Binding_PrimitiveCollection_DeserializesFromJson(string collectionName)
        {
            HttpClient client = GetClient();
            var res = await client.GetStringAsync($$"""/test/test?dataSource=ParameterTestsSource&dataSource.{{collectionName}}=[1,2,3,4]""");

            Assert.Equal(
                $$"""{"{{collectionName.ToCamelCase()}}":[1,2,3,4]}""",
                res);
        }

        [Theory]
        [InlineData(nameof(ParameterTestsSource.IntArray))]
        [InlineData(nameof(ParameterTestsSource.IntICollection))]
        [InlineData(nameof(ParameterTestsSource.IntList))]
        public async Task Binding_PrimitiveCollection_BindsConventionally(string collectionName)
        {
            HttpClient client = GetClient();
            var res = await client.GetStringAsync($$"""/test/test?dataSource=ParameterTestsSource&dataSource.{{collectionName}}=1&dataSource.{{collectionName}}=2&dataSource.{{collectionName}}=3""");

            Assert.Equal(
                $$"""{"{{collectionName.ToCamelCase()}}":[1,2,3]}""",
                res);
        }

        [Fact]
        public async Task Binding_Bytes_BindsFromBase64()
        {
            HttpClient client = GetClient();
            var res = await client.GetStringAsync($$"""/test/test?dataSource=ParameterTestsSource&dataSource.bytes=SGVsbG8gV29ybGQ%3D""");

            Assert.Equal(
                $$"""{"bytes":"SGVsbG8gV29ybGQ="}""",
                res);
        }

        [Fact]
        public async Task Validation_ExternalTypeParameter_ValidatesMembers()
        {
            HttpClient client = GetClient();
            var res = await client.GetAsync("""/test/test?dataSource=ParameterTestsSource&dataSource.PersonCriterion={"PersonIds":[1,2,3,4,5,6]}""");
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, res.StatusCode);
            Assert.Equal(
                """{"wasSuccessful":false,"message":"dataSource.PersonCriterion.PersonIds: The field PersonIds must be a string or collection type with a minimum length of '2' and maximum length of '5'."}""",
                await res.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Validation_TopLevelParameter_ValidatesDirectly()
        {
            HttpClient client = GetClient();
            var res = await client.GetAsync("""/test/test?dataSource=ParameterTestsSource&dataSource.IntArray=[1,2,3,4,5,6]""");
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, res.StatusCode);
            Assert.Equal(
                """{"wasSuccessful":false,"message":"dataSource.IntArray: The field IntArray must be a string or collection type with a minimum length of '2' and maximum length of '5'."}""",
                await res.Content.ReadAsStringAsync());
        }

        private static HttpClient GetClient() => GetClient<TestController>();

        private static HttpClient GetClient<T>()
        {
            // Ensure the code gen output assembly is built and loaded.
            // We need the generated DTOs for these tests.
            CodeGenTestBase.WebAssembly.Value.ToString();

            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();
            builder.Services.AddDbContext<AppDbContext>(db => db.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            builder.Services.AddCoalesce<AppDbContext>();

            builder.Services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                })
                .ConfigureApplicationPartManager(apm => apm.ApplicationParts.Add(new TypesPart(typeof(T))));

            var app = builder.Build();
            app.MapDefaultControllerRoute();
            app.Start();
            return app.GetTestClient();
        }

        private class TypesPart(params Type[] types) : ApplicationPart, IApplicationPartTypeProvider
        {
            public override string Name => string.Join(", ", Types.Select(t => t.FullName));

            public IEnumerable<TypeInfo> Types { get; } = types.Select(t => t.GetTypeInfo());
        }
    }
}
