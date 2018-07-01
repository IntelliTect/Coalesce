using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Tests.Fixtures;
using IntelliTect.Coalesce.Tests.TargetClasses;
using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Tests.Util;
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
    public class StandardDataSourceTests : TestDbContextFixture
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
            
            var query = source.ApplyListFiltering(Db.Set<TModel>(), filterParams);

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
            new object[] { false, "can't parse", new DateTime(2017, 08, 2, 12, 34, 57) },
        };

        [Theory]
        [MemberData(nameof(Filter_MatchesDateTimesData))]
        public void ApplyListPropertyFilter_WhenPropIsDateTime_FiltersProp(
            bool shouldMatch, string inputValue, DateTime fieldValue)
        {
            var (prop, query) = PropertyFiltersTestHelper<ComplexModel, DateTime>(
                m => m.DateTime, fieldValue, inputValue);

            Assert.Equal(shouldMatch ? 1 : 0, query.Count());
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
        };

        [Theory]
        [MemberData(nameof(Filter_MatchesDateTimeOffsetsData))]
        public void ApplyListPropertyFilter_WhenPropIsDateTimeOffset_FiltersProp(
            bool shouldMatch, string inputValue, int usersUtcOffset, DateTimeOffset fieldValue)
        {
            CrudContext.TimeZone = TimeZoneInfo.CreateCustomTimeZone("test", TimeSpan.FromHours(usersUtcOffset), "test", "test");
            var (prop, query) = PropertyFiltersTestHelper<ComplexModel, DateTimeOffset>(
                m => m.DateTimeOffset, fieldValue, inputValue);

            Assert.Equal(shouldMatch ? 1 : 0, query.Count());
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

            Assert.Equal(shouldMatch ? 1 : 0, query.Count());
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
            new object[] { true, Guid.Parse("{1358AEE1-4CFF-4957-961C-2A60F769BD41}"), "" },
            new object[] { true, Guid.Parse("{1358AEE1-4CFF-4957-961C-2A60F769BD41}"), null },
        };

        [Theory]
        [MemberData(nameof(Filter_MatchesGuidData))]
        public void ApplyListPropertyFilter_WhenPropIsGuid_FiltersProp(
            bool shouldMatch, Guid propValue, string inputValue)
        {
            var (prop, query) = PropertyFiltersTestHelper<ComplexModel, Guid>(
                m => m.Guid, propValue, inputValue);

            Assert.Equal(shouldMatch ? 1 : 0, query.Count());
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

            Assert.Equal(shouldMatch ? 1 : 0, query.Count());
        }

        [Theory]
        [InlineData(true, "propVal", "propVal")]
        [InlineData(true, "propVal", "")]

        // Null or empty inputs always do nothing - these will always match.
        [InlineData(true, "propVal", null)]
        [InlineData(true, null, null)]
        [InlineData(true, null, "")]

        [InlineData(true, "propVal", "prop*")]
        [InlineData(false, null, "prop")]
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

            Assert.Equal(shouldMatch ? 1 : 0, query.Count());
        }


        [Fact]
        public void ApplyListClientSpecifiedSorting_ChecksPropAuthorization()
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
            source.Query().AssertOrder(models);

            // Order should do nothing because the prop is unauthorized.
            source
                .Query(s => s.ApplyListSorting(s.Query(), new ListParameters
                {
                    OrderBy = $"{nameof(ComplexModel.AdminReadableReferenceNavigation)}.{nameof(ComplexModel.ComplexModelId)}"
                }))
                .AssertOrder(models);

            // Order should do nothing because the prop is unauthorized,
            // and subsequent orderings after any un-handlable ordering are ignored.
            source
                .Query(s => s.ApplyListSorting(s.Query(), new ListParameters
                {
                    OrderBy = $"{nameof(ComplexModel.AdminReadableReferenceNavigation)}.{nameof(ComplexModel.ComplexModelId)} ASC, {nameof(ComplexModel.ComplexModelId)} DESC"
                }))
                .AssertOrder(models);


            // Order should work because the user is now part of the required role.
            source.User.LogIn();
            source
                .Query(s => s.ApplyListSorting(s.Query(), new ListParameters
                {
                    OrderBy = $"{nameof(ComplexModel.AdminReadableReferenceNavigation)}.{nameof(ComplexModel.ComplexModelId)}"
                }))
                .AssertOrder(m => m.ComplexModelId, 10, 9, 2, 1);
        }

        [Fact]
        public void ApplyListClientSpecifiedSorting_IgnoresInvalidProperties()
        {
            var source = Source<ComplexModel>()
                .AddModel(new ComplexModel { ComplexModelId = 1, String = "def" })
                .AddModel(new ComplexModel { ComplexModelId = 2, String = "abc" });

            // Order should do nothing because "FOOBAR" isn't a valid prop.
            source
                .Query(s => s.ApplyListSorting(s.Query(), new ListParameters
                {
                    OrderBy = $"FOOBAR ASC, {nameof(ComplexModel.String)} ASC"
                }))
                .AssertOrder(m => m.ComplexModelId, 1, 2);

            // Order should do nothing because "ComplexModelId" isn't an object prop.
            source
                .Query(s => s.ApplyListSorting(s.Query(), new ListParameters
                {
                    OrderBy = $"ComplexModelId.FooBar ASC"
                }))
                .AssertOrder(m => m.ComplexModelId, 1, 2);
        }

        [Fact]
        public void ApplyListClientSpecifiedSorting_UsesDefaultOrderingForPoco()
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
            Assert.Equal(2, defaultOrdering.Count());
            Assert.Equal(nameof(ComplexModel.Name), defaultOrdering[0].FieldName);
            Assert.Equal(nameof(ComplexModel.ComplexModelId), defaultOrdering[1].FieldName);

            source.User.LogIn();
            source
                .Query(s => s.ApplyListSorting(s.Query(), new ListParameters
                {
                    OrderBy = $"{nameof(ComplexModel.ReferenceNavigation)} DESC"
                }))
                .AssertOrder(m => m.ComplexModelId, 1, 2, 3);
        }

        [Fact]
        public void ApplyListDefaultSorting_UsesAttributeOrdering()
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
            Assert.Equal(2, defaultOrdering.Count());
            Assert.Equal(nameof(ComplexModel.Name), defaultOrdering[0].FieldName);
            Assert.Equal(nameof(ComplexModel.ComplexModelId), defaultOrdering[1].FieldName);

            source.User.LogIn();
            source
                .Query(s => s.ApplyListSorting(s.Query(), new ListParameters()))
                .AssertOrder(m => m.ComplexModelId, 2, 1, 3);
        }

        [Fact]
        public void ApplyListDefaultSorting_UsesNameIfNoAttribute()
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
            Assert.Single(defaultOrdering);
            Assert.Equal(nameof(Product.Name), defaultOrdering[0].FieldName);

            source.User.LogIn();
            source
                .Query(s => s.ApplyListSorting(s.Query(), new ListParameters()))
                .AssertOrder(m => m.ProductId, 2, 1, 3);
        }

        [Fact]
        public void ApplyListDefaultSorting_FallsBackToId()
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
            Assert.Single(defaultOrdering);
            Assert.Equal(nameof(Person.PersonId), defaultOrdering[0].FieldName);

            source.User.LogIn();
            source
                .Query(s => s.ApplyListSorting(s.Query(), new ListParameters()))
                .AssertOrder(m => m.PersonId, 1, 2, 3);
        }

        [Theory]
        [InlineData(true, "propVal", "string:propV")]
        [InlineData(false, "propVal", "string:proppV")]
        public void ApplyListSearchTerm_WhenTermTargetsField_SearchesField(
            bool shouldMatch, string propValue, string inputValue)
        {
            Source<ComplexModel>()
                .AddModel(m => m.String, propValue)
                .Query(s => s.ApplyListSearchTerm(s.Query(), new FilterParameters
                {
                    Search = inputValue
                }))
                .AssertMatched(shouldMatch);
        }

        [Fact]
        public void ApplyListSearchTerm_WhenTargetedPropNotAuthorized_IgnoresProp()
        {
            var query = Source<ComplexModel>()
                .AddModel(m => m.AdminReadableString, "propValue", out PropertyViewModel prop)
                .Query(s => s.ApplyListSearchTerm(s.Query(), new FilterParameters
                {
                    Search = "AdminReadableString:propV"
                }));

            // Preconditions
            const string role = RoleNames.Admin;
            Assert.False(CrudContext.User.IsInRole(role));
            Assert.True(prop.SearchMethod == DataAnnotations.SearchAttribute.SearchMethods.BeginsWith);
            Assert.Collection(prop.SecurityInfo.ReadRolesList, r => Assert.Equal(role, r));

            // Since searching by prop isn't valid for this specific property,
            // the search will instead treat the entire input as the search term.
            // Since this search term doesn't match the property's value, the results should be empty.
            query.AssertMatched(false);
        }
    }
}
