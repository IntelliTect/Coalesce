using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Api.Controllers;

public static class ControllerHelpers

{
    public static byte[] ReadAllBytes(this Stream stream, bool closeUnderlying = false)
    {
        if (stream is MemoryStream memoryStream)
        {
            return memoryStream.ToArray();
        }

        using (memoryStream = new MemoryStream())
        {
            stream.CopyTo(memoryStream);
            if (closeUnderlying) stream.Dispose();
            return memoryStream.ToArray();
        }
    }

    public static async Task<byte[]> ReadAllBytesAsync(this Stream stream, bool closeUnderlying = false)
    {
        if (stream is MemoryStream memoryStream)
        {
            return memoryStream.ToArray();
        }

        using (memoryStream = new MemoryStream())
        {
            await stream.CopyToAsync(memoryStream);
            if (closeUnderlying) stream.Dispose();
            return memoryStream.ToArray();
        }
    }

    /// <summary>
    /// Returns true if there was any attempt to pass in any value 
    /// for the properties on the object named by <paramref name="prefix"/>. 
    /// Returns false if the only key is <paramref name="prefix"/> itself with an empty value,
    /// which is how null objects are serialized to formdata by coalesce-vue.
    /// </summary>
    public static bool HasAnyValue(this IFormCollection? form, string prefix)
    {
        if (form is null) return false;

        var hasValue = form.Keys.Any(k => k.StartsWith(prefix + "[") || k.StartsWith(prefix + "."));

        if (form[prefix] is [""] && !hasValue)
        {
            return false;
        }
        return true;
    }
}
