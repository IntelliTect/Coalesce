using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTect.Coalesce
{
    public interface ITimeZoneResolver
    {
        TimeZoneInfo GetTimeZoneInfo();
    }
}
