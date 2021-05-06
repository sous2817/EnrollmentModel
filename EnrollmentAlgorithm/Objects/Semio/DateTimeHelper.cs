using System;

namespace Semio.Core.Helpers
{
    /// <summary>
    /// Helper container for DateTime utilities.
    /// </summary>
    public static class DateTimeHelper
    {
        /// <summary>
        ///
        /// </summary>
        public const string DateTimeFormat = "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'";

        /// <summary>
        ///
        /// </summary>
        public const string DateTimeDefault = "Mon, 01 Jan 0001 00:00:00 GMT";

        /// <summary>
        ///
        /// </summary>
        public const string DurationDefault = "PT0S";
    }

    public static class SystemTime
    {
        public static Func<DateTime> Now = () => DateTime.Now;
        public static Func<DateTime> UtcNow = () => DateTime.UtcNow;
    }
}