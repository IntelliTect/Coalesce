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
        public static string ObservableConstructorCall(this TypeViewModel typeModel)
        {
            if (typeModel.IsCollection)
            {
                return "ko.observableArray([])";
            }
            else if (typeModel.IsDate)
            {
                if (typeModel.IsNullable)
                    return "ko.observable(null)";
                else
                    return "ko.observable(moment())";
            }
            else
            {
                return "ko.observable(null)";
            }
        }

        /// <summary>
        /// Type used in knockout for the observable with ViewModels.
        /// </summary>
        public static string TsKnockoutType(this TypeViewModel typeModel, bool nullable = false)
        {
            if (typeModel.IsCollection)
                return $"KnockoutObservableArray<{typeModel.PureType.TsType}>";
            else
                return $"KnockoutObservable<{typeModel.TsType}{(nullable ? " | null" : "")}>";
        }
    }
}
