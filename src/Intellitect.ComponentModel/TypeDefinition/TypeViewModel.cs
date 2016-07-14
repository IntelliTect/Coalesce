using Intellitect.ComponentModel.TypeDefinition.Wrappers;
using Intellitect.ComponentModel.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Intellitect.ComponentModel.TypeDefinition
{
    public class TypeViewModel
    {
        internal TypeWrapper Wrapper { get; }

        internal TypeViewModel(TypeWrapper wrapper)
        {
            Wrapper = wrapper;
        }

        public string Name
        {
            get
            {
                if (Wrapper.IsArray) return "Array";
                return Wrapper.Name;
            }
        }

        public string CsDefaultValue
        {
            get
            {
                if (IsString) { return ""; }
                if (IsPOCO) { return "null"; }
                if (IsEnum) { return "0"; }
                if (IsNumber) { return "0"; }
                if (IsDateTime) { return "DateTime.MinValue"; }
                if (IsDateTimeOffset) { return "DateTimeOffset.MinValue"; }
                if (IsBool) { return "false"; }
                return "null";
            }
        }

        public string CsConvertFromString
        {
            get
            {
                if (IsString) { return ""; }
                if (IsPOCO) { return "(object)"; }
                if (IsEnum) { return "Convert.ToInt32"; }
                if (IsNumber) { return "Convert.To" + Name; }
                if (IsDateTime) { return "DateTime.Parse"; }
                if (IsDateTimeOffset) { return "DateTimeOffset.Parse"; }
                if (IsBool) { return "Convert.ToBoolean"; }
                return "(object)";
            }
        }

        /// <summary>
        /// Returns true if this type inherits from T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsA<T>()
        {
            return Wrapper.IsA<T>();
        }

        public string FullName { get { return $"{Wrapper.Namespace}.{Name}"; } }

        public string NameWithTypeParams { get { return $"{Wrapper.NameWithTypeParams}"; } }

        /// <summary>
        /// Returns true if the property is an array.
        /// </summary>
        public bool IsArray { get { return Wrapper.IsArray; } }

        /// <summary>
        /// Returns true if the property is a collection.
        /// </summary>
        public bool IsCollection { get { return Wrapper.IsCollection; } }

        /// <summary>
        /// True if this is a boolean.
        /// </summary>
        public bool IsBool { get { return Wrapper.IsBool; } }

        /// <summary>
        /// Returns true if the property is nullable.
        /// </summary>
        public bool IsNullable { get { return Wrapper.IsNullable; } }

        /// <summary>
        /// Returns true if the property returns void.
        /// </summary>
        public bool IsVoid { get { return Name == "Void"; } }



        /// <summary>
        /// Returns the first generic argument for a generic type.
        /// </summary>
        public TypeViewModel FirstTypeArgument { get { return new TypeViewModel(Wrapper.FirstTypeArgument); } }

        /// <summary>
        /// Type used in knockout for the observable.
        /// </summary>
        public string TsType
        {
            get
            {
                if (Wrapper.IsTimeZoneInfo) return "any";
                if (IsBool) return "boolean";
                if (IsDate) return "moment.Moment";
                if (IsCollection && IsNumber) return "number[]";
                if (IsCollection) return PureType + "[]";
                if (IsClass) return PureType.Name;
                if (IsEnum) return "number";
                if (IsNumber) return "number";
                return "any";
            }
        }

        /// <summary>
        /// Type used in knockout for the observable.
        /// </summary>
        public string TsConvertFromString(string expression)
        {
            if (IsBool) return $"({expression}.toUpperCase() == 'TRUE')";
            if (IsEnum) return $"parseInt({expression})";
            if (IsNumber) return $"parseFloat({expression})";
            if (IsDate) return $"moment({expression})";
            return expression;
        }


        /// <summary>
        /// Type used in knockout for the observable.
        /// </summary>
        public string TsTypePlain
        {
            get
            {
                if (this.Name == nameof(TimeZoneInfo)) return "any";
                if (IsBool) return "boolean";
                if (IsDate) return "moment.Moment";
                if (IsClass) return PureType.Name;
                if (IsEnum) return "number";
                if (IsNumber) return "number";
                return "any";
            }
        }

        /// <summary>
        /// Type used in knockout for the observable.
        /// </summary>
        public string JsKnockoutType
        {
            get
            {
                if (IsCollection || IsArray) return "ko.observableArray([])";
                if (IsComplexType) return "ko.observable(null)";
                else if (IsDate)
                {
                    if (IsNullable) return "ko.observable(null)";
                    else return "ko.observable(moment())";
                }
                else return "ko.observable(null)";
            }
        }

        /// <summary>
        /// Type used in knockout for the observable with ViewModels.
        /// </summary>
        public string TsKnockoutType
        {
            get
            {
                if ((IsArray || IsCollection) && IsNumber) return "KnockoutObservableArray<number>";
                if (Wrapper.IsTimeZoneInfo) return "KnockoutObservable<any>";
                else if (IsCollection && HasClassViewModel) return "KnockoutObservableArray<ViewModels." + ClassViewModel.ViewModelClassName + ">";
                else if (IsCollection) return "KnockoutObservableArray<any>";
                else if (IsString) return "KnockoutObservable<string>";
                else if (IsPOCO && HasClassViewModel) return "KnockoutObservable<ViewModels." + ClassViewModel.ViewModelClassName + ">";
                else return "KnockoutObservable<" + TsType + ">";
            }
        }


        /// <summary>
        /// Type used in knockout for the observable without viewmodels.
        /// </summary>
        public string TsKnockoutTypePlain
        {
            get
            {
                if ((IsArray || IsCollection) && IsNumber) return "KnockoutObservableArray<number>";
                else if (IsCollection) return "KnockoutObservableArray<any>";
                else if (IsString) return "KnockoutObservable<string>";
                else return "KnockoutObservable<" + TsType + ">";
            }
        }

        public bool HasClassViewModel
        {
            get
            {
                return ClassViewModel != null;
            }
        }

        public ClassViewModel ClassViewModel
        {
            get
            {
                if (IsPOCO) return PureType.Wrapper.ClassViewModel;
                return null;
            }
        }




        /// <summary>
        /// Returns true if this is a complex type.
        /// </summary>
        public bool IsComplexType
        {
            get
            {
                return Wrapper.HasAttribute<ComplexTypeAttribute>();
            }
        }

        /// <summary>
        /// True if this is a DateTime or DateTimeOffset.
        /// </summary>
        public bool IsDate { get { return Wrapper.IsDate; } }

        /// <summary>
        /// True if the property is a string.
        /// </summary>
        public bool IsString { get { return Wrapper.IsString; } }

        /// <summary>
        /// True if the property is a DateTime or Nullable DateTime
        /// </summary>
        public bool IsDateTime { get { return Wrapper.IsDateTime; } }

        /// <summary>
        /// True if the property is a DateTimeOffset or Nullable DateTimeOffset
        /// </summary>
        public bool IsDateTimeOffset { get { return Wrapper.IsDateTimeOffset; } }

        /// <summary>
        /// Returns true if the class is an enum.
        /// </summary>
        public bool IsNumber
        {
            get
            {
                switch (PureType.Name)
                {
                    case nameof(Byte):
                    case nameof(Int32):
                    case nameof(Single):
                    case nameof(Double):
                    case nameof(Decimal):
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Gets the type name without any collection around it.
        /// </summary>
        public TypeViewModel PureType
        {
            get
            {
                return new TypeViewModel(Wrapper.PureType);
            }
        }

        /// <summary>
        /// Returns true if the property type is a class.
        /// </summary>
        public bool IsClass { get { return Wrapper.IsClass; } }

        /// <summary>
        /// Returns true if the property is class outside the system NameSpace, but is not a string, array, or filedownload
        /// </summary>
        public bool IsPOCO
        {
            get
            {
                return
                    !IsArray && !IsCollection &&
                    !FullName.StartsWith("System") && 
                    Wrapper.IsClass;
                // && IsFileDownload;
            }
        }

        public bool IsGeneric { get { return Wrapper.IsGeneric; } }


        public bool IsEnum { get { return Wrapper.IsEnum; } }

        public string TsDeclaration
        {
            get
            {
                return $"{Name}: {TsType}";
            }
        }
        public string TsDeclarationPlain(string parameterName)
        {
            return $"{parameterName}: {TsTypePlain}";
        }

        public string CsDeclaration(string parameterName)
        {
            return $"{Name} {parameterName}";
        }



        /// <summary>
        /// Returns all the possible enumeration values.
        /// </summary>
        public Dictionary<int, string> EnumValues { get { return Wrapper.EnumValues; } }

        // This doesn't work correctly, but we will probably need it at some point.
        //public string FullType { get
        //    {
        //        if (IsGeneric) return $"{Wrapper.Namespace}.{Name}<{FirstTypeArgument}>";
        //        if (IsArray) return $"{Name}[]";
        //        return $"{Wrapper.Namespace}.{Name}";
        //    }
        //}

        public string NullableTypeForDto
        {
            get
            {
                string typeName = "";
                if (Wrapper.IsNullable || Wrapper.IsArray) typeName = Wrapper.NameWithTypeParams;
                else typeName = Wrapper.Name + "?";

                //var model = ReflectionRepository.Models.Where(m => m.OnContext && m.Name == typeName).FirstOrDefault();
                var model = ReflectionRepository.GetClassViewModel(Wrapper.PureType.Name);
                if (model != null && model.OnContext)
                {
                    typeName = (new Regex($"({model.Name}(?!(DtoGen)))")).Replace(typeName, $"{model.Name}DtoGen");
                }

                return typeName;
            }
        }

        public string ExplicitConversionType
        {
            get
            {
                if (Wrapper.IsNullable || Wrapper.IsArray) return "";
                else return $"({Wrapper.Name})";
            }
        }
    }
}
