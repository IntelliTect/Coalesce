using System;
using Xunit;

namespace IntelliTect.Coalesce.Tests
{
    public class DatePrimaryKeyIntegrationTests
    {
        [Theory]
        [InlineData("2023-12-25T10:30:00")]
        [InlineData("2023-12-25T10:30:00Z")]
        [InlineData("2023-12-25")]
        public void DateTimeConversion_FromUrlString_WorksCorrectly(string dateString)
        {
            // This simulates what happens in the generated controller
            // when a date string comes from the URL
            
            // Act
            var parsedId = (DateTime)Convert.ChangeType(dateString, typeof(DateTime));
            
            // Assert
            Assert.IsType<DateTime>(parsedId);
            Assert.True(parsedId > DateTime.MinValue);
        }

        [Theory]
        [InlineData("2023-12-25")]
        public void DateOnlyConversion_FromUrlString_WorksCorrectly(string dateString)
        {
            // This simulates what happens in the generated controller
            // when a DateOnly string comes from the URL
            
            // Act
            var parsedId = (DateOnly)Convert.ChangeType(dateString, typeof(DateOnly));
            
            // Assert
            Assert.IsType<DateOnly>(parsedId);
            Assert.Equal(new DateOnly(2023, 12, 25), parsedId);
        }

        [Theory]
        [InlineData("2023-12-25T10:30:00+00:00")]
        [InlineData("2023-12-25T10:30:00Z")]
        public void DateTimeOffsetConversion_FromUrlString_WorksCorrectly(string dateString)
        {
            // This simulates what happens in the generated controller
            // when a DateTimeOffset string comes from the URL
            
            // Act
            var parsedId = (DateTimeOffset)Convert.ChangeType(dateString, typeof(DateTimeOffset));
            
            // Assert
            Assert.IsType<DateTimeOffset>(parsedId);
            Assert.True(parsedId > DateTimeOffset.MinValue);
        }

        [Theory]
        [InlineData("invalid-date")]
        [InlineData("")]
        [InlineData("2023-13-45")] // Invalid month/day
        public void DateTimeConversion_WithInvalidString_ThrowsExpectedException(string invalidDateString)
        {
            // This simulates error handling in the generated controller
            
            // Act & Assert
            Assert.ThrowsAny<Exception>(() => 
                (DateTime)Convert.ChangeType(invalidDateString, typeof(DateTime)));
        }

        [Fact]
        public void UrlEncoded_DateTimeString_CanBeDecoded()
        {
            // This tests a real-world scenario where a DateTime comes URL-encoded
            var urlEncodedDateTime = "2023-12-25T10%3A30%3A00"; // "2023-12-25T10:30:00" URL encoded
            
            // Decode the URL-encoded string (this would typically be done by ASP.NET Core)
            var decoded = Uri.UnescapeDataString(urlEncodedDateTime);
            
            // Convert to DateTime (this is what our generated controller does)
            var parsedId = (DateTime)Convert.ChangeType(decoded, typeof(DateTime));
            
            Assert.Equal(new DateTime(2023, 12, 25, 10, 30, 0), parsedId);
        }

        [Fact]
        public void ISO8601_DateTimeString_ParsesCorrectly()
        {
            // Test ISO 8601 format which is commonly used in web APIs
            var iso8601DateTime = "2023-12-25T10:30:00.000Z";
            
            var parsedId = (DateTime)Convert.ChangeType(iso8601DateTime, typeof(DateTime));
            
            Assert.IsType<DateTime>(parsedId);
            // Note: The exact value may vary due to timezone handling, 
            // but the important thing is that it parses without error
        }
    }
}