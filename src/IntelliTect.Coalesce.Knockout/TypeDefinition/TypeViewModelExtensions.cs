using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTect.Coalesce.Knockout.TypeDefinition
{
    public static class TypeViewModelExtensions
    {
        /// <summary>
        /// Type used in knockout for the observable.
        /// </summary>
        public static string JsKnockoutType(this TypeViewModel typeModel)
        {
            if (typeModel.IsByteArray) return "ko.observable(null)";
            if (typeModel.IsCollection || typeModel.IsArray) return "ko.observableArray([])";
            else if (typeModel.IsDate)
            {
                if (typeModel.IsNullable) return "ko.observable(null)";
                else return "ko.observable(moment())";
            }
            else return "ko.observable(null)";
        }

        /// <summary>
        /// Type used in knockout for the observable with ViewModels.
        /// </summary>
        public static string TsKnockoutType(this TypeViewModel typeModel)
        {
            if (typeModel.IsByteArray) return "KnockoutObservable<string>";
            else if ((typeModel.IsArray || typeModel.IsCollection) && (typeModel.PureType.IsNumber)) return "KnockoutObservableArray<number>";
            else if ((typeModel.IsArray || typeModel.IsCollection) && (typeModel.PureType.IsString)) return "KnockoutObservableArray<string>";
            else if (typeModel.IsCollection && typeModel.HasClassViewModel) return "KnockoutObservableArray<ViewModels." + typeModel.ClassViewModel.ViewModelClassName + ">";
            else if (typeModel.IsCollection || typeModel.IsArray) return "KnockoutObservableArray<any>";
            else if (typeModel.IsString) return "KnockoutObservable<string>";
            else if (typeModel.IsPOCO && typeModel.HasClassViewModel) return "KnockoutObservable<ViewModels." + typeModel.ClassViewModel.ViewModelClassName + ">";
            else return "KnockoutObservable<" + typeModel.TsType + ">";
        }
    }
}
