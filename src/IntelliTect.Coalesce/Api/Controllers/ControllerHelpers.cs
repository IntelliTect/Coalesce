using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Api.Controllers
{
    public static class ControllerHelpers

    {
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
    }
}
