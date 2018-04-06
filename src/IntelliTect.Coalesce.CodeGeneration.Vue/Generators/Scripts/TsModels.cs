using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.CodeGeneration.Vue.Utils;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Vue.Generators
{
    public class TsModels : StringBuilderFileGenerator<ReflectionRepository>
    {
        public TsModels(GeneratorServices services) : base(services)
        {
        }

        public override Task<string> BuildOutputAsync()
        {
            var b = new TypeScriptCodeBuilder(indentSize: 2);
            b.Line("import * as metadata from './metadata.g'");
            b.Line("import { Model, DataSource, convertToModel } from 'coalesce-vue/lib/model'");
         //   b.Line("import { Domain, getEnumMeta, ModelType, ExternalType } from './coalesce/core/metadata' ");
            b.Line();

            foreach (var model in Model.ClientEnums)
            {
                using (b.Block($"export enum {model.Name}"))
                {
                    foreach (var value in model.EnumValues)
                    {
                        b.Line($"{value.Value} = {value.Key},");
                    }
                }

                b.Line();
                b.Line();
            }

            foreach (var model in Model.ClientClasses)
            {
                using (b.Block($"export namespace {model.ViewModelClassName}"))
                {
                    b.DocComment($"Mutates the input object and its descendents into a valid {model.ViewModelClassName} implementation.");
                    b.Line($"export function from(data?: Partial<{model.ViewModelClassName}>): {model.ViewModelClassName} {{ return convertToModel(data || {{}}, metadata.{model.ViewModelClassName}) }}");

                    b.Line();
                    using (b.Block("export namespace DataSources"))
                    {
                        foreach (var source in model.ClientDataSources(Model))
                        {
                            b.DocComment(source.Comment, true);
                            var sourceMeta = $"metadata.{model.ViewModelClassName}.dataSources.{source.ClientTypeName.ToCamelCase()}";

                            using (b.Block($"export interface {source.ClientTypeName} extends DataSource<typeof {sourceMeta}>"))
                            {
                                foreach (var param in source.DataSourceParameters)
                                {
                                    b.DocComment(param.Comment);
                                    var typeString = new VueType(param.Type).TsType();
                                    b.Line($"{param.JsVariable}: {typeString} | null");
                                }
                            }
                            using (b.Block($"export namespace {source.ClientTypeName}"))
                            {
                                b.DocComment($"Mutates the input object and its descendents into a valid {source.ViewModelClassName} implementation.");
                                b.Line($"export function from(data?: Partial<{source.ViewModelClassName}>): {source.ViewModelClassName} {{ return convertToModel(data || {{}}, {sourceMeta}) }}");
                            }
                        }
                    }
                }

                using (b.Block($"export interface {model.ViewModelClassName} extends Model<typeof metadata.{model.ViewModelClassName}>"))
                {
                    foreach (var prop in model.ClientProperties)
                    {
                        b.DocComment(prop.Comment);
                        var typeString = new VueType(prop.Type).TsType();
                        b.Line($"{prop.JsVariable}: {typeString} | null");
                    }
                }

                b.Line();
                b.Line();
            }

            return Task.FromResult(b.ToString());
        }
    }
}
