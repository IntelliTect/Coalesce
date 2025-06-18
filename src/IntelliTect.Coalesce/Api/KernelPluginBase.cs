using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Api
{
    public class KernelPluginBase<T>(CrudContext context)
        where T : class
    {
        protected CrudContext Context { get; } = context;

        protected ClassViewModel GeneratedForClassViewModel { get; } = context.ReflectionRepository.GetClassViewModel<T>()!;

        protected IServiceProvider ServiceProvider => Context.ServiceProvider!;

        protected bool _isScoped;

        /// <summary>
        /// Performs an invocation in a new service scope so that previous results 
        /// (i.e. DbContext ChangeTracker entries)
        /// don't interfere with subsequent invocations when the kernel makes multiple calls
        /// to service one request/message.
        /// </summary>
        protected async Task<TResult> InvokeScoped<TResult>(Delegate action, params object[] args)
        {
            using var scope = ServiceProvider.CreateScope();
            var scopedInstance = (KernelPluginBase<T>)ActivatorUtilities.GetServiceOrCreateInstance(scope.ServiceProvider, this.GetType());
            scopedInstance._isScoped = true;
            return await (Task<TResult>)action.Method.Invoke(scopedInstance, args)!;
        }

        protected string Json(object obj)
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles });
        }
    }

}
