using System.Runtime.CompilerServices;
using IntelliTect.Coalesce.DataAnnotations;

[assembly: InternalsVisibleTo("IntelliTect.Coalesce.Tests")]

namespace IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;


internal class InternalClass
{
}

[InternalUse]
public class InternalUseClass
{
}
