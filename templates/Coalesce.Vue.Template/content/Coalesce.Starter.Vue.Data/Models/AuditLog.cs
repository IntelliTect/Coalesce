using IntelliTect.Coalesce.AuditLogging;

namespace Coalesce.Starter.Vue.Data.Models;

[Edit(DenyAll)]
[Delete(DenyAll)]
[Create(DenyAll)]
[Read(nameof(Permission.ViewAuditLogs))]
public class AuditLog : DefaultAuditLog
{
    public string? UserId { get; set; }

    [Display(Name = "Changed By")]
    public AppUser? User { get; set; }
}