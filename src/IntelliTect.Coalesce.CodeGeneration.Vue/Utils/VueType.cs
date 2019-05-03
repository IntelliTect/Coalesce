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

        public string TsType(string modelPrefix = null, bool viewModel = false)
        {
            // TODO: expand this pattern to the knockout generation,
            // and make a base class that for the common bits (strings are always strings, for example).

            // This class (VueType) is a start towards that, but it needs more work for sure.

            modelPrefix = modelPrefix != null ? modelPrefix + "." : "";

            return TsType(type, modelPrefix, viewModel);
        }

        private string TsType(TypeViewModel type, string modelPrefix, bool viewModel)
        {
            if (type.IsByteArray) return "string";
            if (type.IsCollection && type.IsNumber) return "number[]";
            if (type.IsCollection) return TsTypePlain(type.PureType, modelPrefix, viewModel) + "[]";
            if (type.IsGuid) return "string";
            return TsTypePlain(type, modelPrefix, viewModel);
        }

        private string TsTypePlain(TypeViewModel type, string modelPrefix, bool viewModel)
        {
            if (type.IsString) return "string";
            if (type.IsBool) return "boolean";
            if (type.IsDate) return "Date";
            if (type.IsEnum) return modelPrefix + this.type.NullableUnderlyingType.ClientTypeName;
            if (type.IsNumber) return "number";
            if (type.IsVoid) return "void";
            if (type.IsPOCO)
            {
                string viewModelAppend = "";
                if (viewModel && type.ClassViewModel.IsDbMappedType)
                {
                    modelPrefix = "";
                    viewModelAppend = "ViewModel";
                }
                return $"{modelPrefix}{type.PureType.Name}{viewModelAppend}";
            }
            if (type.IsClass) return type.PureType.Name;
            return "any";
        }
    }
}
