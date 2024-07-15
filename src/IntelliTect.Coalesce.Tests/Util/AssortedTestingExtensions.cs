using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Models;
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
using System.Threading.Tasks;
using Xunit;

#nullable enable

namespace IntelliTect.Coalesce.Tests.Util
{
    internal static class AssortedTestingExtensions
    {
        public static StandardDataSource<TModel, AppDbContext> AddModel<TModel, TProp>(
            this StandardDataSource<TModel, AppDbContext> source, Expression<Func<TModel, TProp>> propSelector, TProp propValue
        )
            where TModel : class, new()
        {
            return source.AddModel(propSelector, propValue, out _);
        }

        public static StandardDataSource<TModel, AppDbContext> AddModel<TModel, TProp>(
            this StandardDataSource<TModel, AppDbContext> source, Expression<Func<TModel, TProp>> propSelector, TProp propValue, out PropertyViewModel propInfo
        )
            where TModel : class, new()
        {
            var model = new TModel();
            propInfo = source.ClassViewModel.PropertyBySelector(propSelector);
            propInfo.PropertyInfo.SetValue(model, propValue);
            return source.AddModel(model);
        }

        public static StandardDataSource<TModel, AppDbContext> AddModel<TModel>(
            this StandardDataSource<TModel, AppDbContext> source, TModel model
        )
            where TModel : class, new()
        {
            source.Db.Set<TModel>().Add(model);
            source.Db.SaveChanges();
            return source;
        }

        public static IQueryable<TModel> Query<TModel>(
            this StandardDataSource<TModel, AppDbContext> source, Func<StandardDataSource<TModel, AppDbContext>, IQueryable<TModel>> method
        )
            where TModel : class, new()
        {
            return method(source);
        }

        public static IQueryable<TModel> Query<TModel>(this StandardDataSource<TModel, AppDbContext> source)
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

            foreach (var (model, expected) in modelsList.Zip(expectedOrder))
            {
                // Model not null because we asserted the length matches
                Assert.Equal(expected, propSelector(model!));
            }
        }

        public static void AssertOrder<TModel>(this IEnumerable<TModel> models, params TModel[] expectedOrder)
        {
            var modelsList = models.ToList();
            Assert.Equal(expectedOrder.Length, modelsList.Count);

            Assert.All(
                modelsList.Zip(expectedOrder, (model, expected) => model!.Equals(expected)),
                Assert.True
            );
        }

        /// <summary>
        /// Asserts that the result was a failure.
        /// </summary>
        public static void AssertError(this ApiResult result)
        {
            Assert.False(result.WasSuccessful);
        }

        /// <summary>
        /// Asserts that the result was a failure.
        /// </summary>
        public static void AssertError(this ApiResult result, string message)
        {
            result.AssertError();
            Assert.Equal(message, result.Message);
        }

        /// <summary>
        /// Asserts that the result was a failure.
        /// </summary>
        public static async Task AssertError<T>(this Task<T> resultTask, string message)
            where T : ApiResult
        {
            var result = await resultTask;
            result.AssertError();
            Assert.Equal(message, result.Message);
        }

        /// <summary>
        /// Asserts that the result was successful.
        /// </summary>
        public static void AssertSuccess(this ApiResult result, string? message = null)
        {
            // Returns a more useful assertion error than only checking WasSuccessful.
            Assert.Equal(message, result.Message);
            Assert.True(result.WasSuccessful);
        }

        /// <summary>
        /// Asserts that the result was successful.
        /// </summary>
        public static T AssertSuccess<T>(this ItemResult<T> result)
        {
            Assert.Null(result.Message);
            Assert.True(result.WasSuccessful);
            return result.Object!;
        }

        /// <summary>
        /// Asserts that the result was successful.
        /// </summary>
        public static async Task<T> AssertSuccess<T>(this Task<ItemResult<T>> resultTask)
        {
            var result = await resultTask;
            Assert.Null(result.Message);
            Assert.True(result.WasSuccessful);
            return result.Object!;
        }

        /// <summary>
        /// Asserts that the result was successful.
        /// </summary>
        public static async Task AssertSuccess(this Task<ItemResult> resultTask)
        {
            var result = await resultTask;
            Assert.True(result.WasSuccessful);
        }

        /// <summary>
        /// Asserts that the result was successful.
        /// </summary>
        public static void AssertSuccess<T>(this ItemResult<T> result, T value)
        {
            result.AssertSuccess();
            Assert.Equal(value, result.Object);
        }

        public static void LogIn(this ClaimsPrincipal user, string role = RoleNames.Admin)
        {
            Claim[] claims = role == null ? [] : [new Claim(ClaimTypes.Role, RoleNames.Admin)];
            user.AddIdentity(new ClaimsIdentity(claims, "TestAuth"));
        }
    }
}
