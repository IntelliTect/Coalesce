using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.TypeDefinition.Wrappers;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public class MethodViewModel
    {
        internal MethodWrapper Wrapper { get; }

        internal MethodViewModel(MethodWrapper wrapper, ClassViewModel parent)
        {
            Wrapper = wrapper;
            Parent = parent;
        }

        public bool IsStatic => Wrapper.IsStatic;

        public bool IsIQueryableOfParent =>
            IsStatic && ReturnType.IsA<IQueryable>() && ReturnType.PureType.Name == Parent.Name;

        public string JsVariable => Name.ToCamelCase();

        public string JsVariableResult => JsVariable + "Result";
        public string JsVariableResultRaw => JsVariable + "ResultRaw";
        public string JsVariableIsLoading => JsVariable + "IsLoading";
        public string JsVariableMessage => JsVariable + "Message";
        public string JsVariableWasSuccessful => JsVariable + "WasSuccessful";
        public string JsVariableUi => JsVariable + "Ui";
        public string JsVariableModal => JsVariable + "Modal";
        public string JsVariableArgs => JsVariable + "Args";
        public string JsVariableWithArgs => JsVariable + "WithArgs";

        public string Comment => Wrapper.Comment;

        /// <summary>
        /// Name of the property
        /// </summary>
        public string Name => Wrapper.Name;


        /// <summary>
        /// Name of the class that is used for storing arguments on the client.
        /// </summary>
        public string ArgsName => Wrapper.Name + "Args";


        /// <summary>
        /// Name of the type
        /// </summary>
        public TypeViewModel ReturnType => new TypeViewModel(Wrapper.ReturnType);


        /// <summary>
        /// Type of the return. Object if void.
        /// </summary>
        public string ReturnTypeNameForApi
        {
            get
            {
                string result = ReturnType.NameWithTypeParams;
                if (result == "Void") return "object";
                result = result.Replace("IQueryable", "IEnumerable");
                if (ReturnType.IsCollection && ReturnType.PureType.HasClassViewModel)
                {
                    result = result.Replace($"<{ReturnType.PureType.ClassViewModel.Name}>", $"<{ReturnType.PureType.ClassViewModel.DtoName}>");
                }
                else if (!ReturnType.IsCollection && ReturnType.HasClassViewModel)
                {
                    result = $"{ReturnType.ClassViewModel.DtoName}";
                }
                return result;
            }
        }

        /// <summary>
        /// List of parameters
        /// </summary>
        public IEnumerable<ParameterViewModel> Parameters => Wrapper.Parameters;

        /// <summary>
        /// List of parameters that are not Dependency Injected (DI)
        /// </summary>
        public IEnumerable<ParameterViewModel> ClientParameters => Wrapper.Parameters.Where(f => !f.IsDI);

        /// <summary>
        /// Gets the TypeScript parameters for this method call.
        /// </summary>
        public string TsParameters
        {
            get
            {
                string result = "";
                result = string.Join(", ", ClientParameters.Select(f => f.Type.TsDeclarationPlain(f.Name)));
                if (!string.IsNullOrWhiteSpace(result)) result = result + ", ";
                result = result + "callback: () => void = null, reload: boolean = true";
                return result;
            }
        }

        /// <summary>
        /// Gets the CS parameters for this method call.
        /// </summary>
        public string CsParameters
        {
            get
            {
                var parameters = Parameters.Where(f => !f.IsManualDI).ToArray();
                // When static add an id that specifies the object to work on.
                string result = "";
                if (!IsStatic)
                {
                    result = $"{Parent.PrimaryKey.PureType.Name} id";
                    if (parameters.Any()) result += ", ";
                }
                result += string.Join(", ", parameters.Select(f => f.CsDeclaration));
                return result;
            }
        }

        /// <summary>
        /// Gets the CS arguments passed to this method call.
        /// </summary>
        public string CsArguments
        {
            get
            {
                var result = string.Join(", ", Parameters.Select(f => f.CsArgumentName));
                return result;
            }
        }


        /// <summary>
        /// Gets the js arguments passed to this method call.
        /// </summary>
        public string JsArguments(string obj = "", bool callback = false)
        {
            string result;
            if (obj != "")
            {
                result = string.Join(", ", ClientParameters.Select(f => $"{obj}.{f.Name.ToCamelCase()}()"));
            }
            else
            {
                result = string.Join(", ", ClientParameters.Select(f => obj + f.Name.ToCamelCase()));
            }
            if (callback)
            {
                if (!string.IsNullOrEmpty(result))
                {
                    result = result + ", ";
                }
                result = result + "callback";
            }
            return result;
        }

        public string JsPostObject
        {
            get
            {
                var result = "{ ";
                if (!IsStatic)
                {
                    result = result + "id: this.myId";
                    if (Parameters.Any()) result = result + ", ";
                }

                result += string.Join(", ", ClientParameters.Select(f => $"{f.Name}: {f.TsConversion(f.Name)}"));
                result += " }";
                return result;

            }
        }


        public ClassViewModel Parent { get; }

        /// <summary>
        /// Gets the name for the API call.
        /// </summary>
        public string ApiUrl => $"{Parent.ApiUrl}/{Name}";

        /// <summary>
        /// Returns the DisplayName Attribute or 
        /// puts a space before every upper class letter aside from the first one.
        /// </summary>
        public string DisplayName
        {
            get
            {
                var displayName = Wrapper.GetAttributeValue<DisplayNameAttribute>(nameof(DisplayNameAttribute.DisplayName)) as string;
                if (displayName != null) return displayName;
                displayName = Wrapper.GetAttributeValue<DisplayAttribute>(nameof(DisplayAttribute.Name)) as string;
                if (displayName != null) return DisplayName;
                else return Regex.Replace(Name, "[A-Z]", " $0").Trim();
            }
        }



        /// <summary>
        /// For the specified area, returns true if the property has a hidden attribute.
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        public bool IsHidden(HiddenAttribute.Areas area)
        {
            var hiddenArea = (Nullable<HiddenAttribute.Areas>)Wrapper.GetAttributeValue<DisplayNameAttribute>(nameof(DisplayNameAttribute.DisplayName));
            if (hiddenArea == null) return false;
            return hiddenArea.Value == HiddenAttribute.Areas.All || hiddenArea.Value == area;
        }

        public SecurityInfoMethod SecurityInfo
        {
            get
            {
                var result = new SecurityInfoMethod();

                if (Wrapper.HasAttribute<ExecuteAttribute>())
                {
                    result.IsExecute = true;
                    var roles = (string)Wrapper.GetAttributeValue<ExecuteAttribute>(nameof(ExecuteAttribute.Roles));
                    result.ExecuteRoles = roles;
                }

                return result;
            }
        }

        /// <summary>
        /// Returns true if this method is marked as InternalUse. Not exposed through the API
        /// </summary>
        public bool IsInternalUse => Wrapper.IsInternalUse;

        /// <summary>
        /// If true, this is a client side method.
        /// </summary>
        public bool IsClientMethod
        {
            get
            {
                return !IsInternalUse && 
                    Name != "BeforeSave" && 
                    Name != "AfterSave" && 
                    Name != "BeforeDelete" && 
                    Name != "AfterDelete" &&
                    Name != "Exclude";
            }
        }
    }
}
