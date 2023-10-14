using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Security.Claims;

namespace IntelliTect.Coalesce.AuditLogging;

public class DefaultAuditOperationContext<TObjectChange> : IAuditOperationContext<TObjectChange>
    where TObjectChange : DefaultObjectChange
{
    public DefaultAuditOperationContext(IHttpContextAccessor httpContextAccessor)
    {
        HttpContextAccessor = httpContextAccessor;
    }

    private IHttpContextAccessor HttpContextAccessor { get; }

    protected HttpContext? HttpContext => HttpContextAccessor.HttpContext;

    public ClaimsPrincipal? User => HttpContext?.User;

    public virtual void Populate(TObjectChange auditEntry, EntityEntry changedEntity)
    {
        // read only once from the underlying AsyncLocal for perf:
        var context = HttpContext;

        auditEntry.ClientIp = context?.Connection?.RemoteIpAddress?.ToString();
        auditEntry.Referrer = context?.Request?.GetTypedHeaders()?.Referer?.PathAndQuery;
        auditEntry.Endpoint = context?.Request?.Path;
    }
}