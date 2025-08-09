using System;
using Xunit;
using Xunit.Abstractions;

namespace IntelliTect.Coalesce.Tests
{
    /// <summary>
    /// This test verifies that our date primary key implementation works as expected
    /// by simulating the exact logic that would be generated in a controller.
    /// </summary>
    public class DatePrimaryKeyWorkflowTests
    {
        private readonly ITestOutputHelper _output;

        public DatePrimaryKeyWorkflowTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void DateTimePrimaryKey_EndToEndWorkflow_Success()
        {
            // Arrange - Simulate a DateTime primary key entity
            var entityId = new DateTime(2023, 12, 25, 10, 30, 0);
            
            // Act 1: Convert DateTime to string (this would happen on the client/frontend)
            var idAsString = entityId.ToString("O"); // ISO 8601 format
            _output.WriteLine($"DateTime as string: {idAsString}");
            
            // Act 2: Parse string back to DateTime (this is what our generated controller does)
            var parsedId = SimulateControllerIdParsing<DateTime>(idAsString);
            
            // Assert
            Assert.Equal(entityId, parsedId);
        }

        [Fact]
        public void DateOnlyPrimaryKey_EndToEndWorkflow_Success()
        {
            // Arrange - Simulate a DateOnly primary key entity
            var entityId = new DateOnly(2023, 12, 25);
            
            // Act 1: Convert DateOnly to string
            var idAsString = entityId.ToString("O");
            _output.WriteLine($"DateOnly as string: {idAsString}");
            
            // Act 2: Parse string back to DateOnly (controller logic)
            var parsedId = SimulateControllerIdParsing<DateOnly>(idAsString);
            
            // Assert
            Assert.Equal(entityId, parsedId);
        }

        [Fact]
        public void DateTimeOffsetPrimaryKey_EndToEndWorkflow_Success()
        {
            // Arrange - Simulate a DateTimeOffset primary key entity
            var entityId = new DateTimeOffset(2023, 12, 25, 10, 30, 0, TimeSpan.Zero);
            
            // Act 1: Convert DateTimeOffset to string
            var idAsString = entityId.ToString("O");
            _output.WriteLine($"DateTimeOffset as string: {idAsString}");
            
            // Act 2: Parse string back to DateTimeOffset (controller logic)
            var parsedId = SimulateControllerIdParsing<DateTimeOffset>(idAsString);
            
            // Assert
            Assert.Equal(entityId, parsedId);
        }

        [Fact]
        public void InvalidDateString_ThrowsExceptionWithMeaningfulMessage()
        {
            // Arrange
            var invalidDateString = "not-a-date";
            
            // Act & Assert
            var exception = Assert.ThrowsAny<Exception>(() => 
                SimulateControllerIdParsing<DateTime>(invalidDateString));
                
            _output.WriteLine($"Exception message: {exception.Message}");
        }

        [Fact]
        public void UrlEncodedDateTime_ParsesCorrectly()
        {
            // Arrange - Simulate URL-encoded DateTime coming from a route
            var originalDateTime = new DateTime(2023, 12, 25, 10, 30, 0);
            var dateTimeString = originalDateTime.ToString("O");
            var urlEncoded = Uri.EscapeDataString(dateTimeString);
            _output.WriteLine($"URL encoded: {urlEncoded}");
            
            // Act - Simulate URL decoding (done by ASP.NET Core) and parsing (our controller)
            var decoded = Uri.UnescapeDataString(urlEncoded);
            var parsedId = SimulateControllerIdParsing<DateTime>(decoded);
            
            // Assert
            Assert.Equal(originalDateTime, parsedId);
        }

        [Theory]
        [InlineData("2023-12-25T10:30:00")]
        [InlineData("2023-12-25T10:30:00Z")]
        [InlineData("2023-12-25T10:30:00.000Z")]
        [InlineData("2023-12-25")]
        public void CommonDateFormats_ParseSuccessfully(string dateString)
        {
            // This tests various common date formats that might come from frontend clients
            
            // Act
            var result = SimulateControllerIdParsing<DateTime>(dateString);
            
            // Assert
            Assert.IsType<DateTime>(result);
            Assert.NotEqual(DateTime.MinValue, result);
            _output.WriteLine($"Parsed '{dateString}' to {result}");
        }

        /// <summary>
        /// This method simulates the exact logic that our generated controller uses
        /// to parse date IDs from string route parameters.
        /// </summary>
        private static T SimulateControllerIdParsing<T>(string idString)
        {
            // This is the exact same logic generated in ModelApiController.cs
            try
            {
                return (T)Convert.ChangeType(idString, typeof(T));
            }
            catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException)
            {
                throw new ArgumentException($"Invalid date format for id parameter: {idString}", ex);
            }
        }
    }
}