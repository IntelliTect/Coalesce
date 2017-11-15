using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace IntelliTect.Coalesce.TypeDefinition.Wrappers
{
    internal class ReflectionTypeViewModel : TypeViewModel
    {
        protected internal Type Info { get; internal set; }

        public ReflectionTypeViewModel(Type type)
        {
            Info = type;
        }

        public override string Name => Info.Name.Replace("`1", "");

        public override object GetAttributeValue<TAttribute>(string valueName) =>
            Info.GetAttributeValue<TAttribute>(valueName);

        public override bool HasAttribute<TAttribute>() =>
            Info.HasAttribute<TAttribute>();

        public override bool IsA<T>() => 
            Info.IsSubclassOf(typeof(T)) || typeof(T).IsAssignableFrom(Info) || typeof(T) == Info;

        public override bool IsGeneric => Info.IsGenericType;

        public override bool IsCollection => Info.GetInterface("IEnumerable") != null && !IsArray && !IsString;

        public override bool IsArray => Info.IsArray;

        public override bool IsNullable => Info.IsClass || IsNullableType;

        public override bool IsNullableType => Info.Name.Contains("Nullable");

        public override bool IsClass => Info.IsClass;

        public override Dictionary<int, string> EnumValues
        {
            get
            {
                var result = new Dictionary<int, string>();
                var info = Info;
                if (IsNullableType)
                {
                    info = Nullable.GetUnderlyingType(info);
                }
                foreach (var value in Enum.GetValues(info))
                {
                    result.Add((int)value, value.ToString());
                }
                return result;
            }
        }

        public override bool IsEnum => Info.IsEnum;

        public override string Namespace => Info.Namespace;

        public override string FullNamespace => Info.Namespace;


        public override TypeViewModel FirstTypeArgument => 
            new ReflectionTypeViewModel(Info.GenericTypeArguments.First());

        public override TypeViewModel ArrayType => 
            new ReflectionTypeViewModel(Info.GetElementType());

        public override ClassViewModel ClassViewModel
        {
            get
            {
                if (!HasClassViewModel) return null;
                if (PureType != this) return PureType.ClassViewModel;
                if (Info != null) return ReflectionRepository.GetClassViewModel(Info);
                return null;
            }
        }

    }
}
