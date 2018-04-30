using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTect.Coalesce
{
    internal class StaticTimeZoneResolver : ITimeZoneResolver
    {
        public StaticTimeZoneResolver(TimeZoneInfo timeZoneInfo) {
            TimeZoneInfo = timeZoneInfo;
        }

        public TimeZoneInfo TimeZoneInfo { get; }

        public TimeZoneInfo GetTimeZoneInfo() => TimeZoneInfo;
    }
}
