using IntelliTect.Coalesce.TypeDefinition;
using System.Collections.Generic;
using System.Linq;

namespace IntelliTect.Coalesce.CodeGeneration.Vue.Utils
{
    public static class VueHelperExtensions
    {
        public static IEnumerable<(ParameterViewModel Param, string Signature, bool IsRequired)> SignatureData(
            this IEnumerable<ParameterViewModel> parameters, 
            VueType.Flags paramTypeFlags)
        {
            // Determine where the last required parameter is.
            // We treat any parameter that comes before that as also required
            // for purposes of generating a method signature, even if the param
            // is actually optional, since optional parameters can only come after all required params.
            var paramList = parameters.ToList();
            var lastRequired = paramList.LastOrDefault(p => p.IsRequired);
            HashSet<ParameterViewModel> forcedRequired = lastRequired is null 
                ? [] 
                : paramList.TakeWhile(p => p != lastRequired).ToHashSet();

            return paramList.Select(p => (
                p, 
                p.JsVariable +
                    (!IsRequired(p) ? "?" : "") +
                    ": " +
                    $"{new VueType(p.Type, paramTypeFlags).TsType("$models")} | null",
                IsRequired(p)
            ));

            bool IsRequired(ParameterViewModel p) => p.IsRequired || forcedRequired.Contains(p);
        }
    }
}
