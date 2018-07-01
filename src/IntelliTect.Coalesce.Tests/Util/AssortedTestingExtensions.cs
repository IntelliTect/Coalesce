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

namespace IntelliTect.Coalesce.Tests.Util
{



    internal static class AssortedTestingExtensions
    {
        public static StandardDataSource<TModel, TestDbContext> AddModel<TModel, TProp>(
            this StandardDataSource<TModel, TestDbContext> source, Expression<Func<TModel, TProp>> propSelector, TProp propValue
        )
            where TModel : class, new()
        {
            return source.AddModel(propSelector, propValue, out _);
        }

        public static StandardDataSource<TModel, TestDbContext> AddModel<TModel, TProp>(
            this StandardDataSource<TModel, TestDbContext> source, Expression<Func<TModel, TProp>> propSelector, TProp propValue, out PropertyViewModel propInfo
        )
            where TModel : class, new()
        {
            var model = new TModel();
            propInfo = source.ClassViewModel.PropertyBySelector(propSelector);
            propInfo.PropertyInfo.SetValue(model, propValue);
            return source.AddModel(model);
        }

        public static StandardDataSource<TModel, TestDbContext> AddModel<TModel>(
            this StandardDataSource<TModel, TestDbContext> source, TModel model
        )
            where TModel : class, new()
        {
            source.Db.Set<TModel>().Add(model);
            source.Db.SaveChanges();
            return source;
        }

        public static IQueryable<TModel> Query<TModel>(
            this StandardDataSource<TModel, TestDbContext> source, Func<StandardDataSource<TModel, TestDbContext>, IQueryable<TModel>> method
        )
            where TModel : class, new()
        {
            return method(source);
        }

        public static IQueryable<TModel> Query<TModel>(this StandardDataSource<TModel, TestDbContext> source)
            where TModel : class, new()
        {
            return source.Db.Set<TModel>();
        }

        public static void AssertMatched<TModel>(this IQueryable<TModel> query, bool shouldMatch)
            where TModel : class, new()
        {
            Assert.Equal(shouldMatch ? 1 : 0, query.Count());
        }

        public static void AssertOrder<TModel, TProp>(this IEnumerable<TModel> models, Func<TModel, TProp> propSelector, params TProp[] expectedOrder)
        {
            var modelsList = models.ToList();
            Assert.Equal(expectedOrder.Length, modelsList.Count);

            Assert.All(
                modelsList.Zip(expectedOrder, (model, expected) => propSelector(model).Equals(expected)),
                Assert.True
            );
        }

        public static void AssertOrder<TModel>(this IEnumerable<TModel> models, params TModel[] expectedOrder)
        {
            var modelsList = models.ToList();
            Assert.Equal(expectedOrder.Length, modelsList.Count);

            Assert.All(
                modelsList.Zip(expectedOrder, (model, expected) => model.Equals(expected)),
                Assert.True
            );
        }

        public static void LogIn(this ClaimsPrincipal user, string role = RoleNames.Admin)
        {
            var claims = role == null ? new Claim[0] : new[] { new Claim(ClaimTypes.Role, RoleNames.Admin) };
            user.AddIdentity(new ClaimsIdentity(claims, "TestAuth"));
        }
    }
}
