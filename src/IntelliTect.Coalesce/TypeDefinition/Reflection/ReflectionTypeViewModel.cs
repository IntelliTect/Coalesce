using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public class ReflectionTypeViewModel : TypeViewModel
    {
        protected internal Type Info { get; internal set; }

        public ReflectionTypeViewModel(Type type) : this(null, type)
        {
        }

        internal ReflectionTypeViewModel(ReflectionRepository reflectionRepository, Type type) : base(reflectionRepository)
        {
            ReflectionRepository = reflectionRepository;
            Info = type;

            FirstTypeArgument = IsGeneric && Info.IsConstructedGenericType
                ? ReflectionTypeViewModel.GetOrCreate(reflectionRepository, Info.GenericTypeArguments[0])
                : null;

            ArrayType = IsArray 
                ? ReflectionTypeViewModel.GetOrCreate(reflectionRepository, Info.GetElementType()) 
                : null;

            IEnumerable<Type> GetBaseClassesAndInterfaces(Type t)
            {
                // From https://stackoverflow.com/a/17143662
                return t.BaseType == null || t.BaseType == typeof(object)
                    ? t.GetInterfaces()
                    : Enumerable
                        .Repeat(t.BaseType, 1)
                        .Concat(t.GetInterfaces())
                        .Concat(GetBaseClassesAndInterfaces(t.BaseType))
                        .Distinct();
            }

            BaseClassesAndInterfaces = GetBaseClassesAndInterfaces(Info).Concat(new[] { Info }).ToList();

            ClassViewModel = ShouldCreateClassViewModel
                ? new ReflectionClassViewModel(this)
                : null;

            // This is precomputed because it is used for .Equals() and the == operator.
            FullyQualifiedName = GetFriendlyTypeName(Info);
        }

        internal static ReflectionTypeViewModel GetOrCreate(ReflectionRepository reflectionRepository, Type type)
        {
            return reflectionRepository?.GetOrAddType(type) ?? new ReflectionTypeViewModel(reflectionRepository, type);
        }


        // TODO: why is an arity of 1 removed from the name? Seems to be an oversight
        // - If we're removing arity, we should remove any arity.
        public override string Name => Info.Name.Replace("`1", "");

        public override object GetAttributeValue<TAttribute>(string valueName) =>
            Info.GetAttributeValue<TAttribute>(valueName);

        public override bool HasAttribute<TAttribute>() =>
            Info.HasAttribute<TAttribute>();

        private ICollection<Type> BaseClassesAndInterfaces { get; }

        private Type GetSatisfyingBaseType(Type type)
        {
            return BaseClassesAndInterfaces
                .FirstOrDefault(x =>
                       x.Equals(type)
                    || x.IsSubclassOf(type)
                    || type.IsAssignableFrom(x)
                    || (x.IsGenericType && x.GetGenericTypeDefinition() == type)
                );
        }

        public override TypeViewModel[] GenericArgumentsFor(Type type) =>
            GetSatisfyingBaseType(type)?
            .GenericTypeArguments
            .Select(t => ReflectionTypeViewModel.GetOrCreate(ReflectionRepository, t))
            .ToArray();

        public override bool IsA(Type type) => GetSatisfyingBaseType(type) != null;

        public override bool IsGeneric => Info.IsGenericType;

        public override bool IsArray => Info.IsArray;

        public override bool IsNullable => Info.IsClass || IsNullableType;

        public override bool IsClass => Info.IsClass;

        public override bool IsInterface => Info.IsInterface;

        public override bool IsInternalUse => base.IsInternalUse || !Info.IsVisible;

        public override bool IsVoid => Info == typeof(void);

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

        public override bool IsEnum =>
            IsNullableType ? NullableUnderlyingType.IsEnum : Info.IsEnum;

        public override string FullNamespace => Info.Namespace;

        private static string GetFriendlyTypeName(Type type)
        {
            // From https://stackoverflow.com/questions/401681

            if (type.IsGenericParameter)
            {
                return type.Name;
            }

            if (!type.IsGenericType)
            {
                return type.FullName;
            }

            var builder = new System.Text.StringBuilder();
            var name = type.Name;
            var index = name.IndexOf("`");
            builder.AppendFormat("{0}.{1}", type.Namespace, name.Substring(0, index));
            builder.Append('<');
            var first = true;
            foreach (var arg in type.GetGenericArguments())
            {
                if (!first)
                {
                    builder.Append(',');
                }
                builder.Append(GetFriendlyTypeName(arg));
                first = false;
            }
            builder.Append('>');
            return builder.ToString();
        }

        public override string FullyQualifiedName { get; }

        // TODO: write tests that assert that this will format types the same 
        // way as SymbolTypeViewModel.VerboseFullyQualifiedName. Adjust either one as needed.
        public override string VerboseFullyQualifiedName => FullyQualifiedName;

        public override TypeViewModel FirstTypeArgument { get; }

        public override TypeViewModel ArrayType { get; }

        public override ClassViewModel ClassViewModel { get; }

        public override Type TypeInfo => Info;

        public override bool EqualsType(TypeViewModel b) => b is ReflectionTypeViewModel r ? Info == r.Info : false;

        public override bool Equals(object obj)
        {
            if (!(obj is ReflectionTypeViewModel that)) return base.Equals(obj);

            return Info == that.Info;
        }
    }
}
