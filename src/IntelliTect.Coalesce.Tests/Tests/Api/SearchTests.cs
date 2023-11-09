using IntelliTect.Coalesce.Helpers.Search;
using IntelliTect.Coalesce.Tests.TargetClasses;
using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace IntelliTect.Coalesce.Tests.Api
{
    public class SearchTests
    {
        public static IEnumerable<object[]> Search_DateTimeOffsetsData = new[]
        {
            // Year only
            new object[] { true, "2017", -8, new DateTimeOffset(2017, 08, 01, 0, 0, 0, TimeSpan.FromHours(-8)) },
            new object[] { false, "2018", -8, new DateTimeOffset(2017, 08, 01, 0, 0, 0, TimeSpan.FromHours(-8)) },
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
        [MemberData(nameof(Search_DateTimeOffsetsData))]
        public void Search_DateTimeOffset_RespectsTimeZone(bool expectedMatch, string searchTerm, int utcOffset, DateTimeOffset searchCandidate)
        {
            SearchHelper(
                (ComplexModel t) => t.DateTimeOffset,
                searchTerm,
                searchCandidate,
                expectedMatch,
                TimeZoneInfo.CreateCustomTimeZone("test", TimeSpan.FromHours(utcOffset), "test", "test"));
        }


        public static IEnumerable<object[]> Search_MatchesDateTimesData = new[]
        {
            new object[] { true, "2017", new DateTime(2017, 08, 15, 0, 0, 0) },
            new object[] { false, "2018", new DateTime(2017, 08, 15, 0, 0, 0) },
            new object[] { true, "2017-08", new DateTime(2017, 08, 01, 0, 0, 0) },
            new object[] { true, "2017-08", new DateTime(2017, 08, 15, 0, 0, 0) },
            new object[] { false, "2017-08", new DateTime(2017, 07, 31, 23, 59, 59) },
            new object[] { false, "2017-08", new DateTime(2017, 09, 01, 0, 0, 0) },
            new object[] { false, "2018-08", new DateTime(2017, 08, 15, 0, 0, 0) },

            new object[] { true, "Nov 6 17 11:35 AM", new DateTime(2017, 11, 06, 11, 35, 0) },
            new object[] { true, "Nov 6, 2017 11:35 AM", new DateTime(2017, 11, 06, 11, 35, 59) },
            new object[] { false, "November 6, 2017 11:35 AM", new DateTime(2017, 11, 06, 11, 36, 00) },
            new object[] { false, "November 6, 2017 11:35 AM", new DateTime(2017, 11, 06, 11, 34, 59) },

            new object[] { true, "6 November 2017 11:00 AM", new DateTime(2017, 11, 06, 11, 00, 0) },
            new object[] { true, "6 Nov 2017 11:00 AM", new DateTime(2017, 11, 06, 11, 30, 14) },
            new object[] { true, "6 Nov 17 11:00 AM", new DateTime(2017, 11, 06, 11, 59, 59) },
            new object[] { false, "November 6, 2017 11:00 AM", new DateTime(2017, 11, 06, 12, 00, 00) },
            new object[] { false, "November 6, 2017 11:00 AM", new DateTime(2017, 11, 06, 10, 59, 59) },
        };

        [Theory]
        [MemberData(nameof(Search_MatchesDateTimesData))]
        public void Search_DateTime_IsTimeZoneAgnostic(bool expectedMatch, string searchTerm, DateTime searchCandidate)
        {
            SearchHelper(
                (ComplexModel t) => t.DateTime,
                searchTerm,
                searchCandidate,
                expectedMatch,
                TimeZoneInfo.CreateCustomTimeZone("test", TimeSpan.FromHours(new Random().Next(-11, 12)), "test", "test"));
        }

        [Theory]
        [InlineData(true, "FAFAB015-FFA4-41F8-B4DD-C15EB0CE40B6", "FAFAB015-FFA4-41F8-B4DD-C15EB0CE40B6")]
        [InlineData(true, "FAFAB015-FFA4-41F8-B4DD-C15EB0CE40B6", "fafab015-ffa4-41f8-b4dd-c15eb0ce40b6")]
        [InlineData(false, "FAFAB015-FFA4-41F8-B4DD-C15EB0CE40B6", "A6740FB5-99DE-4079-B6F7-A1692772A0A4")]
        public void Search_Guid_SearchesCorrectly(
            bool shouldMatch, string propValue, string inputValue)
        {
            SearchHelper(
                (ComplexModel t) => t.Guid,
                inputValue,
                Guid.Parse(propValue),
                shouldMatch);
        }

        [Theory]
        [InlineData(true, "a1", "a1")]
        [InlineData(true, "A1", "a1")]
        [InlineData(true, "a1", "A1")]
        [InlineData(false, "a1b", "a1")]
        public void Search_StringEqualsInsensitive_SearchesCorrectly(
            bool shouldMatch, string propValue, string inputValue)
        {
            SearchHelper(
                (ComplexModel t) => t.StringSearchedEqualsInsensitive,
                inputValue,
                propValue,
                shouldMatch);
        }

        [Theory]
        [InlineData(true, "a1", "a1")]
        [InlineData(false, "A1", "a1")]
        [InlineData(false, "a1", "A1")]
        [InlineData(false, "a1b", "a1")]
        public void Search_StringEqualsNatural_SearchesCorrectly(
            bool shouldMatch, string propValue, string inputValue)
        {
            // Note about this test: the above tests cases should fail when
            // casing matches because all these tests evaluate in memory.
            SearchHelper(
                (ComplexModel t) => t.StringSearchedEqualsNatural,
                inputValue,
                propValue,
                shouldMatch);
        }

        [Theory]
        [InlineData(true, 0, "0")]
        [InlineData(true, int.MaxValue, "2147483647")]
        [InlineData(false, int.MaxValue, "2147483648")]
        [InlineData(true, 2, "2")]
        [InlineData(true, 22, "22")]
        [InlineData(false, 3, "2")]
        [InlineData(false, 2, "22")]
        [InlineData(false, 22, "2")]
        public void Search_Int_SearchesCorrectly(
            bool shouldMatch, int propValue, string inputValue)
        {
            SearchHelper(
                (ComplexModel t) => t.Int,
                inputValue,
                propValue,
                shouldMatch);
        }

        [Theory]
        [InlineData(true, "a1", "a1")]
        [InlineData(false, "B1", "a1")]
        public void Search_Collection_SearchesCorrectly(
            bool shouldMatch, string propValue, string inputValue)
        {
            SearchHelper(
                (ComplexModel t) => t.Tests,
                inputValue,
                new[] {new Test { TestName = propValue } },
                shouldMatch);
        }


        private void SearchHelper<T, TProp>(
            Expression<Func<T, TProp>> propSelector,
            string searchTerm,
            TProp searchCandidate,
            bool expectedMatch,
            TimeZoneInfo timeZoneInfo = null
        )
            where T : new()
        {
            var classViewModel = new ReflectionClassViewModel(typeof(T));
            var prop = classViewModel.PropertyBySelector(propSelector);
            var context = new CrudContext(() => new ClaimsPrincipal(), timeZoneInfo ?? TimeZoneInfo.Local);

            var param = Expression.Parameter(typeof(T));
            var searchClauses = prop
                .SearchProperties(classViewModel, maxDepth: 2, force: true)
                .SelectMany(p => p.GetLinqSearchStatements(
                    context,
                    param,
                    searchTerm
                ))
                .Select(t => t.statement)
                .ToList();

            T model = new T();
            prop.PropertyInfo.SetValue(model, searchCandidate);

            var matchedItems = searchClauses.Any()
                ? new[] { model }
                    .AsQueryable()
                    .Where(Expression.Lambda<Func<T, bool>>(searchClauses.OrAny(), param))
                    .ToArray()
                : new T[0];

            if (expectedMatch)
                Assert.True(matchedItems.Length == 1, $"{searchTerm} didn't match {searchCandidate}.");
            else
                Assert.False(matchedItems.Length == 1, $"{searchTerm} matched on {searchCandidate}, but shouldn't have.");
        }
    }
}
