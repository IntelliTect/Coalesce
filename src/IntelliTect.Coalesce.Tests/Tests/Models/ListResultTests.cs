using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Tests.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Tests.Tests.Models;

public class ListResultTests
{
    [Test]
    public async Task Constructor_FromListResult_CopiesPagination()
    {
        var model = new ComplexModel();
        var original = new ListResult<ComplexModel>()
        {
            Page = 1,
            PageSize = 2,
            TotalCount = 3,
            IncludeTree = new IncludeTree(),
            List = [model],
            Message = "Test",
            WasSuccessful = true,
        };

        var replica = new ListResult<TestDto<ComplexModel>>(original);

        // List can't be copied over because the type is different.
        await Assert.That(replica.List).IsNull();
        // Everything else can and should be copied.
        await Assert.That(replica.IncludeTree).IsEqualTo(original.IncludeTree);
        await Assert.That(replica.Page).IsEqualTo(original.Page);
        await Assert.That(replica.PageSize).IsEqualTo(original.PageSize);
        await Assert.That(replica.PageCount).IsEqualTo(original.PageCount);
        await Assert.That(replica.TotalCount).IsEqualTo(original.TotalCount);
        await Assert.That(replica.WasSuccessful).IsEqualTo(original.WasSuccessful);
        await Assert.That(replica.Message).IsEqualTo(original.Message);
    }

    [Test]
    public async Task Constructor_FromApiResult_CopiesProperties()
    {
        var original = new ApiResult(false, "error")
        {
            IncludeTree = new IncludeTree(),
        };

        var replica = new ListResult<TestDto<ComplexModel>>(original);

        await Assert.That(replica.IncludeTree).IsEqualTo(original.IncludeTree);
        await Assert.That(replica.WasSuccessful).IsEqualTo(original.WasSuccessful);
        await Assert.That(replica.Message).IsEqualTo(original.Message);
    }

    [Test]
    public async Task Constructor_FromQuery_ComputesPagination()
    {
        var query = new List<ComplexModel>(){
            new(),
            new(),
            new() { Name = "Foo" },
            new(),
            new(),
            new(),
            new(),
            new(),
        }.AsQueryable();

        var result = new ListResult<ComplexModel>(query, 2, 2);

        await Assert.That(result.WasSuccessful).IsTrue();
        await Assert.That(result.Message).IsNull();
        await Assert.That(result.Page).IsEqualTo(2);
        await Assert.That(result.PageSize).IsEqualTo(2);
        await Assert.That(result.PageCount).IsEqualTo(4);
        await Assert.That(result.TotalCount).IsEqualTo(8);
        await Assert.That(result.List[0].Name).IsEqualTo("Foo");
    }
}