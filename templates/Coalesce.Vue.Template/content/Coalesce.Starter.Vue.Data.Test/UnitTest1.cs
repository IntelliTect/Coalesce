namespace Coalesce.Starter.Vue.Data.Test;

public class UnitTest1 : TestBase
{
    [Test]
    public async Task Test1()
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
        await Assert.That(widget2.Category).IsEqualTo(WidgetCategory.Sprecklesprockets);

        // After calling RefreshServices, we have a different DbContext instance
        // and so we'll get a different entity instance.
        await Assert.That(widget1).IsNotEqualTo(widget2);
#endif
    }
}