using System.ComponentModel;

namespace Coalesce.Starter.Vue.Data.Models;

[Description("A sample model provided by the Coalesce template. Remove this when you start building your real data model.")]
#if AIChat
[SemanticKernel("A Widget represents a whimsical or fantastical invention.", SaveEnabled = true, DeleteEnabled = true)]
#endif
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

    [SemanticKernel("Provides records for whimsical widgets.")]
    public class AISource(CrudContext<AppDbContext> context) : AppDataSource<Widget>(context) { }
}

public enum WidgetCategory
{
    Whizbangs,
    Sprecklesprockets,
    Discombobulators,
}
