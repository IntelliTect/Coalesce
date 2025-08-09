using System;
using Xunit;

namespace IntelliTect.Coalesce.Tests
{
    public class DateConversionTests
    {
        [Theory]
        [InlineData("2023-12-25T10:30:00", "2023-12-25T10:30:00")]
        [InlineData("2023-12-25T10:30:00Z", "2023-12-25T10:30:00Z")]
        [InlineData("2023-12-25", "2023-12-25T00:00:00")]
        public void ConvertChangeType_ParsesDateTimeFromString(string input, string expectedOutput)
        {
            // Act
            var result = Convert.ChangeType(input, typeof(DateTime));
            
            // Assert
            var expectedDateTime = DateTime.Parse(expectedOutput);
            Assert.Equal(expectedDateTime, result);
        }

        [Theory]
        [InlineData("2023-12-25", "2023-12-25")]
        public void ConvertChangeType_ParsesDateOnlyFromString(string input, string expectedOutput)
        {
            // Act
            var result = Convert.ChangeType(input, typeof(DateOnly));
            
            // Assert
            var expectedDate = DateOnly.Parse(expectedOutput);
            Assert.Equal(expectedDate, result);
        }

        [Theory]
        [InlineData("2023-12-25T10:30:00+00:00", "2023-12-25T10:30:00+00:00")]
        [InlineData("2023-12-25T10:30:00Z", "2023-12-25T10:30:00+00:00")]
        public void ConvertChangeType_ParsesDateTimeOffsetFromString(string input, string expectedOutput)
        {
            // Act
            var result = Convert.ChangeType(input, typeof(DateTimeOffset));
            
            // Assert
            var expectedDateTimeOffset = DateTimeOffset.Parse(expectedOutput);
            Assert.Equal(expectedDateTimeOffset, result);
        }

        [Fact]
        public void UrlEncoded_DateTimeStrings_CanBeParsed()
        {
            // These are URL-encoded date strings that might come from route parameters
            var urlEncodedDateTime = "2023-12-25T10%3A30%3A00"; // "2023-12-25T10:30:00" URL encoded
            var decoded = Uri.UnescapeDataString(urlEncodedDateTime);
            
            var result = Convert.ChangeType(decoded, typeof(DateTime));
            var expected = new DateTime(2023, 12, 25, 10, 30, 0);
            
            Assert.Equal(expected, result);
        }
    }
}