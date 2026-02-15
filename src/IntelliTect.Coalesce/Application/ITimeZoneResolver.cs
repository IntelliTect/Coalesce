using System;

namespace IntelliTect.Coalesce;

public interface ITimeZoneResolver
{
    TimeZoneInfo GetTimeZoneInfo();
}
