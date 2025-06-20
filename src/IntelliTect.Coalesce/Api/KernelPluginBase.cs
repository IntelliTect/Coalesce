using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Api
{
    public class KernelPluginBase<T>(CrudContext context)
        where T : class
    {
        protected CrudContext Context { get; } = context;

        protected ClaimsPrincipal User => Context.User;

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

        /// <summary>
        /// Workaround for https://github.com/microsoft/semantic-kernel/issues/12532 so we can control JSON serialization.
        /// </summary>
        protected async Task<string> Json(Func<Task<object>> func)
        {
            return JsonSerializer.Serialize(await func(), new JsonSerializerOptions { 
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            });
        }
    }

}
