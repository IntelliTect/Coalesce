using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;
using IntelliTect.Coalesce.CodeGeneration.Api.BaseGenerators;
using IntelliTect.Coalesce.Utilities;
using System.Linq;
using IntelliTect.Coalesce.DataAnnotations;
using Microsoft.AspNetCore.WebUtilities;

namespace IntelliTect.Coalesce.CodeGeneration.Api.Generators
{
    public class ModelApiController : ApiController
    {
        public ModelApiController(GeneratorServices services) : base(services) { }

        public override void BuildOutput(CSharpCodeBuilder b)
        {
            var securityInfo = Model.SecurityInfo;
            string namespaceName = WriteNamespaces(b);

            b.Line();
            using (b.Block($"namespace {namespaceName}.Api"))
            {
                WriteControllerRouteAttribute(b);
                b.Line("[Authorize]");
                b.Line("[ServiceFilter(typeof(IApiActionFilter))]");

                b.Line($"public partial class {Model.ApiControllerClassName} ");
                if (Model.DbContext != null)
                {
                    b.Indented($": BaseApiController<{Model.BaseViewModel.FullyQualifiedName}, {Model.DtoName}, {Model.DbContext.Type.FullyQualifiedName}>");
                }
                else
                {
                    b.Indented($": BaseApiController<{Model.BaseViewModel.FullyQualifiedName}, {Model.DtoName}>");
                }


                // b.Block() has no contents here because we put the base class on a separate line to avoid really long lines.
                b.Line("{");
                using (b.Indented())
                {
                    WriteClassContents(b, securityInfo);
                }
                b.Line("}");
            }
        }

        private void WriteClassContents(CSharpCodeBuilder b, ClassSecurityInfo securityInfo)
        {
            var primaryKeyParameter = $"{Model.PrimaryKey.Type.FullyQualifiedName} id";
            var dataSourceParameter = $"IDataSource<{Model.BaseViewModel.FullyQualifiedName}> dataSource";
            var behaviorsParameter = $"IBehaviors<{Model.BaseViewModel.FullyQualifiedName}> behaviors";
            if (Model.IsDto)
            {
                var declaredForAttr = $"[DeclaredFor(typeof({Model.FullyQualifiedName}))] ";
                dataSourceParameter = declaredForAttr + dataSourceParameter;
                behaviorsParameter = declaredForAttr + behaviorsParameter;
            }

            using (b.Block(Model.DbContext != null 
                ? $"public {Model.ApiControllerClassName}(CrudContext<{Model.DbContext.Type.FullyQualifiedName}> context) : base(context)"
                : $"public {Model.ApiControllerClassName}(CrudContext context) : base(context)"
            ))
            {
                b.Line($"GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<{Model.FullyQualifiedName}>();");
            }

            if (securityInfo.IsReadAllowed())
            {
                // ENDPOINT: /get/{id}
                b.Line();
                b.Line("[HttpGet(\"get/{id}\")]");
                b.Line($"{securityInfo.Read.MvcAnnotation()}");
                b.Line($"{Model.ApiActionAccessModifier} virtual Task<ItemResult<{Model.DtoName}>> Get(");
                b.Indented($"{primaryKeyParameter},");
                b.Indented($"DataSourceParameters parameters,");
                b.Indented($"{dataSourceParameter})");
                b.Indented($"=> GetImplementation(id, parameters, dataSource);");

                // ENDPOINT: /list
                b.Line();
                b.Line("[HttpGet(\"list\")]");
                b.Line($"{securityInfo.Read.MvcAnnotation()}");
                b.Line($"{Model.ApiActionAccessModifier} virtual Task<ListResult<{Model.DtoName}>> List(");
                b.Indented($"ListParameters parameters,");
                b.Indented($"{dataSourceParameter})");
                b.Indented($"=> ListImplementation(parameters, dataSource);");

                // ENDPOINT: /count
                b.Line();
                b.Line("[HttpGet(\"count\")]");
                b.Line($"{securityInfo.Read.MvcAnnotation()}");
                b.Line($"{Model.ApiActionAccessModifier} virtual Task<ItemResult<int>> Count(");
                b.Indented($"FilterParameters parameters,");
                b.Indented($"{dataSourceParameter})");
                b.Indented($"=> CountImplementation(parameters, dataSource);");
            }

            if (securityInfo.Save.IsAllowed())
            {
                // ENDPOINT: /save
                b.Line();
                b.Line("[HttpPost(\"save\")]");
                b.Line($"{securityInfo.Save.MvcAnnotation()}");
                b.Line($"{Model.ApiActionAccessModifier} virtual Task<ItemResult<{Model.DtoName}>> Save(");
                b.Indented($"[FromForm] {Model.DtoName} dto,");
                b.Indented($"[FromQuery] DataSourceParameters parameters,");
                b.Indented($"{dataSourceParameter},");
                b.Indented($"{behaviorsParameter})");
                b.Indented($"=> SaveImplementation(dto, parameters, dataSource, behaviors);");
            }

            if (securityInfo.IsDeleteAllowed())
            {
                // ENDPOINT: /delete/{id}
                b.Line();
                b.Line("[HttpPost(\"delete/{id}\")]");
                b.Line($"{securityInfo.Delete.MvcAnnotation()}");
                b.Line($"{Model.ApiActionAccessModifier} virtual Task<ItemResult<{Model.DtoName}>> Delete(");
                b.Indented($"{primaryKeyParameter},");
                b.Indented($"{behaviorsParameter},");
                b.Indented($"{dataSourceParameter})");
                b.Indented($"=> DeleteImplementation(id, new DataSourceParameters(), dataSource, behaviors);");
            }

            if (Model.ClientMethods.Any())
            {
                b.Line();
                b.Line("// Methods from data class exposed through API Controller.");
            }
            foreach (var method in Model.ClientMethods)
            {
                WriteControllerActionPreamble(b, method);
                using (WriteControllerActionSignature(b, method))
                {
                    if (method.IsStatic)
                    {
                        WriteMethodInvocation(b, method, method.Parent.FullyQualifiedName);
                    }
                    else
                    {
                        b.Line($"var dataSource = dataSourceFactory.GetDataSource<" +
                            $"{Model.BaseViewModel.FullyQualifiedName}, {Model.FullyQualifiedName}>" +
                            $"(\"{method.LoadFromDataSourceName}\");");

                        if (Model.IsDto)
                        {
                            b.Line($"var itemResult = await dataSource.GetMappedItemAsync<{Model.FullyQualifiedName}>(id, new DataSourceParameters());");
                        }
                        else
                        {
                            b.Line("var (itemResult, _) = await dataSource.GetItemAsync(id, new DataSourceParameters());");
                        }
                        using (b.Block("if (!itemResult.WasSuccessful)"))
                        {
                            b.Line($"return new {method.ApiActionReturnTypeDeclaration}(itemResult);");
                        }
                        b.Line("var item = itemResult.Object;");

                        var varyByProperty = method.VaryByProperty;
                        if (varyByProperty != null)
                        {
                            WriteEtagProcessing(b, method, varyByProperty);
                        }

                        WriteMethodInvocation(b, method, "item");
                        if (Model.DbContext != null)
                        {
                            b.Line("await Db.SaveChangesAsync();");
                        }
                    }

                    WriteMethodResultProcessBlock(b, method);
                }
            }
        }

        private static void WriteEtagProcessing(CSharpCodeBuilder b, MethodViewModel method, PropertyViewModel varyByProperty)
        {
            b.Line();

            b.Line($"var _currentVaryValue = item.{varyByProperty.Name};");
            using (b.Block("if (_currentVaryValue != default)"))
            {
                string eTagString = varyByProperty.Type switch
                {
                    { IsByteArray: true } => "_currentVaryValue",
                    { IsString: true } => "System.Text.Encoding.UTF8.GetBytes(_currentVaryValue)",
                    _ => "System.Text.Encoding.UTF8.GetBytes(_currentVaryValue.ToString())",
                };

                // Always base64 the etag because otherwise its just too hard to prevent invalid characters leaking in.
                // Quotes, mismatches slashes (which mess up escape sequences), and ASCII control characters all make this hard
                eTagString = $"{typeof(Base64UrlTextEncoder).FullName}.Encode({eTagString})";

                // I'm so sorry about the escape sequence here.
                // We have to have code that replaces quotes within the value with slash-escaped quotes.
                // In order to write that code in C#, we have to escape the literal quotes and slashes.
                // And then we have to escape THOSE slashes AGAIN because we're writing a C# string that
                // will itself output a C# string.
                /* test cases for this (should write some unit tests at some point i guess?)
                 * "foo"
                 * "foo\""
                 * "foo\\\"" // this case fails if we don't also escape slashes. just escaping quotes isnt enough.
                 * "foo\\\\\""
                 */
                b.Line($"var _expectedEtagHeader = new Microsoft.Net.Http.Headers.EntityTagHeaderValue('\"' + {eTagString} + '\"');");

                if (varyByProperty.IsClientProperty)
                {
                    // Max age of zero forces a re-request always,
                    // but if the etag matches we can still return a 304.
                    b.Line("var _cacheControlHeader = new Microsoft.Net.Http.Headers.CacheControlHeaderValue { Private = true, MaxAge = TimeSpan.Zero };");

                    string varyValueComparison = varyByProperty.Type switch
                    {
                        // JS dates only have millisecond precision, so truncate the server value to milliseconds.
                        // Truncation is what happens on the JS side to anything more precise than a millisecond.
                        { IsDateTimeOffset: true } => 
                            "_currentVaryValue.AddTicks(-_currentVaryValue.Ticks % TimeSpan.TicksPerMillisecond) == etag",
                        { IsByteArray: true } => "_currentVaryValue.SequenceEqual(etag)",
                        _ => "_currentVaryValue == etag"
                    };

                    using (b.Block($"if (etag != default && {varyValueComparison})"))
                    {
                        // If the client is including the correct etag in the querystring,
                        // we can give them a long term cache duration
                        // so they can avoid having to make any request at all
                        // (with just etag alone, the client still has to check if
                        // the etag is still valid, but including the correct hash
                        // in the querystring this means the client has prior knowledge
                        // about the current version via the VaryByProperty's value).
                        b.Line("_cacheControlHeader.MaxAge = TimeSpan.FromDays(30);");
                    }

                    b.Line("Response.GetTypedHeaders().CacheControl = _cacheControlHeader;");
                }

                b.Line("Response.GetTypedHeaders().ETag = _expectedEtagHeader;");

                using (b.Block("if (Request.GetTypedHeaders().IfNoneMatch.Any(value => value.Compare(_expectedEtagHeader, true)))"))
                {
                    // The client already has the current response cached. Tell them to use it.
                    b.Line("return StatusCode(StatusCodes.Status304NotModified);");
                }
            }
            b.Line();
        }
    }
}
