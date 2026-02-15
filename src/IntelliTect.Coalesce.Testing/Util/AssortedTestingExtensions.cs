using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

#nullable enable

namespace IntelliTect.Coalesce.Testing.Util;

public static class AssortedTestingExtensions
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

    public static async Task AssertMatched<TModel>(this IQueryable<TModel> query, bool shouldMatch)
        where TModel : class, new()
    {
        await Assert.That(query.Count()).IsEqualTo(shouldMatch ? 1 : 0);
    }

    public static async Task AssertOrder<TModel, TProp>(this IEnumerable<TModel> models, Func<TModel, TProp> propSelector, params TProp[] expectedOrder)
    {
        var modelsList = models.ToList();
        await Assert.That(modelsList.Count).IsEqualTo(expectedOrder.Length);

        foreach (var (model, expected) in modelsList.Zip(expectedOrder))
        {
            // Model not null because we asserted the length matches
            await Assert.That(propSelector(model!)).IsEqualTo(expected);
        }
    }

    public static async Task AssertOrder<TModel>(this IEnumerable<TModel> models, params TModel[] expectedOrder)
    {
        var modelsList = models.ToList();
        await Assert.That(modelsList.Count).IsEqualTo(expectedOrder.Length);

        foreach (var (model, expected) in modelsList.Zip(expectedOrder))
        {
            await Assert.That(model!.Equals(expected)).IsTrue();
        }
    }

    /// <summary>
    /// Asserts that the result was a failure.
    /// </summary>
    public static async Task AssertError(this ApiResult result)
    {
        await Assert.That(result.WasSuccessful).IsFalse();
    }

    /// <summary>
    /// Asserts that the result was a failure.
    /// </summary>
    public static async Task AssertError(this ApiResult result, string message)
    {
        await result.AssertError();
        await Assert.That(result.Message).IsEqualTo(message);
    }

    /// <summary>
    /// Asserts that the result was a failure.
    /// </summary>
    public static async Task AssertError<T>(this Task<T> resultTask, string message)
        where T : ApiResult
    {
        var result = await resultTask;
        await result.AssertError();
        await Assert.That(result.Message).IsEqualTo(message);
    }

    /// <summary>
    /// Asserts that the result was successful.
    /// </summary>
    public static async Task AssertSuccess(this ApiResult result, string? message = null)
    {
        // Returns a more useful assertion error than only checking WasSuccessful.
        await Assert.That(result.Message).IsEqualTo(message);
        await Assert.That(result.WasSuccessful).IsTrue();
    }

    /// <summary>
    /// Asserts that the result was successful.
    /// </summary>
    public static async Task<T> AssertSuccess<T>(this ItemResult<T> result)
    {
        await Assert.That(result.Message).IsNull();
        await Assert.That(result.WasSuccessful).IsTrue();
        return result.Object!;
    }

    /// <summary>
    /// Asserts that the result was successful.
    /// </summary>
    public static async Task<T> AssertSuccess<T>(this Task<ItemResult<T>> resultTask)
    {
        var result = await resultTask;
        await Assert.That(result.Message).IsNull();
        await Assert.That(result.WasSuccessful).IsTrue();
        return result.Object!;
    }

    /// <summary>
    /// Asserts that the result was successful.
    /// </summary>
    public static async Task AssertSuccess(this Task<ItemResult> resultTask)
    {
        var result = await resultTask;
        await Assert.That(result.WasSuccessful).IsTrue();
    }

    /// <summary>
    /// Asserts that the result was successful.
    /// </summary>
    public static async Task AssertSuccess<T>(this ItemResult<T> result, T value)
    {
        await result.AssertSuccess();
        await Assert.That(result.Object).IsEqualTo(value);
    }

    public static void LogIn(this ClaimsPrincipal user, string role = RoleNames.Admin)
    {
        Claim[] claims = role == null ? [] : [new Claim(ClaimTypes.Role, RoleNames.Admin)];
        user.AddIdentity(new ClaimsIdentity(claims, "TestAuth"));
    }
}
