using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using Z.EntityFramework.Plus;

namespace IntelliTect.Coalesce.AuditLogging;

public class CoalesceAuditLoggingBuilder<TObjectChange>
    where TObjectChange : class, IObjectChange
{
    private readonly AuditOptions options;

    public CoalesceAuditLoggingBuilder(AuditOptions options)
    {
        this.options = options;
    }

    public CoalesceAuditLoggingBuilder<TObjectChange> WithAugmentation<T>()
        where T : IAuditOperationContext<TObjectChange>
    {
        options.OperationContextType = typeof(T);
        return this;
    }

    /// <inheritdoc cref="AuditOptions.MergeWindow"/> 
    public CoalesceAuditLoggingBuilder<TObjectChange> WithMergeWindow(TimeSpan timeSpan)
    {
        options.MergeWindow = timeSpan;
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
    public CoalesceAuditLoggingBuilder<TObjectChange> ConfigureAudit(Action<AuditConfiguration> configure)
        => ConfigureAudit<object?>((c, _) => configure(c), null, configure.Method);

    /// <inheritdoc cref="ConfigureAudit(Action{AuditConfiguration})"/>
    public CoalesceAuditLoggingBuilder<TObjectChange> ConfigureAudit<TArg>(Action<AuditConfiguration, TArg> configure, TArg arg)
        => ConfigureAudit(configure, arg, configure.Method);

    private CoalesceAuditLoggingBuilder<TObjectChange> ConfigureAudit<TArg>(Action<AuditConfiguration, TArg> configure, TArg arg, MethodInfo methodCacheKey)
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
