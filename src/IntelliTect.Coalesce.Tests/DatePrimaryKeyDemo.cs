using System;

namespace IntelliTect.Coalesce.Tests
{
    /// <summary>
    /// Simple demonstration of the date primary key functionality
    /// </summary>
    public class DatePrimaryKeyDemo
    {
        public static void RunDemo()
        {
            Console.WriteLine("=== Date Primary Key Support Demo ===");
            Console.WriteLine();

            // Test DateTime primary key
            TestDateTimePrimaryKey();
            Console.WriteLine();

            // Test DateOnly primary key  
            TestDateOnlyPrimaryKey();
            Console.WriteLine();

            // Test DateTimeOffset primary key
            TestDateTimeOffsetPrimaryKey();
            Console.WriteLine();

            // Test error handling
            TestErrorHandling();
            Console.WriteLine();

            Console.WriteLine("=== Demo Complete ===");
        }

        private static void TestDateTimePrimaryKey()
        {
            Console.WriteLine("Testing DateTime Primary Key:");
            
            var originalDateTime = new DateTime(2023, 12, 25, 10, 30, 0);
            Console.WriteLine($"  Original DateTime: {originalDateTime}");
            
            // Convert to string (frontend to API)
            var dateString = originalDateTime.ToString("O");
            Console.WriteLine($"  As string (ISO 8601): {dateString}");
            
            // Parse back (generated controller logic)
            var parsed = (DateTime)Convert.ChangeType(dateString, typeof(DateTime));
            Console.WriteLine($"  Parsed back: {parsed}");
            
            Console.WriteLine($"  Round-trip successful: {originalDateTime == parsed}");
        }

        private static void TestDateOnlyPrimaryKey()
        {
            Console.WriteLine("Testing DateOnly Primary Key:");
            
            var originalDate = new DateOnly(2023, 12, 25);
            Console.WriteLine($"  Original DateOnly: {originalDate}");
            
            // Convert to string
            var dateString = originalDate.ToString("O");
            Console.WriteLine($"  As string: {dateString}");
            
            // Parse back
            var parsed = (DateOnly)Convert.ChangeType(dateString, typeof(DateOnly));
            Console.WriteLine($"  Parsed back: {parsed}");
            
            Console.WriteLine($"  Round-trip successful: {originalDate == parsed}");
        }

        private static void TestDateTimeOffsetPrimaryKey()
        {
            Console.WriteLine("Testing DateTimeOffset Primary Key:");
            
            var originalDateTimeOffset = new DateTimeOffset(2023, 12, 25, 10, 30, 0, TimeSpan.Zero);
            Console.WriteLine($"  Original DateTimeOffset: {originalDateTimeOffset}");
            
            // Convert to string
            var dateString = originalDateTimeOffset.ToString("O");
            Console.WriteLine($"  As string: {dateString}");
            
            // Parse back
            var parsed = (DateTimeOffset)Convert.ChangeType(dateString, typeof(DateTimeOffset));
            Console.WriteLine($"  Parsed back: {parsed}");
            
            Console.WriteLine($"  Round-trip successful: {originalDateTimeOffset == parsed}");
        }

        private static void TestErrorHandling()
        {
            Console.WriteLine("Testing Error Handling:");
            
            var invalidDateString = "not-a-valid-date";
            Console.WriteLine($"  Invalid date string: '{invalidDateString}'");
            
            try
            {
                var parsed = (DateTime)Convert.ChangeType(invalidDateString, typeof(DateTime));
                Console.WriteLine("  ERROR: Should have thrown an exception!");
            }
            catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException)
            {
                Console.WriteLine($"  Correctly caught exception: {ex.GetType().Name}");
                Console.WriteLine($"  Error message: {ex.Message}");
            }
        }
    }
}