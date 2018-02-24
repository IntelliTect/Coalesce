using IntelliTect.Coalesce.CodeGeneration.Generation;
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
            b.Line("import { Model } from './coalesce/core/model'");
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
                using (b.Block($"export interface {model.ViewModelClassName} extends Model<typeof metadata.{model.ViewModelClassName}>"))
                {
                    foreach (var prop in model.ClientProperties)
                    {
                        // TODO: this .Replace() to get rid of "ViewModels." is a hack. 
                        // So is the moment .Replace().
                        // So is the enum handling.
                        // We need to create some sort of resolver class for resolving C# types to the names we should use in generated typescript.
                        string type = prop.Type.IsEnum 
                            ? prop.Type.Name 
                            : prop.Type.TsType
                                .Replace("ViewModels.", "")
                                .Replace("moment.Moment", "Date");

                        b.Line($"{prop.JsVariable}: {type} | null");
                    }
                }
                b.Line();
            }

            return Task.FromResult(b.ToString());
        }
    }
}
