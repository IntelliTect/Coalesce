using Coalesce.Starter.Vue.Data.Auth;
using Coalesce.Starter.Vue.Data.Models;
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
    }
}