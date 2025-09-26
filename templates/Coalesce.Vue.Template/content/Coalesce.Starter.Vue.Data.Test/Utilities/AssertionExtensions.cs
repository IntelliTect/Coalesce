using IntelliTect.Coalesce.Models;

namespace Coalesce.Starter.Vue.Data.Test;

public static class AssertionExtensions
{
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
    /// <param name="message">Expected error message.</param>
    public static void AssertError(this ApiResult result, string message)
    {
        result.AssertError();
        Assert.Equal(message, result.Message);
    }

    /// <summary>
    /// Asserts that the result was successful.
    /// </summary>
    public static void AssertSuccess(this ApiResult result, string? message = null)
    {
        Assert.True(result.WasSuccessful, result.Message);
        Assert.Equal(message, result.Message);
    }

    /// <summary>
    /// Asserts that the result was successful.
    /// </summary>
    public static T AssertSuccess<T>(this ItemResult<T> result)
    {
        Assert.True(result.WasSuccessful, result.Message);
        Assert.Null(result.Message);
        return result.Object ?? throw new ArgumentException("Successful result unexpectedly returned null object");
    }

    /// <summary>
    /// Asserts that the result was successful.
    /// </summary>
    public static async Task<T> AssertSuccess<T>(this Task<ItemResult<T>> resultTask)
    {
        var result = await resultTask;
        return result.AssertSuccess();
    }

    /// <summary>
    /// Asserts that the result was successful.
    /// </summary>
    public static async Task AssertSuccess(this Task<ItemResult> resultTask)
    {
        var result = await resultTask;
        Assert.True(result.WasSuccessful, result.Message);
    }

    /// <summary>
    /// Asserts that the result was successful.
    /// </summary>
    /// <param name="expectedValue">Expected value on the result.</param>
    public static void AssertSuccess<T>(this ItemResult<T> result, T expectedValue)
    {
        result.AssertSuccess();
        Assert.Equal(expectedValue, result.Object);
    }
}
