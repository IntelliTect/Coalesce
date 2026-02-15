using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Testing.TargetClasses;
using IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.TypeDefinition;
using System.Linq.Expressions;
using System.Security.Claims;

namespace IntelliTect.Coalesce.Tests.Api;

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

    [Test]
    [InstanceMethodDataSource(nameof(Search_DateTimeOffsetsData))]
    public async Task Search_DateTimeOffset_RespectsTimeZone(bool expectedMatch, string searchTerm, int utcOffset, DateTimeOffset searchCandidate)
    {
        await SearchHelper(
            (ComplexModel t) => t.DateTimeOffset,
            "datetimeoffset:" + searchTerm,
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

    [Test]
    [InstanceMethodDataSource(nameof(Search_MatchesDateTimesData))]
    public async Task Search_DateTime_IsTimeZoneAgnostic(bool expectedMatch, string searchTerm, DateTime searchCandidate)
    {
        await SearchHelper(
            (ComplexModel t) => t.DateTime,
            "datetime:" + searchTerm,
            searchCandidate,
            expectedMatch,
            TimeZoneInfo.CreateCustomTimeZone("test", TimeSpan.FromHours(new Random().Next(-11, 12)), "test", "test"));
    }

    public static IEnumerable<object[]> Search_MatchesDateOnlyData = new[]
    {
        new object[] { true, "2017", new DateOnly(2017, 08, 15) },
        new object[] { false, "2018", new DateOnly(2017, 08, 15) },
        new object[] { false, "2018", new DateOnly(2017, 12, 31) },
        new object[] { false, "2018", new DateOnly(2019, 1, 1) },
        new object[] { true, "2017-08", new DateOnly(2017, 08, 01) },
        new object[] { true, "2017-08", new DateOnly(2017, 08, 15) },
        new object[] { false, "2017-08", new DateOnly(2017, 07, 31) },
        new object[] { false, "2017-08", new DateOnly(2017, 09, 01) },
        new object[] { false, "2018-08", new DateOnly(2017, 08, 15) },

        new object[] { true, "Nov 6 17", new DateOnly(2017, 11, 06) },
        new object[] { true, "Nov 6, 2017", new DateOnly(2017, 11, 06) },
        new object[] { true, "6 November 2017", new DateOnly(2017, 11, 06) },
        new object[] { true, "6 Nov 2017", new DateOnly(2017, 11, 06) },
        new object[] { true, "6 Nov 17", new DateOnly(2017, 11, 06) },
    };

    [Test]
    [InstanceMethodDataSource(nameof(Search_MatchesDateOnlyData))]
    public async Task Search_DateOnly_SearchesCorrectly(bool expectedMatch, string searchTerm, DateOnly searchCandidate)
    {
        await SearchHelper(
            (ComplexModel t) => t.SystemDateOnly,
            "systemdateonly:" + searchTerm,
            searchCandidate,
            expectedMatch);
    }

    [Test]
    [Arguments(true, "FAFAB015-FFA4-41F8-B4DD-C15EB0CE40B6", "FAFAB015-FFA4-41F8-B4DD-C15EB0CE40B6")]
    [Arguments(true, "FAFAB015-FFA4-41F8-B4DD-C15EB0CE40B6", "fafab015-ffa4-41f8-b4dd-c15eb0ce40b6")]
    [Arguments(false, "FAFAB015-FFA4-41F8-B4DD-C15EB0CE40B6", "A6740FB5-99DE-4079-B6F7-A1692772A0A4")]
    public async Task Search_Guid_SearchesCorrectly(
        bool shouldMatch, string propValue, string inputValue)
    {
        await SearchHelper(
            (ComplexModel t) => t.Guid,
            "guid:" + inputValue,
            Guid.Parse(propValue),
            shouldMatch);
    }

    [Test]
    [Arguments(true, "mailto:test", "mailto:test")]
    [Arguments(true, "mailto:test", "mailto:TEST")]
    [Arguments(false, "mailto:test", "https://www.google.com/")]
    public async Task Search_Uri_SearchesCorrectly(
        bool shouldMatch, string propValue, string inputValue)
    {
        await SearchHelper(
            (ComplexModel t) => t.Uri,
            "uri:" + inputValue,
            new Uri(propValue),
            shouldMatch);
    }

    class DefaultString { public string Name { get; set; } }

    [Test]
    [Arguments(true, "Brisk Breeze", "Brisk Br")]
    [Arguments(true, "Brisk Breeze", "Brisk Bre")]
    [Arguments(false, "Brisk Breeze", "Brisk Brz")]
    public async Task Search_SingleDefaultString_SearchesCorrectly(
        bool shouldMatch, string propValue, string inputValue)
    {
        await SearchHelper(
            (DefaultString t) => t.Name,
            inputValue,
            propValue,
            shouldMatch);
    }

    class BeginsWith_SplitOnSpaces { [Search(SearchMethod = SearchAttribute.SearchMethods.BeginsWith, IsSplitOnSpaces = true)] public string Name { get; set; } }

    [Test]
    [Arguments(true, "John Smith", "John")]
    [Arguments(false, "John Smith", "Smith")] // BeginsWith with IsSplitOnSpaces=true: "Smith" doesn't match beginning of "John Smith"
    [Arguments(false, "John Smith", "John Smith")] // BeginsWith with IsSplitOnSpaces=true: "John Smith" split into ["John", "Smith"], "Smith" doesn't begin "John Smith"
    [Arguments(false, "John Smith", "Jo Sm")] // BeginsWith with IsSplitOnSpaces=true: "Jo" and "Sm" don't begin "John Smith"
    [Arguments(false, "John Smith", "ohn")]
    [Arguments(false, "John Smith", "mith")]
    [Arguments(false, "John Smith", "Jane")]
    public async Task Search_BeginsWith_SplitOnSpaces_SearchesCorrectly(
        bool shouldMatch, string propValue, string inputValue)
    {
        await SearchHelper(
            (BeginsWith_SplitOnSpaces t) => t.Name,
            inputValue,
            propValue,
            shouldMatch);
    }

    class BeginsWith_NoSplitOnSpaces { [Search(SearchMethod = SearchAttribute.SearchMethods.BeginsWith, IsSplitOnSpaces = false)] public string Name { get; set; } }

    [Test]
    [Arguments(true, "John Smith", "John")]
    [Arguments(true, "John Smith", "John Smith")]
    [Arguments(false, "John Smith", "Smith")]
    [Arguments(false, "John Smith", "Jo Sm")]
    [Arguments(false, "John Smith", "ohn")]
    public async Task Search_BeginsWith_NoSplitOnSpaces_SearchesCorrectly(
        bool shouldMatch, string propValue, string inputValue)
    {
        await SearchHelper(
            (BeginsWith_NoSplitOnSpaces t) => t.Name,
            inputValue,
            propValue,
            shouldMatch);
    }

    class Contains_SplitOnSpaces { [Search(SearchMethod = SearchAttribute.SearchMethods.Contains, IsSplitOnSpaces = true)] public string Name { get; set; } }

    [Test]
    [Arguments(true, "John Smith Jr", "John")]
    [Arguments(true, "John Smith Jr", "Smith")]
    [Arguments(true, "John Smith Jr", "Jr")]
    [Arguments(true, "John Smith Jr", "ohn")]
    [Arguments(true, "John Smith Jr", "mith")]
    [Arguments(true, "John Smith Jr", "Jo Sm")]
    [Arguments(true, "John Smith Jr", "ohn mith")]
    [Arguments(false, "John Smith Jr", "Jane")]
    [Arguments(false, "John Smith Jr", "Senior")]
    public async Task Search_Contains_SplitOnSpaces_SearchesCorrectly(
        bool shouldMatch, string propValue, string inputValue)
    {
        await SearchHelper(
            (Contains_SplitOnSpaces t) => t.Name,
            inputValue,
            propValue,
            shouldMatch);
    }

    class Contains_NoSplitOnSpaces { [Search(SearchMethod = SearchAttribute.SearchMethods.Contains, IsSplitOnSpaces = false)] public string Name { get; set; } }

    [Test]
    [Arguments(true, "John Smith Jr", "John")]
    [Arguments(true, "John Smith Jr", "Smith")]
    [Arguments(true, "John Smith Jr", "Jr")]
    [Arguments(true, "John Smith Jr", "ohn")]
    [Arguments(true, "John Smith Jr", "mith")]
    [Arguments(true, "John Smith Jr", "John Smith")]
    [Arguments(true, "John Smith Jr", "Smith Jr")]
    [Arguments(false, "John Smith Jr", "Jo Sm")]
    [Arguments(false, "John Smith Jr", "Jane")]
    public async Task Search_Contains_NoSplitOnSpaces_SearchesCorrectly(
        bool shouldMatch, string propValue, string inputValue)
    {
        await SearchHelper(
            (Contains_NoSplitOnSpaces t) => t.Name,
            inputValue,
            propValue,
            shouldMatch);
    }

    class Equals_SplitOnSpaces { [Search(SearchMethod = SearchAttribute.SearchMethods.Equals, IsSplitOnSpaces = true)] public string Name { get; set; } }

    [Test]
    [Arguments(true, "John", "John")]
    [Arguments(true, "JOHN", "john")]
    [Arguments(false, "John Smith", "John")]
    [Arguments(false, "John Smith", "Smith")]
    [Arguments(false, "John", "Jo")]
    public async Task Search_Equals_SplitOnSpaces_SearchesCorrectly(
        bool shouldMatch, string propValue, string inputValue)
    {
        await SearchHelper(
            (Equals_SplitOnSpaces t) => t.Name,
            inputValue,
            propValue,
            shouldMatch);
    }

    class Equals_NoSplitOnSpaces { [Search(SearchMethod = SearchAttribute.SearchMethods.Equals, IsSplitOnSpaces = false)] public string Name { get; set; } }

    [Test]
    [Arguments(true, "John Smith", "John Smith")]
    [Arguments(true, "JOHN SMITH", "john smith")]
    [Arguments(false, "John Smith", "John")]
    [Arguments(false, "John Smith", "Smith")]
    [Arguments(false, "John Smith", "john")]
    public async Task Search_Equals_NoSplitOnSpaces_SearchesCorrectly(
        bool shouldMatch, string propValue, string inputValue)
    {
        await SearchHelper(
            (Equals_NoSplitOnSpaces t) => t.Name,
            inputValue,
            propValue,
            shouldMatch);
    }

    class EqualsNatural_SplitOnSpaces { [Search(SearchMethod = SearchAttribute.SearchMethods.EqualsNatural, IsSplitOnSpaces = true)] public string Name { get; set; } }

    [Test]
    [Arguments(true, "John", "John")]
    [Arguments(false, "JOHN", "john")]
    [Arguments(false, "John Smith", "John")]
    [Arguments(false, "John", "Jo")]
    public async Task Search_EqualsNatural_SplitOnSpaces_SearchesCorrectly(
        bool shouldMatch, string propValue, string inputValue)
    {
        await SearchHelper(
            (EqualsNatural_SplitOnSpaces t) => t.Name,
            inputValue,
            propValue,
            shouldMatch);
    }

    class EqualsNatural_NoSplitOnSpaces { [Search(SearchMethod = SearchAttribute.SearchMethods.EqualsNatural, IsSplitOnSpaces = false)] public string Name { get; set; } }

    [Test]
    [Arguments(true, "John Smith", "John Smith")]
    [Arguments(false, "JOHN SMITH", "john smith")]
    [Arguments(false, "John Smith", "John")]
    [Arguments(false, "John Smith", "Smith")]
    public async Task Search_EqualsNatural_NoSplitOnSpaces_SearchesCorrectly(
        bool shouldMatch, string propValue, string inputValue)
    {
        await SearchHelper(
            (EqualsNatural_NoSplitOnSpaces t) => t.Name,
            inputValue,
            propValue,
            shouldMatch);
    }

    class MultipleProperties_SplitOnSpaces
    {
        [Search(SearchMethod = SearchAttribute.SearchMethods.BeginsWith, IsSplitOnSpaces = true)]
        public string FirstName { get; set; }

        [Search(SearchMethod = SearchAttribute.SearchMethods.BeginsWith, IsSplitOnSpaces = true)]
        public string LastName { get; set; }

        [Search(SearchMethod = SearchAttribute.SearchMethods.Contains, IsSplitOnSpaces = true)]
        public string Email { get; set; }
    }

    [Test]
    [Arguments(true, "John", "Smith", "john@example.com", "John")]
    [Arguments(true, "John", "Smith", "john@example.com", "Smith")]
    [Arguments(true, "John", "Smith", "john@example.com", "example")]  // Email contains "example"
    [Arguments(true, "John", "Smith", "john@example.com", "John Smith")] // "John" matches FirstName, "Smith" matches LastName
    [Arguments(true, "John", "Smith", "john@example.com", "John example")] // "John" matches FirstName, "example" matches Email
    [Arguments(true, "John", "Smith", "john@example.com", "Smith example")] // "Smith" matches LastName, "example" matches Email
    [Arguments(false, "John", "Smith", "john@example.com", "Jane")] // No property matches "Jane"
    [Arguments(true, "John", "Smith", "john@example.com", "ohn")]  // Email contains "ohn" in "john@example.com"
    [Arguments(false, "John", "Smith", "john@example.com", "mith")] // FirstName/LastName use BeginsWith and don't begin with "mith", Email doesn't contain "mith"
    public async Task Search_MultipleProperties_SplitOnSpaces_SearchesCorrectly(
        bool shouldMatch, string firstName, string lastName, string email, string inputValue)
    {
        await SearchHelper<MultipleProperties_SplitOnSpaces>(
            inputValue,
            model =>
            {
                model.FirstName = firstName;
                model.LastName = lastName;
                model.Email = email;
            },
            shouldMatch);
    }

    class MixedSplitOnSpaces
    {
        [Search(SearchMethod = SearchAttribute.SearchMethods.BeginsWith, IsSplitOnSpaces = true)]
        public string Name { get; set; }

        [Search(SearchMethod = SearchAttribute.SearchMethods.Contains, IsSplitOnSpaces = false)]
        public string Description { get; set; }
    }

    [Test]
    [Arguments(true, "John Doe", "Software Engineer", "John")] // "John" begins "John Doe"
    [Arguments(false, "John Doe", "Software Engineer", "Doe")] // "Doe" doesn't begin "John Doe", and Description doesn't contain "Doe" 
    [Arguments(true, "John Doe", "Software Engineer", "Software")] // Description contains "Software"
    [Arguments(true, "John Doe", "Software Engineer", "Engineer")] // Description contains "Engineer"
    [Arguments(true, "John Doe", "Software Engineer", "Software Engineer")] // Description contains "Software Engineer"
    [Arguments(true, "John Doe", "Software Engineer", "oftware")] // Description contains "oftware" (part of "Software")
    [Arguments(true, "John Doe", "Software Engineer", "neer")]    // Description contains "neer" (part of "Engineer")
    [Arguments(false, "John Doe", "Software Engineer", "John Engineer")] // "John" begins Name but "Engineer" doesn't begin Name, Description contains "Engineer" but not "John"
    public async Task Search_MixedSplitOnSpaces_SearchesCorrectly(
        bool shouldMatch, string name, string description, string inputValue)
    {
        await SearchHelper<MixedSplitOnSpaces>(
            inputValue,
            model =>
            {
                model.Name = name;
                model.Description = description;
            },
            shouldMatch);
    }

    [Test]
    [Arguments(true, "a1", "a1")]
    [Arguments(true, "A1", "a1")]
    [Arguments(true, "a1", "A1")]
    [Arguments(false, "a1b", "a1")]
    public async Task Search_StringEqualsInsensitive_SearchesCorrectly(
        bool shouldMatch, string propValue, string inputValue)
    {
        await SearchHelper(
            (ComplexModel t) => t.StringSearchedEqualsInsensitive,
            "StringSearchedEqualsInsensitive:" + inputValue,
            propValue,
            shouldMatch);
    }

    [Test]
    [Arguments(true, "a1", "a1")]
    [Arguments(false, "A1", "a1")]
    [Arguments(false, "a1", "A1")]
    [Arguments(false, "a1b", "a1")]
    public async Task Search_StringEqualsNatural_SearchesCorrectly(
        bool shouldMatch, string propValue, string inputValue)
    {
        // Note about this test: the above tests cases should fail when
        // casing matches because all these tests evaluate in memory.
        await SearchHelper(
            (ComplexModel t) => t.StringSearchedEqualsNatural,
            "StringSearchedEqualsNatural:" + inputValue,
            propValue,
            shouldMatch);
    }

    [Test]
    [Arguments(true, 0, "0")]
    [Arguments(true, int.MaxValue, "2147483647")]
    [Arguments(false, int.MaxValue, "2147483648")]
    [Arguments(true, 2, "2")]
    [Arguments(true, 22, "22")]
    [Arguments(false, 3, "2")]
    [Arguments(false, 2, "22")]
    [Arguments(false, 22, "2")]
    public async Task Search_Int_SearchesCorrectly(
        bool shouldMatch, int propValue, string inputValue)
    {
        await SearchHelper(
            (ComplexModel t) => t.Int,
            "Int:" + inputValue,
            propValue,
            shouldMatch);
    }

    [Test]
    [Arguments(true, "a1", "a1")]
    [Arguments(false, "B1", "a1")]
    public async Task Search_Collection_SearchesCorrectly(
        bool shouldMatch, string propValue, string inputValue)
    {
        await SearchHelper(
            (ComplexModel t) => t.Tests,
            inputValue,
            new[] { new Testing.TargetClasses.Test { TestName = propValue } },
            shouldMatch);
    }

    [Test]
    [Arguments(true, "needle", "needlestack", "stackneedle", "foo")]
    [Arguments(true, "needle stack", "needlestack", "stackneedle", "foo")]
    [Arguments(true, "foo stack needle", "needlestack", "stackneedle", "foo")]
    [Arguments(false, "needle stack bar", "needlestack", "stackneedle", "foo")]
    public async Task Search_ComplexCollection_SearchesCorrectly(
        bool shouldMatch, string query, string field1, string field2, string field3)
    {
        await SearchHelper(
            (HasCollection t) => t.Children,
            query,
            [new Collected { Field1 = field1, Field2 = field2, Field3 = field3 }],
            shouldMatch);
    }

    public class HasCollection
    {
        [Search]
        public List<Collected> Children { get; set; }
    }
    public class Collected
    {
        [Search(IsSplitOnSpaces = true)]
        public string Field1 { get; set; }

        [Search(IsSplitOnSpaces = true)]
        public string Field2 { get; set; }

        [Search(IsSplitOnSpaces = true)]
        public string Field3 { get; set; }
    }

    [Test]
    public async Task Search_CollectionWithNoSearchableChildren_DoesNotThrow()
    {
        // This test demonstrates the issue where searching on a collection 
        // with no searchable child properties should not throw an exception.
        // The search should simply return no matches.

        await SearchHelper(
            (HasCollectionNoSearchable t) => t.Children,
            "test query",
            [new CollectedNoSearchable { NonSearchableField = "some value" }],
            false); // Expect no match since there are no searchable fields
    }

    public class HasCollectionNoSearchable
    {
        [Search]
        public List<CollectedNoSearchable> Children { get; set; }
    }

    public class CollectedNoSearchable
    {
        // No [Search] attribute - this property is not searchable
        public string NonSearchableField { get; set; }
    }

    public class SearchTestDataSource<T>(CrudContext context) : QueryableDataSourceBase<T>(context)
        where T : class
    {
    }

    private async Task SearchHelper<T, TProp>(
        Expression<Func<T, TProp>> propSelector,
        string searchTerm,
        TProp searchCandidate,
        bool expectedMatch,
        TimeZoneInfo timeZoneInfo = null
    )
        where T : class, new()
    {
        var classViewModel = new ReflectionClassViewModel(typeof(T));
        var prop = classViewModel.PropertyBySelector(propSelector);
        var context = new CrudContext(() => new ClaimsPrincipal(), timeZoneInfo ?? TimeZoneInfo.Local);

        T model = new T();
        prop.PropertyInfo.SetValue(model, searchCandidate);

        var ds = new SearchTestDataSource<T>(context);
        var query = new List<T> { model }.AsQueryable();
        query = ds.ApplyListSearchTerm(query, new FilterParameters { Search = searchTerm });

        var matchedItems = query.ToArray();

        if (expectedMatch)
            await Assert.That(matchedItems.Length == 1).IsTrue().Because($"{searchTerm} didn't match {searchCandidate}.");
        else
            await Assert.That(matchedItems.Length == 1).IsFalse().Because($"{searchTerm} matched on {searchCandidate}, but shouldn't have.");
    }

    private async Task SearchHelper<T>(
        string searchTerm,
        Action<T> configureModel,
        bool expectedMatch,
        TimeZoneInfo timeZoneInfo = null
    )
        where T : class, new()
    {
        var context = new CrudContext(() => new ClaimsPrincipal(), timeZoneInfo ?? TimeZoneInfo.Local);

        T model = new T();
        configureModel(model);

        var ds = new SearchTestDataSource<T>(context);
        var query = new List<T> { model }.AsQueryable();
        query = ds.ApplyListSearchTerm(query, new FilterParameters { Search = searchTerm });

        var matchedItems = query.ToArray();

        if (expectedMatch)
            await Assert.That(matchedItems.Length == 1).IsTrue().Because($"{searchTerm} didn't match the configured model.");
        else
            await Assert.That(matchedItems.Length == 1).IsFalse().Because($"{searchTerm} matched on the configured model, but shouldn't have.");
    }
}
