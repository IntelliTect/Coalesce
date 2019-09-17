using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;
using IntelliTect.Coalesce.CodeGeneration.Api.BaseGenerators;
using IntelliTect.Coalesce.Utilities;
using System.Linq;
using IntelliTect.Coalesce.DataAnnotations;

namespace IntelliTect.Coalesce.CodeGeneration.Api.Generators
{
    public class ModelApiController : ApiController
    {
        public ModelApiController(GeneratorServices services) : base(services) { }

        public ClassViewModel DbContext { get; set; }

        public ModelApiController WithDbContext(ClassViewModel contextViewModel)
        {
            DbContext = contextViewModel;
            return this;
        }

        public override void BuildOutput(CSharpCodeBuilder b)
        {
            var securityInfo = Model.SecurityInfo;
            string namespaceName = WriteNamespaces(b);

            b.Line();
            using (b.Block($"namespace {namespaceName}.Api"))
            {
                WriteControllerRouteAttribute(b);
                b.Line($"{securityInfo.ClassAnnotation}");
                b.Line("[ServiceFilter(typeof(IApiActionFilter))]");

                b.Line($"public partial class {Model.ApiControllerClassName} ");
                b.Line($"    : BaseApiController<{Model.BaseViewModel.FullyQualifiedName}, {Model.DtoName}, {DbContext.Type.FullyQualifiedName}>");


                // b.Block() has no contents here because we put the base class on a separate line to avoid really long lines.
                b.Line("{");
                using (b.Indented())
                {
                    WriteClassContents(b, securityInfo);
                }
                b.Line("}");
            }
        }

        private void WriteClassContents(CSharpCodeBuilder b, DataAnnotations.ClassSecurityInfo securityInfo)
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

            using (b.Block($"public {Model.ApiControllerClassName}({DbContext.Type.FullyQualifiedName} db) : base(db)"))
            {
                b.Line($"GeneratedForClassViewModel = ReflectionRepository.Global.GetClassViewModel<{Model.FullyQualifiedName}>();");
            }

            if (securityInfo.IsReadAllowed())
            {
                // ENDPOINT: /get/{id}
                b.Line();
                b.Line("[HttpGet(\"get/{id}\")]");
                b.Line($"{securityInfo.ReadAnnotation}");
                b.Line($"{Model.ApiActionAccessModifier} virtual Task<ItemResult<{Model.DtoName}>> Get(");
                b.Indented($"{primaryKeyParameter},");
                b.Indented($"DataSourceParameters parameters,");
                b.Indented($"{dataSourceParameter})");
                b.Indented($"=> GetImplementation(id, parameters, dataSource);");

                // ENDPOINT: /list
                b.Line();
                b.Line("[HttpGet(\"list\")]");
                b.Line($"{securityInfo.ReadAnnotation}");
                b.Line($"{Model.ApiActionAccessModifier} virtual Task<ListResult<{Model.DtoName}>> List(");
                b.Indented($"ListParameters parameters,");
                b.Indented($"{dataSourceParameter})");
                b.Indented($"=> ListImplementation(parameters, dataSource);");

                // ENDPOINT: /count
                b.Line();
                b.Line("[HttpGet(\"count\")]");
                b.Line($"{securityInfo.ReadAnnotation}");
                b.Line($"{Model.ApiActionAccessModifier} virtual Task<ItemResult<int>> Count(");
                b.Indented($"FilterParameters parameters,");
                b.Indented($"{dataSourceParameter})");
                b.Indented($"=> CountImplementation(parameters, dataSource);");
            }

            if (securityInfo.IsCreateAllowed() || securityInfo.IsEditAllowed())
            {
                // ENDPOINT: /save
                b.Line();
                b.Line("[HttpPost(\"save\")]");
                b.Line($"{securityInfo.SaveAnnotation}");
                b.Line($"{Model.ApiActionAccessModifier} virtual Task<ItemResult<{Model.DtoName}>> Save(");
                b.Indented($"{Model.DtoName} dto,");
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
                b.Line($"{securityInfo.DeleteAnnotation}");
                b.Line($"{Model.ApiActionAccessModifier} virtual Task<ItemResult<{Model.DtoName}>> Delete(");
                b.Indented($"{primaryKeyParameter},");
                b.Indented($"{behaviorsParameter},");
                b.Indented($"{dataSourceParameter})");
                b.Indented($"=> DeleteImplementation(id, new DataSourceParameters(), dataSource, behaviors);");
            }

            // ENDPOINT: /csvDownload
            b.DocComment($"Downloads CSV of {Model.DtoName}");
            b.Line("[HttpGet(\"csvDownload\")]");
            b.Line($"{securityInfo.ReadAnnotation}");
            b.Line($"{Model.ApiActionAccessModifier} virtual Task<FileResult> CsvDownload(");
            b.Indented($"ListParameters parameters,");
            b.Indented($"{dataSourceParameter})");
            b.Indented($"=> CsvDownloadImplementation(parameters, dataSource);");

            // ENDPOINT: /csvText
            b.DocComment($"Returns CSV text of {Model.DtoName}");
            b.Line("[HttpGet(\"csvText\")]");
            b.Line($"{securityInfo.ReadAnnotation}");
            b.Line($"{Model.ApiActionAccessModifier} virtual Task<string> CsvText(");
            b.Indented($"ListParameters parameters,");
            b.Indented($"{dataSourceParameter})");
            b.Indented($"=> CsvTextImplementation(parameters, dataSource);");

            if (securityInfo.IsCreateAllowed() || securityInfo.IsEditAllowed())
            {
                // ENDPOINT: /csvUpload
                b.DocComment($"Saves CSV data as an uploaded file");
                b.Line("[HttpPost(\"csvUpload\")]");
                b.Line($"{securityInfo.SaveAnnotation}");
                b.Line($"{Model.ApiActionAccessModifier} virtual Task<IEnumerable<ItemResult>> CsvUpload(");
                b.Indented($"IFormFile file,");
                b.Indented($"{dataSourceParameter},");
                b.Indented($"{behaviorsParameter},");
                b.Indented($"bool hasHeader = true) ");
                b.Indented($"=> CsvUploadImplementation(file, dataSource, behaviors, hasHeader);");

                // ENDPOINT: /csvSave
                b.DocComment($"Saves CSV data as a posted string");
                b.Line("[HttpPost(\"csvSave\")]");
                b.Line($"{securityInfo.SaveAnnotation}");
                b.Line($"{Model.ApiActionAccessModifier} virtual Task<IEnumerable<ItemResult>> CsvSave(");
                b.Indented($"string csv,");
                b.Indented($"{dataSourceParameter},");
                b.Indented($"{behaviorsParameter},");
                b.Indented($"bool hasHeader = true) ");
                b.Indented($"=> CsvSaveImplementation(csv, dataSource, behaviors, hasHeader);");
            }

            b.Line();
            b.Line("// Methods from data class exposed through API Controller.");
            foreach (var method in Model.ClientMethods)
            {
                var returnType = method.ApiActionReturnTypeDeclaration;
                if (!method.IsStatic || method.IsAwaitable)
                {
                    returnType = $"async Task<{returnType}>";
                }

                b.DocComment($"Method: {method.Name}");
                b.Line($"[{method.ApiActionHttpMethodAnnotation}(\"{method.Name}\")]");
                b.Line($"{method.SecurityInfo.ExecuteAnnotation}");
                using (b.Block($"{Model.ApiActionAccessModifier} virtual {returnType} {method.Name} ({method.CsParameters})"))
                {
                    if (method.ResultType.HasClassViewModel ||
                       (method.ResultType.PureType.HasClassViewModel && method.ResultType.IsCollection))
                    {
                        b.Line("IncludeTree includeTree = null;");
                    }

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
                            b.Line($"var itemResult = await dataSource.GetMappedItemAsync<{Model.FullyQualifiedName}>(id, new ListParameters());");
                        }
                        else
                        {
                            b.Line("var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());");
                        }
                        using (b.Block("if (!itemResult.WasSuccessful)"))
                        {
                            b.Line($"return new {method.ApiActionReturnTypeDeclaration}(itemResult);");
                        }
                        b.Line("var item = itemResult.Object;");
                        WriteMethodInvocation(b, method, "item");
                        b.Line("await Db.SaveChangesAsync();");
                    }

                    WriteMethodResultProcessBlock(b, method);
                }
            }


            foreach (var fileProperty in Model.FileProperties)
            {
                var returnType = $"Task<ItemResult<{Model.DtoName}>>";

                if (fileProperty.SecurityInfo.IsRead)
                {
                    b.DocComment($"File Download: {fileProperty.Name}");
                    b.Line($"{ReadAnnotation(fileProperty.SecurityInfo)}");
                    b.Line($"[HttpGet(\"{fileProperty.FileControllerMethodName}\")]");
                    using (b.Block($"{Model.ApiActionAccessModifier} virtual async Task<IActionResult> {fileProperty.FileControllerMethodName}Get ({primaryKeyParameter}, {dataSourceParameter})"))
                    {
                        b.Line($"var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());");
                        b.Line($"if (itemResult.Object?.{fileProperty.Name} == null) return NotFound();");
                        b.Line($"string contentType = \"{fileProperty.FileMimeType}\";");

                        if (fileProperty.FileNameProperty == null)
                        {
                            b.Line($"return File(itemResult.Object.{fileProperty.Name}, contentType);");
                        }
                        else
                        {
                            using (b.Block($"if (!(new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider().TryGetContentType(itemResult.Object.{fileProperty.FileNameProperty.Name}, out contentType)))"))
                            {
                                b.Line($"contentType = \"{fileProperty.FileMimeType}\";");
                            }

                            b.Line($"return File(itemResult.Object.{fileProperty.Name}, contentType, itemResult.Object.{fileProperty.FileNameProperty.Name});");
                        }
                    }
                }

                if (securityInfo.IsCreateAllowed() || securityInfo.IsEditAllowed())
                {
                    b.DocComment($"File Upload: {fileProperty.Name}");
                    b.Line($"{EditAnnotation(fileProperty.SecurityInfo)}");
                    b.Line($"[HttpPut(\"{fileProperty.FileControllerMethodName}\")]");
                    using (b.Block($"{Model.ApiActionAccessModifier} virtual async {returnType} {fileProperty.FileControllerMethodName}Put ({primaryKeyParameter}, IFormFile file, {dataSourceParameter}, IBehaviors<{Model.FullyQualifiedName}> behaviors)"))
                    {
                        b.Line("var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());");
                        b.Line($"if (!itemResult.WasSuccessful) return new ItemResult<{Model.DtoName}>(itemResult);");
                        using (b.Block("using (var stream = new System.IO.MemoryStream())"))
                        {
                            b.Line("await file.CopyToAsync(stream);");

                            b.Line($"itemResult.Object.{fileProperty.Name} = stream.ToArray();");

                            if (fileProperty.FileNameProperty?.HasPublicSetter ?? false)
                            {
                                b.Line($"itemResult.Object.{fileProperty.FileNameProperty.Name} = file.FileName;");
                            }

                            if (fileProperty.FileHashProperty?.HasPublicSetter ?? false)
                            {
                                using (b.Block("using (var sha256Hash = System.Security.Cryptography.SHA256.Create())"))
                                {
                                    b.Line($"var hash = sha256Hash.ComputeHash(itemResult.Object.{fileProperty.Name});");
                                    b.Line($"itemResult.Object.{fileProperty.FileHashProperty.Name} = Convert.ToBase64String(hash);");
                                }
                            }

                            if (fileProperty.FileSizeProperty?.HasPublicSetter ?? false)
                            {
                                b.Line($"itemResult.Object.{fileProperty.FileSizeProperty.Name} = file.Length;");
                            }

                            b.Line("await Db.SaveChangesAsync();");
                        }
                        b.Line($"var result = new ItemResult<{Model.DtoName}>();");
                        b.Line("var mappingContext = new MappingContext(User, \"\");");
                        b.Line($"result.Object = Mapper.MapToDto<{Model.BaseViewModel.FullyQualifiedName}, {Model.DtoName}>(itemResult.Object, mappingContext, null);");
                        b.Line("return result;");
                    }

                    b.DocComment($"File Delete: {fileProperty.Name}");
                    b.Line($"{securityInfo.DeleteAnnotation}");
                    b.Line($"[HttpDelete (\"{fileProperty.FileControllerMethodName}\")]");
                    using (b.Block($"{Model.ApiActionAccessModifier} virtual async Task<IActionResult> {fileProperty.FileControllerMethodName}Delete ({primaryKeyParameter}, {dataSourceParameter}, IBehaviors<{Model.FullyQualifiedName}> behaviors)"))
                    {
                        b.Line($"var (itemResult, _) = await dataSource.GetItemAsync(id, new ListParameters());");
                        b.Line($"if (!itemResult.WasSuccessful) return NotFound();");

                        if (fileProperty.FileNameProperty?.HasPublicSetter ?? false)
                        {
                            b.Line($"itemResult.Object.{fileProperty.FileNameProperty.Name} = null;");
                        }
                        if (fileProperty.FileHashProperty?.HasPublicSetter ?? false)
                        {
                            b.Line($"itemResult.Object.{fileProperty.FileHashProperty.Name} = null;");
                        }
                        if (fileProperty.FileSizeProperty?.HasPublicSetter ?? false)
                        {
                            b.Line($"itemResult.Object.{fileProperty.FileSizeProperty.Name} = 0;");
                        }
                        b.Line($"itemResult.Object.{fileProperty.Name} = null;");

                        b.Line("await Db.SaveChangesAsync();");
                        b.Line("return Ok();");
                    }
                }
            }
        }



        private bool AllowAnonymousAll(PropertySecurityInfo securityInfo)
        {
            return securityInfo.Read.AllowAnonymous &&
                securityInfo.Edit.AllowAnonymous &&
                securityInfo.Delete.AllowAnonymous;
        }

        /// <summary>
        /// Returns an annotation for reading things (List/Get)
        /// </summary>
        private string ReadAnnotation(PropertySecurityInfo securityInfo)
        {
            if (securityInfo.Read.NoAccess) throw NoAccessException();
            if (AllowAnonymousAll(securityInfo)) return string.Empty;
            if (securityInfo.Read.AllowAnonymous || securityInfo.Edit.AllowAnonymous) return "[AllowAnonymous]";
            if (securityInfo.Read.HasRoles) return $"[Authorize(Roles=\"{AllRoles(securityInfo)}\")]";

            return "[Authorize]";
        }

        /// <summary>
        /// Returns an annotation for editing things
        /// </summary>
        private string EditAnnotation(PropertySecurityInfo securityInfo)
        {
            
            if (securityInfo.Edit.NoAccess) throw NoAccessException();
            if (AllowAnonymousAll(securityInfo)) return string.Empty;
            if (securityInfo.Edit.AllowAnonymous) return "[AllowAnonymous]";
            if (securityInfo.Edit.HasRoles) return $"[Authorize(Roles=\"{securityInfo.Edit.ExternalRoleList}\")]";

            return "[Authorize]";
        }

        private string AllRoles(PropertySecurityInfo securityInfo)
        {

            var result = securityInfo.Read.RoleList
                .Union(securityInfo.Delete.RoleList)
                .ToList();

            if (result.Count() == 0) return "";
            return string.Join(",", result);
        }

        /// <summary>
        /// This is checked, and this exception thrown, to prevent accidents in code generation.
        /// </summary>
        /// <returns></returns>
        private Exception NoAccessException()
            => new InvalidOperationException(
                $"Cannot emit an annotation for permission level {SecurityPermissionLevels.DenyAll}. Templates shouldn't emit anything in such cases.");
    }
}
