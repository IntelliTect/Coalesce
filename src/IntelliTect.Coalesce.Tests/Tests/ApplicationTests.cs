using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using Microsoft.Extensions.DependencyInjection;

namespace IntelliTect.Coalesce.Tests.Tests;

public class ApplicationTests
{
    [Fact]
    public void AddCoalesce_DoesNotExplicitlyRequireWebServices()
    {
        // Ensures that esoteric testing scenarios can setup Coalesce services
        // without a full aspnetcore host builder.

        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>();
        services.AddCoalesce(b => b.AddContext<AppDbContext>());

        var sp = services.BuildServiceProvider();

        Assert.NotNull(sp.GetRequiredService<CrudContext<AppDbContext>>());
    }
}
