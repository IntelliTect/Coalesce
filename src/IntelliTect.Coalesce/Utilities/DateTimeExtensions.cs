using System;
using System.Globalization;
using TimeZoneNames;

namespace IntelliTect.Coalesce.Utilities
{
    public static class DateTimeExtensions
    {
        public const string TimeZoneEST = "Eastern Standard Time";
        public const string TimeZonePST = "Pacific Standard Time";
        public const string TimeZoneMST = "Mountain Standard Time";
        public const string TimeZoneCST = "Central Standard Time";
        public const string TimeZoneArizona = "US Mountain Standard Time";
        public const string TimeZoneHawaii = "Hawaiian Standard Time";
        public const string TimeZoneAST = "Alaskan Standard Time";


        public static bool IsNowEDT => TimeZoneInfo.FindSystemTimeZoneById(TimeZoneEST)
            .IsDaylightSavingTime(DateTime.UtcNow.ToLocalTime(TimeZoneEST));

        public static bool IsNowPDT => TimeZoneInfo.FindSystemTimeZoneById(TimeZonePST)
            .IsDaylightSavingTime(DateTime.UtcNow.ToLocalTime(TimeZonePST));


        public static bool IsNowCDT => TimeZoneInfo.FindSystemTimeZoneById(TimeZoneCST)
            .IsDaylightSavingTime(DateTime.UtcNow.ToLocalTime(TimeZoneCST));

        public static bool IsNowMDT => TimeZoneInfo.FindSystemTimeZoneById(TimeZoneMST)
            .IsDaylightSavingTime(DateTime.UtcNow.ToLocalTime(TimeZoneMST));

        public static bool IsNowADT => TimeZoneInfo.FindSystemTimeZoneById(TimeZoneAST)
            .IsDaylightSavingTime(DateTime.UtcNow.ToLocalTime(TimeZoneAST));

        /// <summary>
        ///     Rounds up to the nearest minutes sent in
        /// </summary>
        /// <param name="input"></param>
        /// <param name="minutes"></param>
        /// <returns></returns>
        public static DateTimeOffset TimeRoundUp(this DateTimeOffset input, int minutes)
        {
            return new DateTimeOffset(input.Year, input.Month, input.Day, input.Hour, input.Minute, 0, input.Offset)
                .AddMinutes(input.Minute % minutes == 0 ? 0 : minutes - input.Minute % minutes);
        }

        /// <summary>
        ///     Rounds down to the nearest minutes sent in
        /// </summary>
        /// <param name="input"></param>
        /// <param name="minutes"></param>
        /// <returns></returns>
        public static DateTimeOffset TimeRoundDown(this DateTimeOffset input, int minutes)
        {
            return new DateTimeOffset(input.Year, input.Month, input.Day, input.Hour, input.Minute, 0, input.Offset)
                .AddMinutes(-input.Minute % minutes);
        }


        public static string UtcTimeToPacificStandardTimeAsString(this DateTime time)
        {
            var pacificStandardTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

            var pacificStandardTime =
                TimeZoneInfo.ConvertTimeFromUtc(time, pacificStandardTimeZone);
            var abbreviation = pacificStandardTimeZone.IsDaylightSavingTime(time)
                ? pacificStandardTimeZone.DaylightName
                : pacificStandardTimeZone.StandardName;
            var ret = pacificStandardTime.ToString("MMMM dd, yyyy   hh:mm tt", CultureInfo.CurrentCulture)
                      + " " + abbreviation;
            return ret;
        }

        public static int AgeAtDate(this DateTime birthdate, DateTime dateOfInterest)
        {
            var age = dateOfInterest.Year - birthdate.Year;
            if (dateOfInterest < birthdate.AddYears(age)) age--;
            return age;
        }


        public static string GetNiceDisplay(this DateTime time, string timeZoneId,
            string format = "MMMM dd, yyyy hh:mm tt", bool showTimeZoneAbbrev = true)
        {
            var ret = time.ToLocalTime(timeZoneId).ToString(format, CultureInfo.InvariantCulture);
            if (showTimeZoneAbbrev)
                ret += " " + time.TimeZoneAbbreviation(timeZoneId);
            return ret;
        }

        public static string TimeZoneAbbreviation(this string longTimeZone)
        {
            var words = longTimeZone.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var abbreviatedTimeZone = string.Empty;
            foreach (var word in words)
                abbreviatedTimeZone += word.ToUpper()[0];
            return abbreviatedTimeZone;
        }

        public static string TimeZoneAbbreviation(this DateTime time, string timeZoneId)
        {
            var tzName = TZNames.GetAbbreviationsForTimeZone(timeZoneId, "en-US");
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var timeZoneName = timeZone.IsDaylightSavingTime(time)
                ? tzName.Daylight
                : tzName.Standard;

            return timeZoneName;
        }

        public static DateTime ToLocalTime(this DateTime utcDateTime, TimeZoneInfo timeZoneInfo)
        {
            var date = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeFromUtc(date, timeZoneInfo);
        }

        public static DateTime ToLocalTime(this DateTime utcDateTime, string timeZoneId)
        {
            var date = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTimeFromUtc(date, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
        }

        public static DateTime ToUniversalTime(this DateTime localDateTime, TimeZoneInfo timeZoneInfo)
        {
            var date = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(date, timeZoneInfo);
        }

        public static DateTime ToUniversalTime(this DateTime localDateTime, string timeZoneId)
        {
            if (string.IsNullOrWhiteSpace(timeZoneId))
                throw new ArgumentNullException("timeZoneId");
            var date = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(date, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
        }

        public static DateTimeOffset FromJSDateToDateTimeOffset(this double milliseconds)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(milliseconds);
            return new DateTimeOffset(epoch);
        }

        public static string TimeZoneId(int? timeZoneOffset, bool? observeDaylightSavingsTime)
        {
            if (timeZoneOffset == null || observeDaylightSavingsTime == null)
                return "";
            var prefix = TimeZonePrefix(timeZoneOffset, observeDaylightSavingsTime);
            if (prefix == "Arizona")
                return TimeZoneArizona;
            if (prefix == "Hawaii")
                return TimeZoneHawaii;
            return prefix + " Standard Time";
        }
        /// <summary>
        /// Helpful when trying to create a date on UTC server correctly
        /// </summary>
        /// <param name="timeZoneId"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="hour"></param>
        /// <param name="minutes"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static DateTimeOffset CreateTimeForTimeZone(this DateTimeOffset dateTimeOffset, string timeZoneId, int year, int month, int day, int hour, 
            int minutes, int seconds)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTimeOffset ret = new DateTimeOffset(year, month, day, hour,
                minutes, seconds, tz.GetUtcOffset(dateTimeOffset));
            return ret;
        }

        public static string TimeZonePrefix(int? timeZoneOffset, bool? observeDaylightSavingsTime)
        {
            if (timeZoneOffset == null || observeDaylightSavingsTime == null)
                return "";
            if (
                timeZoneOffset == 240 && observeDaylightSavingsTime == true && IsNowEDT ||
                timeZoneOffset == 300 && observeDaylightSavingsTime == true && !IsNowEDT)
                return "Eastern";
            if (
                timeZoneOffset == 300 && observeDaylightSavingsTime == true && IsNowCDT ||
                timeZoneOffset == 360 && observeDaylightSavingsTime == true && !IsNowCDT
            )
                return "Central";
            if (
                timeZoneOffset == 360 && observeDaylightSavingsTime == true && IsNowMDT ||
                timeZoneOffset == 420 && observeDaylightSavingsTime == true && !IsNowMDT
            )
                return "Mountain";
            if (
                timeZoneOffset == 420 && observeDaylightSavingsTime == true && IsNowPDT ||
                timeZoneOffset == 480 && observeDaylightSavingsTime == true && !IsNowPDT
            )
                return "Pacific";
            if (
                timeZoneOffset == 480 && observeDaylightSavingsTime == true && IsNowADT ||
                timeZoneOffset == 540 && observeDaylightSavingsTime == true && !IsNowADT
            )
                return "Alaskan";

            if (timeZoneOffset == 600 && observeDaylightSavingsTime == false)
                return "Hawaii";
            if (timeZoneOffset == 420 && observeDaylightSavingsTime == false)
                return "Arizona";

            return "";
        }
    }
}