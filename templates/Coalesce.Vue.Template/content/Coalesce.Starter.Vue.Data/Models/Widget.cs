namespace Coalesce.Starter.Vue.Data.Models;

public class Widget
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
