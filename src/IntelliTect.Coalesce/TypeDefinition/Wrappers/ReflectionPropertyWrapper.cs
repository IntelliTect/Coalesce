using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace IntelliTect.Coalesce.TypeDefinition.Wrappers
{
    internal class ReflectionPropertyWrapper : PropertyWrapper
    {
        protected PropertyInfo Info { get; }

        public ReflectionPropertyWrapper(PropertyInfo propetyInfo)
        {
            Info = propetyInfo;
        }


        public override string Name
        {
            get
            {
                return Info.Name;
            }
        }

        public override string Comment { get { return ""; } }



        public override TypeWrapper Type
        {
            get
            {
                return new ReflectionTypeWrapper(Info.PropertyType);
            }
        }

        public override bool CanRead { get { return Info.CanRead; } }

        public override bool CanWrite { get { return Info.CanWrite; } }

        public override bool IsReadOnly { get { return Info.CanRead && !Info.CanWrite; } }

        public override PropertyInfo PropertyInfo { get { return Info; } }

        public override bool IsVirtual
        {
            get
            {
                var getter = Info.GetGetMethod();
                if (getter != null)
                {
                    return getter.IsVirtual;
                }

                var setter = Info.GetSetMethod();
                if (setter != null)
                {
                    return setter.IsVirtual;
                }
                return false;
            }
        }

        public override bool IsStatic
        {
            get
            {
                var getter = Info.GetGetMethod();
                if (getter != null)
                {
                    return getter.IsStatic;
                }

                var setter = Info.GetSetMethod();
                if (setter != null)
                {
                    return setter.IsStatic;
                }
                return false;
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
    }
}
