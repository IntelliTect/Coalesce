using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Tests.Fixtures;
using IntelliTect.Coalesce.Tests.TargetClasses;
using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace IntelliTect.Coalesce.Tests.Api.DataSources
{
    public class StandardDataSourceTests : TestDbContextTests
    {
        public StandardDataSource<Case, TestDbContext> CaseSource { get; }

        public StandardDataSourceTests() : base()
        {
            CaseSource = Source<Case>();
        }

        private StandardDataSource<T, TestDbContext> Source<T>()
            where T : class, new()
            => new StandardDataSource<T, TestDbContext>(CrudContext);
        
        [Theory]
        [InlineData("none")]
        [InlineData("NONE")]
        public void GetQuery_WhenIncludesStringNone_DoesNotIncludeChildren(string includes)
        {
            var query = Source<Case>().GetQuery(new DataSourceParameters { Includes = includes });
            Assert.Empty(query.GetIncludeTree());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("CaseListGen")]
        public void GetQuery_WhenIncludesStringNotNone_IncludesChildren(string includes)
        {
            var query = Source<Case>().GetQuery(new DataSourceParameters { Includes = includes });
            var tree = query.GetIncludeTree();
            Assert.NotNull(tree[nameof(Case.AssignedTo)]);
            Assert.NotNull(tree[nameof(Case.ReportedBy)]);
            Assert.NotNull(tree[nameof(Case.CaseProducts)][nameof(CaseProduct.Product)]); 
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
            
            var query = source.ApplyListPropertyFilters(Db.Set<TModel>(), filterParams);

            return (propInfo, query);
        }

        [Fact]
        public void ApplyListPropertyFilters_WhenPropNotClientExposed_IgnoresProp()
        {
            var (prop, query) = PropertyFiltersTestHelper<ComplexModel, string>(
                m => m.InternalUseProperty, "propValue", "inputValue");

            // Precondition
            Assert.True(prop.IsInternalUse);


            Assert.Single(query);
        }

        [Fact]
        public void ApplyListPropertyFilters_WhenPropNotMapped_IgnoresProp()
        {
            // SPEC RATIONALE: Unmapped properties can't be translated to a query, so they would force
            // server-side evaluation of the filter, which could be staggeringly slow.
            // If a user wants a computed property filterable, it can be implemented as a datasource parameter.

            var (prop, query) = PropertyFiltersTestHelper<ComplexModel, string>(
                m => m.UnmappedSettableString, "propValue", "inputValue");

            // Precondition
            Assert.True(prop.HasNotMapped);


            Assert.Single(query);
        }


        [Fact]
        public void ApplyListPropertyFilters_WhenPropNotAuthorized_IgnoresProp()
        {
            const string role = RoleNames.Admin;
            var (prop, query) = PropertyFiltersTestHelper<ComplexModel, string>(
                m => m.AdminReadableString, "propValue", "inputValue");

            // Preconditions
            Assert.False(CrudContext.User.IsInRole(role));
            Assert.Collection(prop.SecurityInfo.ReadRolesList, r => Assert.Equal(role, r));


            Assert.Single(query);
        }

        [Fact]
        public void ApplyListPropertyFilters_WhenPropAuthorized_FiltersProp()
        {
            const string role = RoleNames.Admin;
            CrudContext.User.AddIdentity(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, role) }));

            var (prop, query) = PropertyFiltersTestHelper<ComplexModel, string>(
                m => m.AdminReadableString, "propValue", "inputValue");

            // Precondition
            Assert.Collection(prop.SecurityInfo.ReadRolesList, r => Assert.Equal(role, r));


            Assert.Empty(query);
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
            new object[] { false, "can't parse", DateTime.Now },
        };
        [Theory]
        [MemberData(nameof(Filter_MatchesDateTimesData))]
        public void ApplyListPropertyFilter_WhenPropIsDateTime_FiltersProp(
            bool shouldMatch, string inputValue, DateTime fieldValue)
        {
            var (prop, query) = PropertyFiltersTestHelper<ComplexModel, DateTime>(
                m => m.DateTime, fieldValue, inputValue);

            if (shouldMatch)
                Assert.Single(query);
            else
                Assert.Empty(query);
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
            new object[] { false, "can't parse", -8, DateTimeOffset.Now },
        };
        [Theory]
        [MemberData(nameof(Filter_MatchesDateTimeOffsetsData))]
        public void ApplyListPropertyFilter_WhenPropIsDateTimeOffset_FiltersProp(
            bool shouldMatch, string inputValue, int usersUtcOffset, DateTimeOffset fieldValue)
        {
            CrudContext.TimeZone = TimeZoneInfo.CreateCustomTimeZone("test", TimeSpan.FromHours(usersUtcOffset), "test", "test");
            var (prop, query) = PropertyFiltersTestHelper<ComplexModel, DateTimeOffset>(
                m => m.DateTimeOffset, fieldValue, inputValue);

            if (shouldMatch)
                Assert.Single(query);
            else
                Assert.Empty(query);
        }

        [Theory]
        [InlineData(true, Case.Statuses.ClosedNoSolution, (int)Case.Statuses.ClosedNoSolution)]
        [InlineData(true, Case.Statuses.ClosedNoSolution, nameof(Case.Statuses.ClosedNoSolution))]
        // Erratic spaces intentional
        [InlineData(true, Case.Statuses.ClosedNoSolution, "")]
        [InlineData(true, Case.Statuses.ClosedNoSolution, " 3 , 4 ")]
        [InlineData(true, Case.Statuses.ClosedNoSolution, "  closednosolution , Cancelled,  Resolved ")]
        [InlineData(false, Case.Statuses.ClosedNoSolution, 5)]
        [InlineData(false, Case.Statuses.ClosedNoSolution, "1,2")]
        [InlineData(false, Case.Statuses.ClosedNoSolution, "closed,Cancelled,Resolved")]
        public void ApplyListPropertyFilter_WhenPropIsEnum_FiltersProp(
            bool shouldMatch, Case.Statuses propValue, object inputValue)
        {
            var (prop, query) = PropertyFiltersTestHelper<Case, Case.Statuses>(
                m => m.Status, propValue, inputValue.ToString());

            if (shouldMatch)
                Assert.Single(query);
            else
                Assert.Empty(query);
        }

        [Theory]
        [InlineData(true, 1, "1")]
        [InlineData(true, 1, "1,2")]
        [InlineData(true, 1, " 1 , 2 ")]
        [InlineData(true, 1, " 1 ,  ")]
        [InlineData(true, 1, " 1 , $0.0 ")]
        [InlineData(true, 1, " 1 , string ")]
        [InlineData(true, 1, "")]
        [InlineData(true, 1, null)]
        [InlineData(false, 1, " 3 , 2 ")]
        [InlineData(false, 1, "2")]
        [InlineData(false, 1, "string")]
        public void ApplyListPropertyFilter_WhenPropIsNumeric_FiltersProp(
            bool shouldMatch, int propValue, string inputValue)
        {
            var (prop, query) = PropertyFiltersTestHelper<ComplexModel, int>(
                m => m.Int, propValue, inputValue);

            if (shouldMatch)
                Assert.Single(query);
            else
                Assert.Empty(query);
        }

        [Theory]
        [InlineData(true, "propVal", "propVal")]
        [InlineData(true, "propVal", "")]
        [InlineData(true, "propVal", null)]
        [InlineData(true, "propVal", "prop*")]
        [InlineData(false, "propVal", "proppVal")]
        [InlineData(false, "propVal", "3")]
        [InlineData(false, "propVal", "propp*")]
        [InlineData(false, "propVal", "propp**")]
        [InlineData(false, "propVal", "propp***")]
        public void ApplyListPropertyFilter_WhenPropIsString_FiltersProp(
            bool shouldMatch, string propValue, string inputValue)
        {
            var (prop, query) = PropertyFiltersTestHelper<ComplexModel, string>(
                m => m.String, propValue, inputValue);

            if (shouldMatch)
                Assert.Single(query);
            else
                Assert.Empty(query);
        }

        // TODO: WRITE THIS TEST!
        // [Theory]
        public void ApplyListSearchTerm_WhenTermTargetsField_SearchesField(
            bool shouldMatch, string propValue, string inputValue)
        {
            var (prop, query) = PropertyFiltersTestHelper<ComplexModel, string>(
                m => m.String, propValue, inputValue);

            if (shouldMatch)
                Assert.Single(query);
            else
                Assert.Empty(query);
        }
    }
}
