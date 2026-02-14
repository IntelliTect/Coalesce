using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Tests.Tests;

public class ApplicationTests
{
    [Test]
    public async Task AddCoalesce_DoesNotExplicitlyRequireWebServices()
    {
        // Ensures that esoteric testing scenarios can setup Coalesce services
        // without a full aspnetcore host builder.

        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>();
        services.AddCoalesce(b => b.AddContext<AppDbContext>());

        var sp = services.BuildServiceProvider();

        await Assert.That(sp.GetRequiredService<CrudContext<AppDbContext>>()).IsNotNull();
    }
}