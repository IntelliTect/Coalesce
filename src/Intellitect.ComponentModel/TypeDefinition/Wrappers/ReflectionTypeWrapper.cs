using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intellitect.ComponentModel.TypeDefinition.Wrappers
{
    internal class ReflectionTypeWrapper : TypeWrapper
    {
        public ReflectionTypeWrapper(Type type)
        {
            Info = type;
        }

        public override string Name { get { return Info.Name; } }
        public override string NameWithTypeParams
        { get
            {
                if (IsArray) return $"{PureType.Name}[]";
                if (IsGeneric) return $"{Name}<{PureType.Name}>";
                return Name;
            }
        }

        public override object GetAttributeValue<TAttribute>(string valueName)
        {
            return Info.GetAttributeValue<TAttribute>(valueName);
        }
        public override bool HasAttribute<TAttribute>()
        {
            return Info.HasAttribute<TAttribute>();
        }

        public override bool IsA<T>()
        {
            return Info.IsSubclassOf(typeof(T)) || typeof(T) == Info;
        }

        public override bool IsGeneric { get { return Info.IsGenericType; } }
        
        public override bool IsCollection { get { return Info.GetInterface("IEnumerable") != null && !IsArray && !IsString; } }

        public override bool IsArray { get { return Info.IsArray; } }

        public override bool IsNullable { get { return Info.IsClass || IsNullableType; } }

        public override bool IsNullableType { get { return Info.Name.Contains("Nullable"); } }

        public override bool IsClass { get { return Info.IsClass; } }

        public override Dictionary<int, string> EnumValues
        {
            get
            {
                var result = new Dictionary<int, string>();
                foreach (var value in Enum.GetValues(Info))
                {
                    result.Add((int)value, value.ToString());
                }
                return result;
            }
        }

        public override bool IsEnum { get { return Info.IsEnum; } }

        public override string Namespace { get { return Info.Namespace; } }



        public override TypeWrapper PureType
        {
            get
            {
                if (IsGeneric && (IsCollection || IsNullable)) { return FirstTypeArgument; }
                else if (IsArray) { return ArrayType; }
                return this;
            }
        }

        public override TypeWrapper FirstTypeArgument { get { return new ReflectionTypeWrapper(Info.GenericTypeArguments.First()); } }

        public override TypeWrapper ArrayType { get { return new ReflectionTypeWrapper(Info.GetElementType()); } }

    }
}
