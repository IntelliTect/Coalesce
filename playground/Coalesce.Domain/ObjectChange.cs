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
    public class ObjectChange : DefaultObjectChange
    {
        public string? Message { get; set; }

        // Convention: the frontend will treat props with "user" in their name specially.
        public int? UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public Person? User { get; set; }
    }

    internal class OperationContext : IAuditOperationContext<ObjectChange>
    {
        public OperationContext(IHttpContextAccessor accessor)
        {
            Accessor = accessor;
        }

        public IHttpContextAccessor Accessor { get; }

        public void Populate(ObjectChange auditEntry, EntityEntry changedEntity)
        {
            var db = ((AppDbContext)changedEntity.Context);

            auditEntry.UserId = db.People
                .Skip(Random.Shared.Next(db.People.Count()))
                .FirstOrDefault()?
                .PersonId;

            var context = Accessor.HttpContext;
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
