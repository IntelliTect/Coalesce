using IntelliTect.Coalesce.AuditLogging;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Coalesce.Starter.Vue.Data.Coalesce;

public class AuditOperationContext : DefaultAuditOperationContext<AuditLog>
{
    public AuditOperationContext(IHttpContextAccessor accessor) : base(accessor) { }

    public override void Populate(AuditLog auditEntry, EntityEntry changedEntity)
    {
        base.Populate(auditEntry, changedEntity);

        auditEntry.UserId = User?.GetUserId();

#if Tenancy
        auditEntry.TenantId = changedEntity.Entity switch
        {
            ITenanted tenanted => tenanted.TenantId,
            Tenant tenant => tenant.TenantId,
            _ => auditEntry.TenantId
        };

        if (auditEntry.TenantId is not null)
        {
            // Strip the TenantId out of the primary key because we stored it in its own column.
            var tenantKeyPrefix = $"{auditEntry.TenantId};";
            auditEntry.KeyValue = auditEntry.KeyValue?.Replace(tenantKeyPrefix, "");
        }
#endif
    }
}