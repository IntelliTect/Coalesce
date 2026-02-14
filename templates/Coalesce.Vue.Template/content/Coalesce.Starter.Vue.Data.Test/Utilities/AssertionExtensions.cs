using IntelliTect.Coalesce.Models;

namespace Coalesce.Starter.Vue.Data.Test;

public static class AssertionExtensions
{
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
    /// <param name="message">Expected error message.</param>
    public static async Task AssertError(this ApiResult result, string message)
    {
        await result.AssertError();
        await Assert.That(result.Message).IsEqualTo(message);
    }

    /// <summary>
    /// Asserts that the result was successful.
    /// </summary>
    public static async Task AssertSuccess(this ApiResult result, string? message = null)
    {
        await Assert.That(result.WasSuccessful)
            .IsTrue()
            .WithMessage(() => result.Message ?? "");
        await Assert.That(result.Message).IsEqualTo(message);
    }

    /// <summary>
    /// Asserts that the result was successful.
    /// </summary>
    public static async Task<T> AssertSuccess<T>(this ItemResult<T> result)
    {
        await Assert.That(result.WasSuccessful)
            .IsTrue()
            .WithMessage(() => result.Message ?? "");
        await Assert.That(result.Message).IsNull();
        return result.Object ?? throw new ArgumentException("Successful result unexpectedly returned null object");
    }

    /// <summary>
    /// Asserts that the result was successful.
    /// </summary>
    public static async Task<T> AssertSuccess<T>(this Task<ItemResult<T>> resultTask)
    {
        var result = await resultTask;
        return await result.AssertSuccess();
    }

    /// <summary>
    /// Asserts that the result was successful.
    /// </summary>
    public static async Task AssertSuccess(this Task<ItemResult> resultTask)
    {
        var result = await resultTask;
        await Assert.That(result.WasSuccessful)
            .IsTrue()
            .WithMessage(() => result.Message ?? "");
    }

    /// <summary>
    /// Asserts that the result was successful.
    /// </summary>
    /// <param name="expectedValue">Expected value on the result.</param>
    public static async Task AssertSuccess<T>(this ItemResult<T> result, T expectedValue)
    {
        await result.AssertSuccess();
        await Assert.That(result.Object).IsEqualTo(expectedValue);
    }
}
