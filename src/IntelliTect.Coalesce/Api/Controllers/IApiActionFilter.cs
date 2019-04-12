using Microsoft.AspNetCore.Mvc.Filters;

namespace IntelliTect.Coalesce.Api.Controllers
{
    /// <summary>
    /// Marker interface for the filter that will handle error reponses and model validation for API Controllers.
    /// </summary>
    public interface IApiActionFilter : IActionFilter, IAlwaysRunResultFilter { }
}
