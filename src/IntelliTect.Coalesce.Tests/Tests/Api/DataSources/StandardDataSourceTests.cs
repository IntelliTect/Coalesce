using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Testing.Fixtures;
using IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Testing.Util;
using IntelliTect.Coalesce.TypeDefinition;
using System.Linq.Expressions;
using System.Security.Claims;

namespace IntelliTect.Coalesce.Tests.Api.DataSources;


public class StandardDataSourceTests : TestDbContextFixture
{
    private StandardDataSource<T, AppDbContext> Source<T>()
        where T : class, new()
        => new StandardDataSource<T, AppDbContext>(CrudContext);

    [Test]
    [Arguments("none")]
    [Arguments("NONE")]
    public async Task GetQuery_WhenIncludesStringNone_DoesNotIncludeChildren(string includes)
    {
        var query = Source<Case>().GetQuery(new DataSourceParameters { Includes = includes });
        await Assert.That(query.GetIncludeTree()).IsEmpty();
    }

    [Test]
    [Arguments("none")]
    [Arguments("NONE")]
    public async Task GetQueryAsync_WhenIncludesStringNone_DoesNotIncludeChildren(string includes)
    {
        var query = await Source<Case>().GetQueryAsync(new DataSourceParameters { Includes = includes });
        await Assert.That(query.GetIncludeTree()).IsEmpty();
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("CaseListGen")]
    public async Task GetQuery_WhenIncludesStringNotNone_IncludesChildren(string includes)
    {
        var query = Source<Case>().GetQuery(new DataSourceParameters { Includes = includes });
        var tree = query.GetIncludeTree();
        await Assert.That(tree[nameof(Case.AssignedTo)]).IsNotNull();
        await Assert.That(tree[nameof(Case.ReportedBy)]).IsNotNull();
        await Assert.That(tree[nameof(Case.CaseProducts)][nameof(CaseProduct.Product)]).IsNotNull();
    }


    private (PropertyViewModel, IQueryable<TModel>) PropertyFiltersTestHelper<TModel, TProp>(
        Expression<Func<TModel, TProp>> propSelector,
        TProp propValue,
        string filterValue
    )
        where TModel : class, new()
    {
        var filterParams = new FilterParameters();
        var source = Source<TModel>();
        var propInfo = source.ClassViewModel.PropertyBySelector(propSelector);
        var model = new TModel();
        propInfo.PropertyInfo.SetValue(model, propValue);

        filterParams.Filter[propInfo.JsonName] = filterValue;
        Db.Set<TModel>().Add(model);
        Db.SaveChanges();

        var query = source.ApplyListFiltering(Db.Set<TModel>(), filterParams);

        return (propInfo, query);
    }

    [Test]
    public async Task ApplyListPropertyFilters_WhenPropNotClientExposed_IgnoresProp()
    {
        var (prop, query) = PropertyFiltersTestHelper<ComplexModel, string>(
            m => m.InternalUseProperty, "propValue", "inputValue");

        // Precondition
        await Assert.That(prop.IsInternalUse).IsTrue();

        await Assert.That(query).HasSingleItem();
    }

    [Test]
    public async Task ApplyListPropertyFilters_WhenPropNotMapped_IgnoresProp()
    {
        // SPEC RATIONALE: Unmapped properties can't be translated to a query, so they would force
        // server-side evaluation of the filter, which could be staggeringly slow.
        // If a user wants a computed property filterable, it can be implemented as a datasource parameter.

        var (prop, query) = PropertyFiltersTestHelper<ComplexModel, string>(
            m => m.UnmappedSettableString, "propValue", "inputValue");

        // Precondition
        await Assert.That(prop.HasNotMapped).IsTrue();

        await Assert.That(query).HasSingleItem();
    }


    [Test]
    public async Task ApplyListPropertyFilters_WhenPropNotAuthorized_IgnoresProp()
    {
        const string role = RoleNames.Admin;
        var (prop, query) = PropertyFiltersTestHelper<ComplexModel, string>(
            m => m.AdminReadableString, "propValue", "inputValue");

        // Preconditions
        await Assert.That(CrudContext.User.IsInRole(role)).IsFalse();
        // TODO: TUnit migration - Assert.Collection had element inspectors. Manually add assertions for each element.
        await Assert.That(prop.SecurityInfo.Read.RoleList).Count().IsEqualTo(1);

        await Assert.That(query).HasSingleItem();
    }

    [Test]
    public async Task ApplyListPropertyFilters_WhenPropRestricted_IgnoresProp()
    {
        var (prop, query) = PropertyFiltersTestHelper<ComplexModel, string>(
            m => m.RestrictedString, "propValue", "inputValue");

        // Preconditions
        await Assert.That(CrudContext.User.Identity?.IsAuthenticated).IsNotEqualTo(true);
        await Assert.That(prop.SecurityInfo.Restrictions).IsNotEmpty();

        await Assert.That(query).HasSingleItem();
    }

    [Test]
    public async Task ApplyListPropertyFilters_WhenPropAuthorized_FiltersProp()
    {
        const string role = RoleNames.Admin;
        CrudContext.User.AddIdentity(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, role) }));

        var (prop, query) = PropertyFiltersTestHelper<ComplexModel, string>(
            m => m.AdminReadableString, "propValue", "inputValue");
        // TODO: TUnit migration - Assert.Collection had element inspectors. Manually add assertions for each element.

        // Precondition
        await Assert.That(prop.SecurityInfo.Read.RoleList).Count().IsEqualTo(1);

        await Assert.That(query).IsEmpty();
    }

    [Test]
    public async Task ApplyListPropertyFilters_WhenPropRestrictionPasses_FiltersProp()
    {
        CrudContext.User.AddIdentity(new ClaimsIdentity("foo"));
        var (prop, query) = PropertyFiltersTestHelper<ComplexModel, string>(
            m => m.RestrictedString, "propValue", "inputValue");

        // Preconditions
        await Assert.That(CrudContext.User.Identity?.IsAuthenticated).IsTrue();
        await Assert.That(prop.SecurityInfo.Restrictions).IsNotEmpty();

        await Assert.That(query).IsEmpty();
    }


    public static IEnumerable<object[]> Filter_MatchesDateTimesData = new[]
    {
        new object[] { true, "2017-08-02", new DateTime(2017, 08, 02, 0, 0, 0) },
        new object[] { true, "2017-08-02", new DateTime(2017, 08, 02, 23, 59, 59) },
        new object[] { true, "2017-08-02", new DateTime(2017, 08, 02, 12, 59, 59) },
        new object[] { false, "2017-08-02", new DateTime(2017, 08, 03, 0, 0, 0) },
        new object[] { false, "2017-08-02", new DateTime(2017, 08, 01, 23, 59, 59) },

        new object[] { true, "2017-08-02T12:34:56", new DateTime(2017, 08, 2, 12, 34, 56) },
        new object[] { false, "2017-08-02T12:34:56", new DateTime(2017, 08, 2, 12, 34, 55) },
        new object[] { false, "2017-08-02T12:34:56", new DateTime(2017, 08, 2, 12, 34, 57) },
        new object[] { false, "can't parse", new DateTime(2017, 08, 2, 12, 34, 57) },
        
        // The exact value "null" should match null values exactly.
        new object[] { true, "null", null },
        new object[] { false, "null", new DateTime(2017, 08, 2, 12, 34, 57) },
        
        // Null or empty inputs always do nothing - these will always match.
        new object[] { true, "", new DateTime(2017, 08, 2, 12, 34, 57) },
        new object[] { true, null, new DateTime(2017, 08, 2, 12, 34, 57) },
    };

    [Test]
    [InstanceMethodDataSource(nameof(Filter_MatchesDateTimesData))]
    public async Task ApplyListPropertyFilter_WhenPropIsDateTime_FiltersProp(
        bool shouldMatch, string inputValue, DateTime? fieldValue)
    {
        var (prop, query) = PropertyFiltersTestHelper<ComplexModel, DateTime?>(
            m => m.DateTimeNullable, fieldValue, inputValue);

        await Assert.That(query.Count()).IsEqualTo(shouldMatch ? 1 : 0);
    }

    public static IEnumerable<object[]> Filter_MatchesDateOnlyData = new[]
    {
        new object[] { true, "2017-08-02", new DateOnly(2017, 08, 02) },
        new object[] { false, "2017-08-02", new DateOnly(2017, 08, 03) },
        new object[] { false, "2017-08-02", new DateOnly(2017, 08, 01) },

        new object[] { true, "2017-08-02T12:34:56", new DateOnly(2017, 08, 02) },
        new object[] { false, "2017-08-02T12:34:56", new DateOnly(2017, 08, 03) },

        new object[] { false, "can't parse", new DateOnly(2017, 08, 02) },
        
        // Null or empty inputs always do nothing - these will always match.
        new object[] { true, "", new DateOnly(2017, 08, 02) },
        new object[] { true, null, new DateOnly(2017, 08, 02) },
    };

    [Test]
    [InstanceMethodDataSource(nameof(Filter_MatchesDateOnlyData))]
    public async Task ApplyListPropertyFilter_WhenPropIsDateOnly_FiltersProp(
        bool shouldMatch, string inputValue, DateOnly fieldValue)
    {
        var (prop, query) = PropertyFiltersTestHelper<ComplexModel, DateOnly>(
            m => m.SystemDateOnly, fieldValue, inputValue);

        await Assert.That(query.Count()).IsEqualTo(shouldMatch ? 1 : 0);
    }

    public static IEnumerable<object[]> Filter_MatchesTimeOnlyData = new[]
    {
        new object[] { true, "12:34:56", new TimeOnly(12, 34, 56) },
        new object[] { true, "12:34", new TimeOnly(12, 34, 00) },
        new object[] { false, "12:34:56", new TimeOnly(12, 34, 55) },
        new object[] { false, "12:34:56", new TimeOnly(12, 34, 57) },
        new object[] { false, "can't parse", new TimeOnly(12, 34, 56) },
        
        // Null or empty inputs always do nothing - these will always match.
        new object[] { true, "", new TimeOnly(12, 34, 56) },
        new object[] { true, null, new TimeOnly(12, 34, 56) },
    };

    [Test]
    [InstanceMethodDataSource(nameof(Filter_MatchesTimeOnlyData))]
    public async Task ApplyListPropertyFilter_WhenPropIsTimeOnly_FiltersProp(
        bool shouldMatch, string inputValue, TimeOnly fieldValue)
    {
        var (prop, query) = PropertyFiltersTestHelper<ComplexModel, TimeOnly>(
            m => m.SystemTimeOnly, fieldValue, inputValue);

        await Assert.That(query.Count()).IsEqualTo(shouldMatch ? 1 : 0);
    }


    public static IEnumerable<object[]> Filter_MatchesDateTimeOffsetsData = new[]
    {
        new object[] { true, "2017-08-02", -8, new DateTimeOffset(2017, 08, 02, 0, 0, 0, TimeSpan.FromHours(-8)) },
        new object[] { true, "2017-08-02", -8, new DateTimeOffset(2017, 08, 02, 23, 59, 59, TimeSpan.FromHours(-8)) },
        new object[] { true, "2017-08-02", -8, new DateTimeOffset(2017, 08, 02, 12, 59, 59, TimeSpan.FromHours(-8)) },
        new object[] { false, "2017-08-02", -8, new DateTimeOffset(2017, 08, 03, 0, 0, 0, TimeSpan.FromHours(-8)) },
        new object[] { false, "2017-08-02", -8, new DateTimeOffset(2017, 08, 01, 23, 59, 59, TimeSpan.FromHours(-8)) },

        new object[] { true, "2017-08-02", -5, new DateTimeOffset(2017, 08, 02, 23, 59, 59, TimeSpan.FromHours(-5)) },
        new object[] { true, "2017-08-02", -6, new DateTimeOffset(2017, 08, 02, 23, 59, 59, TimeSpan.FromHours(-5)) },
        new object[] { false, "2017-08-02", -4, new DateTimeOffset(2017, 08, 02, 23, 59, 59, TimeSpan.FromHours(-5)) },

        new object[] { true, "2017-08-02T12:34:56", -8, new DateTimeOffset(2017, 08, 2, 12, 34, 56, TimeSpan.FromHours(-8)) },
        new object[] { false, "2017-08-02T12:34:56", -8, new DateTimeOffset(2017, 08, 2, 12, 34, 55, TimeSpan.FromHours(-8)) },
        new object[] { false, "2017-08-02T12:34:56", -8, new DateTimeOffset(2017, 08, 2, 12, 34, 57, TimeSpan.FromHours(-8)) },
        new object[] { false, "can't parse", -8, new DateTimeOffset(2017, 08, 2, 12, 34, 57, TimeSpan.FromHours(-8)) },
        
        // The exact value "null" should match null values exactly.
        new object[] { true, "null", -8, null },
        new object[] { false, "null", -8, new DateTimeOffset(638353181662275815, TimeSpan.Zero) },
        
        // Null or empty inputs always do nothing - these will always match.
        new object[] { true, "", -8, new DateTimeOffset(2017, 08, 2, 12, 34, 57, TimeSpan.FromHours(-8)) },
        new object[] { true, null, -8, new DateTimeOffset(2017, 08, 2, 12, 34, 57, TimeSpan.FromHours(-8)) },
    };

    [Test]
    [InstanceMethodDataSource(nameof(Filter_MatchesDateTimeOffsetsData))]
    public async Task ApplyListPropertyFilter_WhenPropIsDateTimeOffset_FiltersProp(
        bool shouldMatch, string inputValue, int usersUtcOffset, DateTimeOffset? fieldValue)
    {
        CrudContext.TimeZone = TimeZoneInfo.CreateCustomTimeZone("test", TimeSpan.FromHours(usersUtcOffset), "test", "test");
        var (prop, query) = PropertyFiltersTestHelper<ComplexModel, DateTimeOffset?>(
            m => m.DateTimeOffsetNullable, fieldValue, inputValue);

        await Assert.That(query.Count()).IsEqualTo(shouldMatch ? 1 : 0);
    }

    [Test]
    [Arguments(true, Case.Statuses.ClosedNoSolution, (int)Case.Statuses.ClosedNoSolution)]
    [Arguments(true, Case.Statuses.ClosedNoSolution, nameof(Case.Statuses.ClosedNoSolution))]
    // Erratic spaces intentional
    [Arguments(true, Case.Statuses.ClosedNoSolution, " 3 , 4 ")]
    [Arguments(true, Case.Statuses.ClosedNoSolution, "  closednosolution , Cancelled,  Resolved ")]
    [Arguments(false, Case.Statuses.ClosedNoSolution, 5)]
    [Arguments(false, Case.Statuses.ClosedNoSolution, "1,2")]
    [Arguments(false, Case.Statuses.ClosedNoSolution, "closed,Cancelled,Resolved")]

    // The exact value "null" should match null values exactly.
    [Arguments(true, null, "null")]
    [Arguments(false, Case.Statuses.ClosedNoSolution, "null")]

    // Null or empty inputs always do nothing - these will always match.
    [Arguments(true, Case.Statuses.ClosedNoSolution, "")]
    [Arguments(true, Case.Statuses.ClosedNoSolution, null)]
    public async Task ApplyListPropertyFilter_WhenPropIsEnum_FiltersProp(
        bool shouldMatch, Case.Statuses? propValue, object inputValue)
    {
        var (prop, query) = PropertyFiltersTestHelper<ComplexModel, Case.Statuses?>(
            m => m.EnumNullable, propValue, inputValue?.ToString());

        await Assert.That(query.Count()).IsEqualTo(shouldMatch ? 1 : 0);
    }

    public static IEnumerable<object[]> Filter_MatchesGuidData = new[]
    {
        new object[] { true, Guid.Parse("{1358AEE1-4CFF-4957-961C-2A60F769BD41}"), "{1358AEE1-4CFF-4957-961C-2A60F769BD41}" },
        new object[] { true, Guid.Parse("{1358AEE1-4CFF-4957-961C-2A60F769BD41}"), "1358AEE1-4CFF-4957-961C-2A60F769BD41" },
        new object[] { true, Guid.Parse("{1358AEE1-4CFF-4957-961C-2A60F769BD41}"), " 1358AEE1-4CFF-4957-961C-2A60F769BD41 " },
        new object[] { true, Guid.Parse("{1358AEE1-4CFF-4957-961C-2A60F769BD41}"), " 1358AEE14CFF4957961C2A60F769BD41 " },
        new object[] { true, Guid.Parse("{1358AEE1-4CFF-4957-961C-2A60F769BD41}"), "{1358aee1-4cff-4957-961c-2a60f769bd41}" },
        new object[] { true, Guid.Parse("{1358AEE1-4CFF-4957-961C-2A60F769BD41}"), "1358aee1-4cff-4957-961c-2a60f769bd41" },
        new object[] { true,
            Guid.Parse("{1358AEE1-4CFF-4957-961C-2A60F769BD41}"),
            "null,1358aee1-4cff-4957-961c-2a60f769bd41" },
        new object[] { true,
            Guid.Parse("{1358AEE1-4CFF-4957-961C-2A60F769BD41}"),
            "DF657AE0-7A0F-4949-9A77-F617CDD21E33,1358aee1-4cff-4957-961c-2a60f769bd41" },

        new object[] { false,
            Guid.Parse("{1358AEE1-4CFF-4957-961C-2A60F769BD41}"),
            "08DACB7C-BF2E-42B6-AC7A-A499AD659F5C" },
        new object[] { false,
            Guid.Parse("{1358AEE1-4CFF-4957-961C-2A60F769BD41}"),
            "2311EC89-EE9E-42C8-AC14-87D0AC535F6A,361DF78D-24BB-4E0A-8D38-81DF73ACE1CA" },

        new object[] { true, Guid.Empty, "00000000-0000-0000-0000-000000000000" },
        new object[] { false, Guid.Empty, "1358AEE1-4CFF-4957-961C-2A60F769BD41" },
        
        // Null or empty inputs always do nothing - these will always match.
        new object[] { true, Guid.Empty, "" },
        new object[] { true, Guid.Empty, null },

        // String "null" input should match null values
        new object[] { true, null, "null" },
        new object[] { false, Guid.Empty, "null" },

        new object[] { true, Guid.Parse("{1358AEE1-4CFF-4957-961C-2A60F769BD41}"), "" },
        new object[] { true, Guid.Parse("{1358AEE1-4CFF-4957-961C-2A60F769BD41}"), null },
    };

    [Test]
    [InstanceMethodDataSource(nameof(Filter_MatchesGuidData))]
    public async Task ApplyListPropertyFilter_WhenPropIsGuid_FiltersProp(
        bool shouldMatch, Guid? propValue, string inputValue)
    {
        var (prop, query) = PropertyFiltersTestHelper<ComplexModel, Guid?>(
            m => m.GuidNullable, propValue, inputValue);

        await Assert.That(query.Count()).IsEqualTo(shouldMatch ? 1 : 0);
    }

    public static IEnumerable<object[]> Filter_MatchesUriData = new[]
    {
        new object[] { true, new Uri("mailto:test"), "mailto:test" },
        new object[] { true, new Uri("https://www.google.com/"), "https://www.google.com/" },
        new object[] { true, new Uri("https://john.doe@www.example.com:1234/forum/questions/?tag=networking&order=newest#top"), "https://john.doe@www.example.com:1234/forum/questions/?tag=networking&order=newest#top" },

        new object[] { false, new Uri("mailto:test"), "http://foo.com" },

        // Null or empty inputs always do nothing - these will always match.
        new object[] { true, new Uri("foo:bar"), "" },
        new object[] { true, new Uri("foo:bar"), null },

        // String "null" input should match null values
        new object[] { true, null, "null" },
    };

    [Test]
    [InstanceMethodDataSource(nameof(Filter_MatchesUriData))]
    public async Task ApplyListPropertyFilter_WhenPropIsUri_FiltersProp(
        bool shouldMatch, Uri propValue, string inputValue)
    {
        var (prop, query) = PropertyFiltersTestHelper<ComplexModel, Uri>(
            m => m.Uri, propValue, inputValue);

        await Assert.That(query.Count()).IsEqualTo(shouldMatch ? 1 : 0);
    }

    [Test]
    [Arguments(true, 1, "1")]
    [Arguments(true, 1, "1,2")]
    [Arguments(true, 1, " 1 , 2 ")]
    [Arguments(true, 1, " 1 ,  ")]
    [Arguments(true, 1, " 1 , $0.0 ")]
    [Arguments(true, 1, " 1 , string ")]

    // Null or empty inputs always do nothing - these will always match.
    [Arguments(true, 1, "")]
    [Arguments(true, 1, null)]

    // String "null" input should match null values
    [Arguments(true, null, "null")]
    [Arguments(false, 1, "null")]

    [Arguments(false, 1, " 3 , 2 ")]
    [Arguments(false, 1, "2")]
    [Arguments(false, 1, "string")]
    public async Task ApplyListPropertyFilter_WhenPropIsNumeric_FiltersProp(
        bool shouldMatch, int? propValue, string inputValue)
    {
        var (prop, query) = PropertyFiltersTestHelper<ComplexModel, int?>(
            m => m.IntNullable, propValue, inputValue);

        await Assert.That(query.Count()).IsEqualTo(shouldMatch ? 1 : 0);
    }

    [Test]
    [Arguments(true, "propVal", "propVal")]
    [Arguments(true, "propVal", "")]

    // Null or empty inputs always do nothing - these will always match.
    [Arguments(true, "propVal", null)]
    [Arguments(true, null, null)]
    [Arguments(true, null, "")]

    [Arguments(true, "propVal", "prop*")]
    [Arguments(false, null, "prop")]
    [Arguments(false, "propVal", "proppVal")]
    [Arguments(false, "propVal", "3")]
    [Arguments(false, "propVal", "propp*")]
    [Arguments(false, "propVal", "propp**")]
    [Arguments(false, "propVal", "propp***")]
    public async Task ApplyListPropertyFilter_WhenPropIsString_FiltersProp(
        bool shouldMatch, string propValue, string inputValue)
    {
        var (prop, query) = PropertyFiltersTestHelper<ComplexModel, string>(
            m => m.String, propValue, inputValue);

        await Assert.That(query.Count()).IsEqualTo(shouldMatch ? 1 : 0);
    }


    [Test]
    public async Task ApplyListClientSpecifiedSorting_ChecksPropAuthorization()
    {
        var models = new[]
        {
            new ComplexModel { ComplexModelId = 10, },
            new ComplexModel { ComplexModelId = 9 },
            new ComplexModel { ComplexModelId = 1, AdminReadableReferenceNavigationId = 10 },
            new ComplexModel { ComplexModelId = 2, AdminReadableReferenceNavigationId = 9 },
        };

        var source = Source<ComplexModel>();
        source.Db.AddRange(models);
        source.Db.SaveChanges();

        // Order should be the same for both the unsorted and the unauthorized sorted order.
        await source.Query().AssertOrder(models);

        // Order should do nothing because the prop is unauthorized.
        await source
            .Query(s => s.ApplyListSorting(s.Query(), new ListParameters
            {
                OrderBy = $"{nameof(ComplexModel.RestrictedString)}"
            }))
            .AssertOrder(models);
        await source
            .Query(s => s.ApplyListSorting(s.Query(), new ListParameters
            {
                OrderBy = $"{nameof(ComplexModel.AdminReadableReferenceNavigation)}.{nameof(ComplexModel.ComplexModelId)}"
            }))
            .AssertOrder(models);

        // Order should do nothing because the prop is unauthorized,
        // and subsequent orderings after any un-handlable ordering are ignored.
        await source
            .Query(s => s.ApplyListSorting(s.Query(), new ListParameters
            {
                OrderBy = $"{nameof(ComplexModel.AdminReadableReferenceNavigation)}.{nameof(ComplexModel.ComplexModelId)} ASC, {nameof(ComplexModel.ComplexModelId)} DESC"
            }))
            .AssertOrder(models);


        // Order should work because the user is now part of the required role.
        source.User.LogIn();
        await source
            .Query(s => s.ApplyListSorting(s.Query(), new ListParameters
            {
                OrderBy = $"{nameof(ComplexModel.AdminReadableReferenceNavigation)}.{nameof(ComplexModel.ComplexModelId)}"
            }))
            .AssertOrder(m => m.ComplexModelId, 10, 9, 2, 1);
    }

    [Test]
    public async Task ApplyListClientSpecifiedSorting_IgnoresInvalidProperties()
    {
        var source = Source<ComplexModel>()
            .AddModel(new ComplexModel { ComplexModelId = 1, String = "def" })
            .AddModel(new ComplexModel { ComplexModelId = 2, String = "abc" });

        // Order should do nothing because "FOOBAR" isn't a valid prop.
        await source
            .Query(s => s.ApplyListSorting(s.Query(), new ListParameters
            {
                OrderBy = $"FOOBAR ASC, {nameof(ComplexModel.String)} ASC"
            }))
            .AssertOrder(m => m.ComplexModelId, 1, 2);

        // Order should do nothing because "ComplexModelId" isn't an object prop.
        await source
            .Query(s => s.ApplyListSorting(s.Query(), new ListParameters
            {
                OrderBy = $"ComplexModelId.FooBar ASC"
            }))
            .AssertOrder(m => m.ComplexModelId, 1, 2);
    }

    [Test]
    public async Task ApplyListClientSpecifiedSorting_UsesDefaultOrderingForPoco()
    {
        var models = new[]
        {
            new ComplexModel { ComplexModelId = 2, Name = "abc" }, // reference: def 1
            new ComplexModel { ComplexModelId = 1, Name = "def" }, // reference: def 3
            new ComplexModel { ComplexModelId = 3, Name = "def" }, // reference: abc 2
        };
        models[0].ReferenceNavigation = models[1];
        models[1].ReferenceNavigation = models[2];
        models[2].ReferenceNavigation = models[0];

        var source = Source<ComplexModel>();
        source.Db.AddRange(models);
        source.Db.SaveChanges();

        // Precondition:
        var defaultOrdering = source.ClassViewModel.DefaultOrderBy.ToList();
        await Assert.That(defaultOrdering.Count).IsEqualTo(2);
        await Assert.That(defaultOrdering[0].Properties[0].Name).IsEqualTo(nameof(ComplexModel.Name));
        await Assert.That(defaultOrdering[1].Properties[0].Name).IsEqualTo(nameof(ComplexModel.ComplexModelId));

        source.User.LogIn();
        await source
            .Query(s => s.ApplyListSorting(s.Query(), new ListParameters
            {
                OrderBy = $"{nameof(ComplexModel.ReferenceNavigation)} DESC"
            }))
            .AssertOrder(m => m.ComplexModelId, 1, 2, 3);
    }

    [Test]
    public async Task ApplyListDefaultSorting_UsesAttributeOrdering()
    {
        var models = new[]
        {
            new ComplexModel { ComplexModelId = 2, Name = "abc" },
            new ComplexModel { ComplexModelId = 1, Name = "def" },
            new ComplexModel { ComplexModelId = 3, Name = "def" },
        };

        var source = Source<ComplexModel>();
        source.Db.AddRange(models);
        source.Db.SaveChanges();

        // Precondition:
        var defaultOrdering = source.ClassViewModel.DefaultOrderBy.ToList();
        await Assert.That(defaultOrdering.Count()).IsEqualTo(2);
        await Assert.That(defaultOrdering[0].Properties[0].Name).IsEqualTo(nameof(ComplexModel.Name));
        await Assert.That(defaultOrdering[1].Properties[0].Name).IsEqualTo(nameof(ComplexModel.ComplexModelId));

        source.User.LogIn();
        await source
            .Query(s => s.ApplyListSorting(s.Query(), new ListParameters()))
            .AssertOrder(m => m.ComplexModelId, 2, 1, 3);
    }

    [Test]
    public async Task ApplyListDefaultSorting_UsesNameIfNoAttribute()
    {
        var models = new[]
        {
            new Product { ProductId = 2, Name = "abc" },
            new Product { ProductId = 1, Name = "def" },
            new Product { ProductId = 3, Name = "def" },
        };

        var source = Source<Product>();
        source.Db.AddRange(models);
        source.Db.SaveChanges();

        // Precondition:
        var defaultOrdering = source.ClassViewModel.DefaultOrderBy.ToList();
        await Assert.That(defaultOrdering).HasSingleItem();
        await Assert.That(defaultOrdering[0].Properties[0].Name).IsEqualTo(nameof(Product.Name));

        source.User.LogIn();
        await source
            .Query(s => s.ApplyListSorting(s.Query(), new ListParameters()))
            .AssertOrder(m => m.ProductId, 2, 1, 3);
    }

    [Test]
    public async Task ApplyListDefaultSorting_FallsBackToId()
    {
        var models = new[]
        {
            new Person { PersonId = 2, },
            new Person { PersonId = 1, },
            new Person { PersonId = 3, },
        };

        var source = Source<Person>();
        source.Db.AddRange(models);
        source.Db.SaveChanges();

        // Precondition:
        var defaultOrdering = source.ClassViewModel.DefaultOrderBy.ToList();
        await Assert.That(defaultOrdering).HasSingleItem();
        await Assert.That(defaultOrdering[0].Properties[0].Name).IsEqualTo(nameof(Person.PersonId));

        source.User.LogIn();
        await source
            .Query(s => s.ApplyListSorting(s.Query(), new ListParameters()))
            .AssertOrder(m => m.PersonId, 1, 2, 3);
    }

    [Test]
    public async Task ApplyListDefaultSorting_RespectsSuppressFallback()
    {
        var models = new[]
        {
            new Testing.TargetClasses.SuppressedDefaultOrdering { Id = 2, Name = "abc" },
            new Testing.TargetClasses.SuppressedDefaultOrdering { Id = 1, Name = "def" },
            new Testing.TargetClasses.SuppressedDefaultOrdering { Id = 3, Name = "def" },
        };

        var source = Source<Testing.TargetClasses.SuppressedDefaultOrdering>();
        source.Db.AddRange(models);
        source.Db.SaveChanges();

        // Precondition: should have no default ordering
        var defaultOrdering = source.ClassViewModel.DefaultOrderBy.ToList();
        await Assert.That(defaultOrdering).IsEmpty();

        source.User.LogIn();
        // Without any sorting specified, the query should maintain database/insertion order
        // (no ordering will be applied)
        var results = source
            .Query(s => s.ApplyListSorting(s.Query(), new ListParameters()))
            .ToList();

        // Results should be in insertion order (2, 1, 3) since no ordering was applied
        await Assert.That(results.Count).IsEqualTo(3);
    }

    [Test]
    [Arguments(true, "propVal", "string:propV")]
    [Arguments(false, "propVal", "string:proppV")]
    public async Task ApplyListSearchTerm_WhenTermTargetsField_SearchesField(
        bool shouldMatch, string propValue, string inputValue)
    {
        await Source<ComplexModel>()
            .AddModel(m => m.String, propValue)
            .Query(s => s.ApplyListSearchTerm(s.Query(), new FilterParameters
            {
                Search = inputValue
            }))
            .AssertMatched(shouldMatch);
    }

    [Test]
    public async Task ApplyListSearchTerm_WhenTargetedPropNotAuthorized_IgnoresProp()
    {
        var query = Source<ComplexModel>()
            .AddModel(m => m.AdminReadableString, "propValue", out PropertyViewModel prop)
            .Query(s => s.ApplyListSearchTerm(s.Query(), new FilterParameters
            {
                Search = "AdminReadableString:propV"
            }));

        // Preconditions
        const string role = RoleNames.Admin;
        await Assert.That(CrudContext.User.IsInRole(role)).IsFalse();
        await Assert.That(prop.SearchMethod == DataAnnotations.SearchAttribute.SearchMethods.BeginsWith).IsTrue();
        // TODO: TUnit migration - Assert.Collection had element inspectors. Manually add assertions for each element.
        await Assert.That(prop.SecurityInfo.Read.RoleList).Count().IsEqualTo(1);

        // Since searching by prop isn't valid for this specific property,
        // the search will instead treat the entire input as the search term.
        // Since this search term doesn't match the property's value, the results should be empty.
        await query.AssertMatched(false);
    }

    [Test]
    public async Task ApplyListSearchTerm_WhenTargetedPropRestricted_IgnoresProp()
    {
        var query = Source<ComplexModel>()
            .AddModel(m => m.RestrictedString, "propValue", out PropertyViewModel prop)
            .Query(s => s.ApplyListSearchTerm(s.Query(), new FilterParameters
            {
                Search = "RestrictedString:propV"
            }));

        // Since this search term doesn't match the property's value, the results should be empty.
        await query.AssertMatched(false);
    }

    [Test]
    public async Task ApplyListPropertyFilters_WhenPropertyIsPrimitiveCollection_ContainsFilter()
    {
        var source = Source<ComplexModel>();
        var prop = source.ClassViewModel.PropertyByName(nameof(ComplexModel.IntCollection));

        // Test with models that either contain or don't contain the filter value
        var model1 = new ComplexModel { IntCollection = new List<int> { 1, 2, 3 } }; // Contains 2
        var model2 = new ComplexModel { IntCollection = new List<int> { 4, 5, 6 } }; // Doesn't contain 2

        Db.Set<ComplexModel>().AddRange(model1, model2);
        Db.SaveChanges();

        var filterParams = new FilterParameters();
        filterParams.Filter[prop.JsonName] = "2";

        var query = source.ApplyListFiltering(Db.Set<ComplexModel>(), filterParams);

        // Should match only model1 (contains 2)
        await Assert.That(prop.IsUrlFilterParameter).IsTrue();
        await Assert.That(query).HasSingleItem();
        await Assert.That(query.Single().ComplexModelId).IsEqualTo(model1.ComplexModelId);
    }

    [Test]
    public async Task ApplyListPropertyFilters_WhenPropertyIsPrimitiveCollection_MultipleValues()
    {
        var source = Source<ComplexModel>();
        var prop = source.ClassViewModel.PropertyByName(nameof(ComplexModel.IntCollection));

        var model1 = new ComplexModel { IntCollection = new List<int> { 1, 2, 3 } };
        var model2 = new ComplexModel { IntCollection = new List<int> { 4, 5, 3 } };
        var model3 = new ComplexModel { IntCollection = new List<int> { 2, 7, 8 } };

        Db.Set<ComplexModel>().AddRange(model1, model2, model3);
        Db.SaveChanges();

        var filterParams = new FilterParameters();
        filterParams.Filter[prop.JsonName] = "3,2";

        var query = source.ApplyListFiltering(Db.Set<ComplexModel>(), filterParams);

        // Should match model1 only (only one with both 2 and 3)
        await Assert.That(query.Count()).IsEqualTo(1);
    }

    [Test]
    public async Task ApplyListPropertyFilters_WhenPropertyIsPrimitiveCollection_NoMatch()
    {
        var source = Source<ComplexModel>();
        var prop = source.ClassViewModel.PropertyByName(nameof(ComplexModel.IntCollection));

        var model1 = new ComplexModel { IntCollection = new List<int> { 1, 2, 3 } };
        var model2 = new ComplexModel { IntCollection = new List<int> { 4, 5, 6 } };

        Db.Set<ComplexModel>().AddRange(model1, model2);
        Db.SaveChanges();

        var filterParams = new FilterParameters();
        filterParams.Filter[prop.JsonName] = "9"; // Value not in any collection

        var query = source.ApplyListFiltering(Db.Set<ComplexModel>(), filterParams);

        // Should match no models
        await Assert.That(query).IsEmpty();
    }

    [Test]
    public async Task ApplyListPropertyFilters_WhenPropertyIsPrimitiveCollection_EmptyCollection()
    {
        var source = Source<ComplexModel>();
        var prop = source.ClassViewModel.PropertyByName(nameof(ComplexModel.IntCollection));

        var model1 = new ComplexModel { IntCollection = new List<int>() }; // Empty collection
        var model2 = new ComplexModel { IntCollection = new List<int> { 1, 2, 3 } };

        Db.Set<ComplexModel>().AddRange(model1, model2);
        Db.SaveChanges();

        var filterParams = new FilterParameters();
        filterParams.Filter[prop.JsonName] = "2";

        var query = source.ApplyListFiltering(Db.Set<ComplexModel>(), filterParams);

        // Should match only model2 (model1 has empty collection)
        await Assert.That(query).HasSingleItem();
        await Assert.That(query.Single().ComplexModelId).IsEqualTo(model2.ComplexModelId);
    }

    [Test]
    public async Task ApplyListPropertyFilters_WhenPropertyIsEnumCollection_ContainsFilter()
    {
        var source = Source<ComplexModel>();
        var prop = source.ClassViewModel.PropertyByName(nameof(ComplexModel.EnumCollection));

        var model1 = new ComplexModel { EnumCollection = new List<Case.Statuses> { Case.Statuses.Open, Case.Statuses.InProgress } };
        var model2 = new ComplexModel { EnumCollection = new List<Case.Statuses> { Case.Statuses.Resolved, Case.Statuses.ClosedNoSolution } };
        var model3 = new ComplexModel { EnumCollection = new List<Case.Statuses> { Case.Statuses.Open, Case.Statuses.Resolved } };

        Db.Set<ComplexModel>().AddRange(model1, model2, model3);
        Db.SaveChanges();

        var filterParams = new FilterParameters();
        filterParams.Filter[prop.JsonName] = "0"; // Open = 0

        var query = source.ApplyListFiltering(Db.Set<ComplexModel>(), filterParams);

        // Should match model1 and model3 (both contain Open status)
        await Assert.That(query.Count()).IsEqualTo(2);
    }
}
