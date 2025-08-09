using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Tests.Util;
using System;
using Xunit;

namespace IntelliTect.Coalesce.CodeGeneration.Tests
{
    public class ExecuteAttributeCacheTests
    {
        [Fact]
        public void MethodViewModel_ClientCacheDuration_ReturnsCorrectValueForCustomMethod()
        {
            var repo = ReflectionRepositoryFactory.Reflection;
            var model = repo.GetClassViewModel<ComplexModel>();
            var method = model.MethodByName(nameof(ComplexModel.DownloadAttachment_CustomCache));
            
            var duration = method.ClientCacheDuration;
            
            Assert.NotNull(duration);
            Assert.Equal(TimeSpan.FromSeconds(3600), duration); // 1 hour
        }

        [Fact]
        public void MethodViewModel_ClientCacheDuration_ReturnsNullForDefaultMethod()
        {
            var repo = ReflectionRepositoryFactory.Reflection;
            var model = repo.GetClassViewModel<ComplexModel>();
            var method = model.MethodByName(nameof(ComplexModel.DownloadAttachment_VaryString));
            
            var duration = method.ClientCacheDuration;
            
            Assert.Null(duration); // Should be null, meaning use default
        }
    }
}