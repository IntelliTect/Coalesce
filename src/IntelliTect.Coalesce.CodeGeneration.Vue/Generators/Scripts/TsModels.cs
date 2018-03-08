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
            var b = new TypeScriptCodeBuilder();
            b.Line("import * as metadata from './metadata.g'");
            b.Line("import { Model, convertToModel } from 'coalesce-vue/lib/model'");
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
            }

            foreach (var model in Model.ClientClasses)
            {
                using (b.Block($"export namespace {model.ViewModelClassName}"))
                {
                    b.Line($"/** Mutates the input object and its descendents into a valid {model.ViewModelClassName} implementation. */");
                    b.Line($"export function from(data?: Partial<{model.ViewModelClassName}>): {model.ViewModelClassName} {{ return convertToModel(data || {{}}, metadata.{model.ViewModelClassName}) }}");
                }

                using (b.Block($"export interface {model.ViewModelClassName} extends Model<typeof metadata.{model.ViewModelClassName}>"))
                {
                    foreach (var prop in model.ClientProperties)
                    {
                        var typeString = new VueType(prop.Type).TsType();
                        b.Line($"{prop.JsVariable}: {typeString} | null");
                    }
                }
                b.Line();
            }

            return Task.FromResult(b.ToString());
        }
    }
}
