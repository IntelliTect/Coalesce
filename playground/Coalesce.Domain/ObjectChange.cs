using IntelliTect.Coalesce.AuditLogging;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Coalesce.Domain
{
    public class ObjectChange : ObjectChangeBase
    {
        public string? Message { get; set; }
    }

    internal class OperationContext : IAuditOperationContext<ObjectChange>
    {
        public OperationContext(IHttpContextAccessor accessor)
        {
            Accessor = accessor;
        }

        public IHttpContextAccessor Accessor { get; }

        public void Populate(ObjectChange auditEntry, EntityEntry entity)
        {
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
