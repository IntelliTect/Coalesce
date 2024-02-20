using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IntelliTect.Coalesce.Tests.Tests
{
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
}
