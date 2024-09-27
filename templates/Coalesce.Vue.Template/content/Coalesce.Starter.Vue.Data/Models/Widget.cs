using System.ComponentModel;

namespace Coalesce.Starter.Vue.Data.Models;

[Description("A sample model provided by the Coalesce template. Remove this when you start building your real data model.")]
public class Widget
#if Tenancy
    : TenantedBase
#elif TrackingBase
    : TrackingBase
#endif
{
    public int WidgetId { get; set; }

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
