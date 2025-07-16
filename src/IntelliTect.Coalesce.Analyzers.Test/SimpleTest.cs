using System.Threading.Tasks;
using Xunit;

namespace IntelliTect.Coalesce.Analyzers.Test
{
    public class SimpleTest
    {
        [Fact]
        public async Task AnalyzerBuildsCorrectly()
        {
            // This is a simple test to ensure the analyzer builds and runs
            var analyzer = new ReadAttributePermissionLevelAnalyzer();
            Assert.NotNull(analyzer);
            Assert.Single(analyzer.SupportedDiagnostics);
            Assert.Equal("COA001", analyzer.SupportedDiagnostics[0].Id);
        }

        [Fact] 
        public async Task EditAnalyzerBuildsCorrectly()
        {
            var analyzer = new EditAttributePermissionLevelAnalyzer();
            Assert.NotNull(analyzer);
            Assert.Single(analyzer.SupportedDiagnostics);
            Assert.Equal("COA002", analyzer.SupportedDiagnostics[0].Id);
        }
    }
}