using Intellitect.ComponentModel.Controllers;
// Model Namespaces
    using Coalesce.Domain;
    using Coalesce.Domain.External;

namespace Coalesce.Web.Api
{
    // This class allows developers to inject base class behaviors into the inheritance chain
    // This file should not be modified, but another partial class should be created where your custom behavior can be placed.
    public partial class LocalBaseApiController<T> : BaseApiController<T, AppContext>
        where T : class, new()
    {
    }
}