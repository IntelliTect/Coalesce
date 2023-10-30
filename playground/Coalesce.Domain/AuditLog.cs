using IntelliTect.Coalesce.AuditLogging;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Coalesce.Domain
{
    public class AuditLog : DefaultAuditLog
    {
        public string? Message { get; set; }

        // Convention: the frontend will treat props with "user" in their name specially.
        public int? UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public Person? User { get; set; }
    }

    internal class OperationContext : DefaultAuditOperationContext<AuditLog>
    {
        public OperationContext(IHttpContextAccessor accessor) : base(accessor) { }

        public override void Populate(AuditLog auditEntry, EntityEntry changedEntity)
        {
            base.Populate(auditEntry, changedEntity);

            var db = ((AppDbContext)changedEntity.Context);

            auditEntry.UserId = db.People
                .Skip(Random.Shared.Next(db.People.Count()))
                .FirstOrDefault()?
                .PersonId;

            var context = HttpContext;
            if (context is not null)
            {
                auditEntry.Message = $"Changed by {context.Connection.RemoteIpAddress} on {context.Request.Headers.Referer}";
            }
            else
            {
                auditEntry.Message = "Changed by System";
            }

        }
    }
}
