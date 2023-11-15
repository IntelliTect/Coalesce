using IntelliTect.Coalesce.DataAnnotations;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Reflection;
using Z.EntityFramework.Plus;

namespace IntelliTect.Coalesce.AuditLogging;

public class AuditLoggingBuilder<TAuditLog>
    where TAuditLog : class, IAuditLog
{
    private readonly AuditOptions options;

    public AuditLoggingBuilder(AuditOptions options)
    {
        this.options = options;
    }

    /// <summary>
    /// Configures the operation context service that will be used to populate additional contextual
    /// fields on audit log entries. The service will be injected from the application service 
    /// provider if available; otherwise, a new instance will be constructed using dependencies 
    /// from the application service provider. To make an injected dependency optional, make the
    /// constructor parameter nullable with a default value of `null`, or create alternate constructors.
    /// </summary>
    public AuditLoggingBuilder<TAuditLog> WithAugmentation<T>()
        where T : IAuditOperationContext<TAuditLog>
    {
        options.OperationContextType = typeof(T);
        return this;
    }

    /// <inheritdoc cref="AuditOptions.MergeWindow"/> 
    public AuditLoggingBuilder<TAuditLog> WithMergeWindow(TimeSpan timeSpan)
    {
        options.MergeWindow = timeSpan;
        return this;
    }

    /// <inheritdoc cref="AuditOptions.Descriptions"/> 
    public AuditLoggingBuilder<TAuditLog> WithDescriptions(DescriptionMode mode)
    {
        options.Descriptions = mode;
        return this;
    }

    /// <inheritdoc cref="AuditOptions.PropertyDescriptions"/> 
    public AuditLoggingBuilder<TAuditLog> WithPropertyDescriptions(PropertyDescriptionMode mode)
    {
        options.PropertyDescriptions = mode;
        return this;
    }
    private static readonly MemoryCache _auditConfigTransforms = new(new MemoryCacheOptions { SizeLimit = 512 });

    /// <summary>
    /// <para>
    /// Configure type and property exclusions and property formatting rules with Z.EntityFramework.Plus.
    /// </para>
    /// <para>
    /// The provided configuration delegate should be static. If dynamic configuration is needed (highly unlikely),
    /// pass the dynamic inputs to the second argument so that configuration caching can account for this.
    /// </para>
    /// </summary>
    public AuditLoggingBuilder<TAuditLog> ConfigureAudit(Action<AuditConfiguration> configure)
        => ConfigureAudit<object?>((c, _) => configure(c), null, configure.Method);

    /// <inheritdoc cref="ConfigureAudit(Action{AuditConfiguration})"/>
    public AuditLoggingBuilder<TAuditLog> ConfigureAudit<TArg>(Action<AuditConfiguration, TArg> configure, TArg arg)
        => ConfigureAudit(configure, arg, configure.Method);

    private AuditLoggingBuilder<TAuditLog> ConfigureAudit<TArg>(Action<AuditConfiguration, TArg> configure, TArg arg, MethodInfo methodCacheKey)
    {
        options.AuditConfiguration = _auditConfigTransforms.GetOrCreate((methodCacheKey, options.AuditConfiguration, arg), entry =>
        {
            entry.Size = 1;

            var cacheKey = ((MethodInfo Method, AuditConfiguration? OldConfig, TArg Arg))entry.Key;

            var newConfig = cacheKey.OldConfig?.Clone() ?? new AuditConfiguration();
            configure(newConfig, cacheKey.Arg!);
            return newConfig;
        });

        return this;
    }
}
