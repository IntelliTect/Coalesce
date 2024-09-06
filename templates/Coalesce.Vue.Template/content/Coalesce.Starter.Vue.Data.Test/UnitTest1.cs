namespace Coalesce.Starter.Vue.Data.Test;

public class UnitTest1 : TestBase
{
    [Fact]
    public void Test1()
    {
#if ExampleModel
        // Arrange
        var widget1 = new Widget { Name = "Gnoam Sprecklesprocket", Category = WidgetCategory.Sprecklesprockets };
        Db.Add(widget1);
        Db.SaveChanges();

        RefreshServices();

        // Act
        var widget2 = Db.Widgets.Single();

        // Assert
        Assert.Equal(WidgetCategory.Sprecklesprockets, widget2.Category);

        // After calling RefreshServices, we have a different DbContext instance
        // and so we'll get a different entity instance.
        Assert.NotEqual(widget1, widget2);
#endif
    }
}