using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;
using IntelliTect.Coalesce.CodeGeneration.Knockout.BaseGenerators;
using System.Threading.Tasks;
using IntelliTect.Coalesce.Utilities;
using System.Linq;
using IntelliTect.Coalesce.Knockout.TypeDefinition;
using IntelliTect.Coalesce.Api.DataSources;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.Generators
{
    public class KoListViewModel : KnockoutViewModelGenerator
    {
        public KoListViewModel(GeneratorServices services) : base(services) { }

        public override void BuildOutput(TypeScriptCodeBuilder b)
        {
            using (b.Block($"module {ListViewModelModuleName}"))
            {
                b.Line();
                WriteDataSources(b);

                b.Line();
                WriteListViewModelClass(b);
            }
        }

        private void WriteListViewModelClass(TypeScriptCodeBuilder b)
        {
            using (b.Block($"export class {Model.ListViewModelClassName} extends Coalesce.BaseListViewModel<{ViewModelFullName}>"))
            {
                b.Line($"public readonly modelName: string = \"{Model.ClientTypeName}\";");
                b.Line($"public readonly apiController: string = \"/{Model.ApiRouteControllerPart}\";");
                b.Line($"public modelKeyName: string = \"{Model.PrimaryKey.JsVariable}\";");
                b.Line($"public itemClass: new () => {ViewModelFullName} = {ViewModelFullName};");

                b.Line();
                b.Line("public filter: {");
                foreach (var prop in Model.BaseViewModel.ClientProperties.Where(f => f.IsUrlFilterParameter))
                {
                    b.Indented($"{prop.JsonName}?: string;");
                }
                b.Line("} | null = null;");

                b.DocComment("The namespace containing all possible values of this.dataSource.");
                b.Line($"public dataSources: typeof {Model.ClientTypeName}DataSources = {Model.ClientTypeName}DataSources;");

                b.DocComment("The data source on the server to use when retrieving objects. Valid values are in this.dataSources.");
                b.Line($"public dataSource: Coalesce.DataSource<{ViewModelFullName}> = new this.dataSources.{DataSourceFactory.DefaultSourceName}();");

                b.DocComment($"Configuration for all instances of {Model.ListViewModelClassName}. Can be overidden on each instance via instance.coalesceConfig.");
                b.Line($"public static coalesceConfig = new Coalesce.ListViewModelConfiguration<{Model.ListViewModelClassName}, {ViewModelFullName}>(Coalesce.GlobalConfiguration.listViewModel);");

                b.DocComment($"Configuration for this {Model.ListViewModelClassName} instance.");
                b.Line($"public coalesceConfig: Coalesce.ListViewModelConfiguration<{Model.ListViewModelClassName}, {ViewModelFullName}>");
                b.Indented($"= new Coalesce.ListViewModelConfiguration<{Model.ListViewModelClassName}, {ViewModelFullName}>({Model.ListViewModelClassName}.coalesceConfig);");

                // Write client methods
                b.Line();
                foreach (var method in Model.ClientMethods.Where(m => m.IsStatic))
                {
                    b.Line();
                    WriteClientMethodDeclaration(b, method, Model.ListViewModelClassName);
                }

                b.Line();
                b.Line($"protected createItem = (newItem?: any, parent?: any) => new {ViewModelFullName}(newItem, parent);");

                b.Line();
                b.Line("constructor() {");
                b.Indented("super();");
                b.Line("}");
            }
        }

        private void WriteDataSources(TypeScriptCodeBuilder b)
        {
            var dataSources = Model.ClientDataSources(Services.ReflectionRepository).ToList();
            var defaultSource = dataSources.SingleOrDefault(s => s.IsDefaultDataSource);

            using (b.Block($"export namespace {Model.ClientTypeName}DataSources"))
            {
                if (defaultSource == null)
                {
                    b.Line($"export class {DataSourceFactory.DefaultSourceName} extends Coalesce.DataSource<{ViewModelFullName}> {{ }}");
                }

                foreach (var source in dataSources)
                {
                    b.DocComment(source.Comment);
                    using (b.Block($"export class {source.ClientTypeName} extends Coalesce.DataSource<{ViewModelFullName}>"))
                    {
                        if (source.DataSourceParameters.Any())
                        {
                            foreach (PropertyViewModel prop in source.DataSourceParameters)
                            {
                                b.DocComment(prop.Comment);
                                b.Line($"public {prop.JsVariable}: {prop.Type.TsKnockoutType(true)} = {prop.Type.ObservableConstructorCall()};");
                            }
                            using (b.Block("public saveToDto = () =>"))
                            {
                                b.Line("var dto: any = {};");
                                foreach (PropertyViewModel prop in source.DataSourceParameters)
                                {
                                    if (prop.Type.IsDate)
                                    {
                                        b.Line($"if (!this.{prop.JsVariable}()) dto.{prop.JsonName} = null;");
                                        b.Line($"else dto.{prop.JsonName} = this.{prop.JsVariable}()!.format('YYYY-MM-DDTHH:mm:ss{(prop.Type.IsDateTimeOffset ? "ZZ" : "")}');");
                                    }
                                    else
                                    {
                                        b.Line($"dto.{prop.JsonName} = this.{prop.JsVariable}();");
                                    }
                                }
                                b.Line("return dto;");
                            }
                        }
                    }

                    // Case-sensitive comparison intended here. We always need a data source cased EXACTLY as "Default".
                    if (source == defaultSource && !source.ClientTypeName.Equals(DataSourceFactory.DefaultSourceName, StringComparison.InvariantCulture))
                    {
                        b.Line($"export const {DataSourceFactory.DefaultSourceName} = {source.ClientTypeName};");
                    }
                }
            }
        }
    }
}
