using IntelliTect.Coalesce.Helpers.Search;
using IntelliTect.Coalesce.Tests.TypeDefinition.TargetClasses;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace IntelliTect.Coalesce.Tests
{
    public class SearchTests
    {
        public static IEnumerable<object[]> Search_MatchesDates_ForTimeZone_Data = new[]
        {
            // Same timezone.
            new object[] { true, "2017-08", -8, new DateTimeOffset(2017, 08, 01, 0, 0, 0, TimeSpan.FromHours(-8)) },
            // For a person at UTC-8, a time that was at midnight in UTC-9 was 1:00 AM to them, so this matches.
            new object[] { true, "2017-08", -8, new DateTimeOffset(2017, 08, 01, 0, 0, 0, TimeSpan.FromHours(-9)) },
            // For a person at UTC-8, a time that was at midnight in UTC-7 was at 11:00 PM the previous day. Shouldnt match.
            new object[] { false, "2017-08", -8, new DateTimeOffset(2017, 08, 01, 0, 0, 0, TimeSpan.FromHours(-7)) },

            // Just explaining the first few - not going to explain the remainder in great detail

            new object[] { true, "2017-08", 0, new DateTimeOffset(2017, 08, 01, 0, 0, 0, TimeSpan.FromHours(0)) },
            new object[] { true, "2017-08", 0, new DateTimeOffset(2017, 08, 01, 0, 0, 0, TimeSpan.FromHours(-8)) },
            new object[] { false, "2017-08", -8, new DateTimeOffset(2017, 08, 01, 0, 0, 0, TimeSpan.FromHours(0)) },

            new object[] { true, "Nov 6 17 11:35 AM", -8, new DateTimeOffset(2017, 11, 06, 11, 35, 0, TimeSpan.FromHours(-8)) },
            new object[] { true, "Nov 6, 2017 11:35 AM", -8, new DateTimeOffset(2017, 11, 06, 11, 35, 59, TimeSpan.FromHours(-8)) },
            new object[] { false, "November 6, 2017 11:35 AM", -8, new DateTimeOffset(2017, 11, 06, 11, 36, 00, TimeSpan.FromHours(-8)) },
            new object[] { false, "November 6, 2017 11:35 AM", -8, new DateTimeOffset(2017, 11, 06, 11, 34, 59, TimeSpan.FromHours(-8)) },
            
            // Search term formats doesn't matter as long as it represents the same time and is parsable.
            // Only trying weird formats on the positive tests, though. Don't want false negatives.

            new object[] { true, "6 November 2017 11:00 AM", -8, new DateTimeOffset(2017, 11, 06, 11, 00, 0, TimeSpan.FromHours(-8)) },
            new object[] { true, "6 Nov 2017 11:00 AM", -8, new DateTimeOffset(2017, 11, 06, 11, 30, 14, TimeSpan.FromHours(-8)) },
            new object[] { true, "6 Nov 17 11:00 AM", -8, new DateTimeOffset(2017, 11, 06, 11, 59, 59, TimeSpan.FromHours(-8)) },
            new object[] { false, "November 6, 2017 11:00 AM", -8, new DateTimeOffset(2017, 11, 06, 12, 00, 00, TimeSpan.FromHours(-8)) },
            new object[] { false, "November 6, 2017 11:00 AM", -8, new DateTimeOffset(2017, 11, 06, 10, 59, 59, TimeSpan.FromHours(-8)) },
            
            new object[] { false, "November 6, 2017 11:00 AM", -6, new DateTimeOffset(2017, 11, 06, 9, 00, 0, TimeSpan.FromHours(-7)) },
            new object[] { false, "November 6, 2017 11:00 AM", -6, new DateTimeOffset(2017, 11, 06, 9, 30, 14, TimeSpan.FromHours(-9)) },
            new object[] { false, "November 6, 2017 11:00", -6, new DateTimeOffset(2017, 11, 06, 9, 59, 59, TimeSpan.FromHours(-6)) },
            new object[] { true, "11-06-17 11 AM", -6, new DateTimeOffset(2017, 11, 06, 10, 00, 00, TimeSpan.FromHours(-7)) },
            new object[] { true, "2017-11-06 11:00 AM", -6, new DateTimeOffset(2017, 11, 06, 8, 59, 59, TimeSpan.FromHours(-9)) },
        };

        [Theory]
        [MemberData(nameof(Search_MatchesDates_ForTimeZone_Data))]
        public void Search_MatchesDates_ForTimeZone(bool expectedMatch, string searchTerm, int utcOffset, DateTimeOffset searchCandidate)
        {
            var classViewModel = new ReflectionClassViewModel(typeof(ComplexModel));

            var searchClauses = classViewModel
                .SearchProperties(classViewModel.Name)
                .SelectMany(p => p.GetLinqDynamicSearchStatements(
                    new ClaimsPrincipal(),
                    TimeZoneInfo.CreateCustomTimeZone("test", TimeSpan.FromHours(utcOffset), "test", "test"),
                    "it",
                    searchTerm
                ))
                .Select(t => t.statement)
                .ToList();

            var matchedItems = new[] { new ComplexModel { DateTimeOffset = searchCandidate } }
                .AsQueryable()
                .Where(string.Join(" || ", searchClauses))
                .ToList();

            if (expectedMatch)
                Assert.True(matchedItems.Count == 1, $"{searchTerm} didn't match {searchCandidate}.");
            else
                Assert.False(matchedItems.Count == 1, $"{searchTerm} matched on {searchCandidate}, but shouldn't have.");
        }
    }
}
