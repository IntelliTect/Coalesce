using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Tests.Util;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace IntelliTect.Coalesce.Tests.Tests.Models
{
    public class ListResultTests
    {
        [Fact]
        public void Constructor_FromListResult_CopiesPagination()
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
            Assert.Null(replica.List);
            // Everything else can and should be copied.
            Assert.Equal(original.IncludeTree, replica.IncludeTree);
            Assert.Equal(original.Page, replica.Page);
            Assert.Equal(original.PageSize, replica.PageSize);
            Assert.Equal(original.PageCount, replica.PageCount);
            Assert.Equal(original.TotalCount, replica.TotalCount);
            Assert.Equal(original.WasSuccessful, replica.WasSuccessful);
            Assert.Equal(original.Message, replica.Message);
        }

        [Fact]
        public void Constructor_FromApiResult_CopiesProperties()
        {
            var original = new ApiResult(false, "error")
            {
                IncludeTree = new IncludeTree(),
            };

            var replica = new ListResult<TestDto<ComplexModel>>(original);

            Assert.Equal(original.IncludeTree, replica.IncludeTree);
            Assert.Equal(original.WasSuccessful, replica.WasSuccessful);
            Assert.Equal(original.Message, replica.Message);
        }

        [Fact]
        public void Constructor_FromQuery_ComputesPagination()
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

            Assert.True(result.WasSuccessful);
            Assert.Null(result.Message);
            Assert.Equal(2, result.Page);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(4, result.PageCount);
            Assert.Equal(8, result.TotalCount);
            Assert.Equal("Foo", result.List[0].Name);
        }
    }
}
