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
        private readonly Flags flags;

        public VueType(TypeViewModel type, Flags flags = Flags.None)
        {
            this.type = type;
            this.flags = flags;
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
            if (type.IsByteArray) return flags.HasFlag(Flags.RawBinary) ? "string | Uint8Array" : "string";
            if (type.IsCollection) return TsTypePlain(type.PureType, modelPrefix, viewModel) + "[]";
            return TsTypePlain(type, modelPrefix, viewModel);
        }

        private string TsTypePlain(TypeViewModel type, string modelPrefix, bool viewModel)
        {
            switch (type.TsTypeKind)
            {
                case TypeDiscriminator.String: return "string";
                case TypeDiscriminator.Boolean: return "boolean";
                case TypeDiscriminator.Date: return "Date";
                case TypeDiscriminator.Enum: return modelPrefix + type.NullableValueUnderlyingType.ClientTypeName;
                case TypeDiscriminator.Number: return "number";
                case TypeDiscriminator.Void: return "void";
                case TypeDiscriminator.Unknown: return "unknown";
            }

            if (type.IsFile)
            {
                return flags.HasFlag(Flags.RawBinary)
                    ? "File"
                    : throw new InvalidOperationException("File not supported in context that doesn't allow raw binary");
            }
            if (type.IsByteArray)
            {
                throw new InvalidOperationException("Collections of byte[] are not supported.");
            }
            if (type.IsPOCO)
            {
                string viewModelAppend = "";
                if (viewModel && (type.ClassViewModel.IsDbMappedType || type.ClassViewModel.IsStandaloneEntity))
                {
                    modelPrefix = "";
                    viewModelAppend = "ViewModel";
                }
                return modelPrefix + type.PureType.Name + viewModelAppend;
            }
            if (type.IsClass) return type.PureType.Name;
            return "unknown";
        }

        [Flags]
        public enum Flags
        {
            None = 0,
            RawBinary = 1 << 0,
        }
    }
}
