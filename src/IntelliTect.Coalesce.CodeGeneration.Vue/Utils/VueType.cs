using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Vue.Utils
{
    public class VueType
    {
        private readonly TypeViewModel type;

        public VueType(TypeViewModel type)
        {
            this.type = type;
        }

        public string TsType(string modelPrefix = null)
        {
            // TODO: this .Replace() to get rid of "ViewModels." is a hack. 
            // So is the enum handling, and the moment replacement
            // We need to create some sort of resolver class for resolving C# types to the names we should use in generated typescript.

            // This class (VueType) was a start towards that, but it needs more work for sure.


            modelPrefix = modelPrefix != null ? modelPrefix + "." : "";

            if (this.type.IsEnum)
            {
                return modelPrefix + this.type.NullableUnderlyingType.ClientTypeName;
            }

            return this.type.TsType
                .Replace("ViewModels.", modelPrefix)
                .Replace("moment.Moment", "Date");

        }
    }
}
