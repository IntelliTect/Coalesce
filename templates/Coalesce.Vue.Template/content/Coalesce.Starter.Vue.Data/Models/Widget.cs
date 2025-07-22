using System.ComponentModel;

namespace Coalesce.Starter.Vue.Data.Models;

[Description("A sample model provided by the Coalesce template. Remove this when you start building your real data model.")]
#if AIChat
[SemanticKernel("A Widget represents a whimsical or fantastical invention.", DefaultDataSourceEnabled = true, SaveEnabled = true, DeleteEnabled = true)]
#endif
public class Widget
#if Tenancy
    : TenantedBase
#elif TrackingBase
    : TrackingBase
#endif
{
    public int WidgetId { get; set; }

    [Read(PermissionLevel = SecurityPermissionLevels.DenyAll)]
    public required string Name { get; set; }

    public required WidgetCategory Category { get; set; }

    public DateTimeOffset? InventedOn { get; set; }
}

public enum WidgetCategory
{
    Whizbangs,
    Sprecklesprockets,
    Discombobulators,
}
