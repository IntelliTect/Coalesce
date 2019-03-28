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
            b.Line("import { Model, DataSource, convertToModel, mapToModel } from 'coalesce-vue/lib/model'");
         //   b.Line("import { Domain, getEnumMeta, ModelType, ExternalType } from './coalesce/core/metadata' ");
            b.Line();

            foreach (var model in Model.ClientEnums.OrderBy(e => e.ClientTypeName))
            {
                using (b.Block($"export enum {model.ClientTypeName}"))
                {
                    foreach (var value in model.EnumValues)
                    {
                        b.Line($"{value.Value} = {value.Key},");
                    }
                }

                b.Line();
                b.Line();
            }

            foreach (var model in Model.ClientClasses
                .OrderByDescending(c => c.IsDbMappedType) // Entity types first
                .ThenBy(e => e.ClientTypeName))
            {
                var name = model.ViewModelClassName;

                using (b.Block($"export interface {name} extends Model<typeof metadata.{name}>"))
                {
                    foreach (var prop in model.ClientProperties)
                    {
                        b.DocComment(prop.Comment);
                        var typeString = new VueType(prop.Type.NullableUnderlyingType).TsType();
                        b.Line($"{prop.JsVariable}: {typeString} | null");
                    }
                }

                using (b.Block($"export class {name}"))
                {
                    b.DocComment($"Mutates the input object and its descendents into a valid {name} implementation.");
                    using (b.Block($"static convert(data?: Partial<{name}>): {name}"))
                    {
                        b.Line($"return convertToModel(data || {{}}, metadata.{name}) ");
                    }

                    b.DocComment($"Maps the input object and its descendents to a new, valid {name} implementation.");
                    using (b.Block($"static map(data?: Partial<{name}>): {name}"))
                    {
                        b.Line($"return mapToModel(data || {{}}, metadata.{name}) ");
                    }

                    b.DocComment($"Instantiate a new {name}, optionally basing it on the given data.");
                    using (b.Block($"constructor(data?: Partial<{name}> | {{[k: string]: any}})"))
                    {
                        b.Indented($"Object.assign(this, {name}.map(data || {{}}));");
                    }
                }

                var dataSources = model.ClientDataSources(Model);
                if (model.IsDbMappedType && dataSources.Any())
                {
                    using (b.Block($"export namespace {name}"))
                    {
                        using (b.Block("export namespace DataSources"))
                        {
                            foreach (var source in dataSources)
                            {
                                b.DocComment(source.Comment, true);
                                var sourceMeta = $"metadata.{name}.dataSources.{source.ClientTypeName.ToCamelCase()}";

                                using (b.Block($"export class {source.ClientTypeName} implements DataSource<typeof {sourceMeta}>"))
                                {
                                    b.Line($"readonly $metadata = {sourceMeta}");
                                    foreach (var param in source.DataSourceParameters)
                                    {
                                        b.DocComment(param.Comment);
                                        var typeString = new VueType(param.Type).TsType();
                                        b.Line($"{param.JsVariable}: {typeString} | null = null");
                                    }
                                }
                            }
                        }
                    }
                }

                b.Line();
                b.Line();
            }

            return Task.FromResult(b.ToString());
        }
    }
}
