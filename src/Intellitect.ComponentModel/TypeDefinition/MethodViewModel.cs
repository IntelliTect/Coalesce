using Intellitect.ComponentModel.DataAnnotations;
using Intellitect.ComponentModel.TypeDefinition.Wrappers;
using Intellitect.ComponentModel.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Intellitect.ComponentModel.TypeDefinition
{
    public class MethodViewModel
    {
        internal MethodWrapper Wrapper { get; }

        internal MethodViewModel(MethodWrapper wrapper, ClassViewModel parent, int classMethodOrder)
        {
            Wrapper = wrapper;
            Parent = parent;
            ClassMethodOrder = classMethodOrder;
        }

        /// <summary>
        /// Order rank of the method in the model.
        /// </summary>
        public int ClassMethodOrder { get; }



        public bool IsStatic
        {
            get
            {
                return Wrapper.IsStatic;
            }
        }

        public bool IsIQueryableOfParent
        {
            get
            {
                return IsStatic && ReturnType.IsA<IQueryable>() && ReturnType.PureType.Name == Parent.Name;
            }
        }

        public string JsVariable { get { return Name.ToCamelCase(); } }
        public string JsVariableResult { get { return Name.ToCamelCase() + "Result"; } }
        public string JsVariableIsLoading { get { return Name.ToCamelCase() + "IsLoading"; } }
        public string JsVariableMessage { get { return Name.ToCamelCase() + "Message"; } }
        public string JsVariableWasSuccessful { get { return Name.ToCamelCase() + "WasSuccessful"; } }
        public string JsVariableUi { get { return Name.ToCamelCase() + "Ui"; } }
        public string JsVariableModal { get { return Name.ToCamelCase() + "Modal"; } }
        public string JsVariableArgs { get { return Name.ToCamelCase() + "Args"; } }
        public string JsVariableWithArgs { get { return Name.ToCamelCase() + "WithArgs"; } }

        public string Comment { get { return Wrapper.Comment; } }

        /// <summary>
        /// Name of the property
        /// </summary>
        public string Name { get { return Wrapper.Name; } }

        /// <summary>
        /// Name of the class that is used for storing arguments on the client.
        /// </summary>
        public string ArgsName { get { return Wrapper.Name + "Args"; } }


        /// <summary>
        /// Name of the type
        /// </summary>
        public TypeViewModel ReturnType
        {
            get
            {
                return new TypeViewModel(Wrapper.ReturnType);
            }
        }

        /// <summary>
        /// Type of the return. 
        /// </summary>
        public string ReturnTypeName
        {
            get
            {
                return ReturnType.NameWithTypeParams;
            }
        }


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
                result = (new Regex($"({Parent.Name}(?!(DtoGen)))")).Replace(result, $"{Parent.Name}DtoGen");
                return result;
            }
        }

        /// <summary>
        /// List of parameters
        /// </summary>
        public IEnumerable<ParameterViewModel> Parameters { get { return Wrapper.Parameters; } }

        /// <summary>
        /// List of parameters that are not Dependency Injected (DI)
        /// </summary>
        public IEnumerable<ParameterViewModel> ClientParameters { get { return Wrapper.Parameters.Where(f => !f.IsDI); } }

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
                result = result + "callback?: any";
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
                // When static add an id that specifies the object to work on.
                string result = "";
                if (!IsStatic)
                {
                    result = $"{Parent.PrimaryKey.PureType.Name} id";
                    if (ClientParameters.Any()) result += ", ";
                }
                result += string.Join(", ", ClientParameters.Select(f => f.Type.CsDeclaration(f.Name)));
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
                result = string.Join(", ", ClientParameters.Select(f => $"{obj}.{f.Name}()"));
            }
            else
            {
                result =  string.Join(", ", ClientParameters.Select(f => obj + f.Name));
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
                var result = "{" + Environment.NewLine;
                if (!IsStatic)
                {
                    result = result + "                        id: self.myId";
                    if (Parameters.Any()) result = result + ", " + Environment.NewLine;
                }

                result += string.Join(", " + Environment.NewLine, ClientParameters.Select(f => $"                        {f.Name}: {f.Name}"));
                result += Environment.NewLine + "                    }";
                return result;

            }
        }


        public ClassViewModel Parent { get; }

        /// <summary>
        /// Gets the name for the API call.
        /// </summary>
        public string ApiUrl
        {
            get
            {
                return $"{Parent.ApiUrl}/{Name}";
            }
        }

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
        public bool IsInternalUse
        {
            get
            {
                return Wrapper.HasAttribute<InternalUseAttribute>();
            }
        }


    }
}
