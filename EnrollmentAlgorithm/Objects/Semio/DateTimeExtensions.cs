using System;

namespace Semio.Core.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Determines whether this is the last day of the month
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static bool IsEndOfMonth(this DateTime date) => date.Day == DateTime.DaysInMonth(date.Year, date.Month);

        /// <summary>
        /// Get the last day of the month for the date
        /// </summary>
        /// <param name="date"></param>
        /// <returns>A new DateTime object set to the Last day of the month for the current date. Leap year is accounted for.</returns>
        public static DateTime EndOfMonth(this DateTime date) => new DateTime(
    date.Year,
    date.Month,
    DateTime.DaysInMonth(date.Year, date.Month)
);

        /// <summary>
        /// Get the first day of the month for the date
        /// </summary>
        /// <param name="date"></param>
        /// <returns>A new DateTime object set to the 1st of the month for the current date.</returns>
        public static DateTime FirstOfMonth(this DateTime date) => new DateTime(
   date.Year,
   date.Month,
   1
);

        /// <summary>
        /// A flag that indicates whether or not another date is in the same Month and Year as the current date.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="otherDate">The other date to compare Year and Month</param>
        /// <returns>A True if the year and month are equivilant. A false if they are not.</returns>
        public static bool IsInSameMonth(this DateTime date, DateTime otherDate) => date.Year == otherDate.Year && date.Month == otherDate.Month;

        /// <summary>
        /// Determines whether two dates are approximately equal.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="otherDate"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        public static bool IsSameApproximateTime(this DateTime date, DateTime otherDate, TimePrecision precision = TimePrecision.Minute)
        {
            if (date.Date != otherDate.Date)
                return false;

            switch (precision)
            {
                case TimePrecision.Hour:
                    return (int)date.TimeOfDay.TotalHours == (int)otherDate.TimeOfDay.TotalHours;
                case TimePrecision.Minute:
                    return (int)date.TimeOfDay.TotalMinutes == (int)otherDate.TimeOfDay.TotalMinutes;
                case TimePrecision.Second:
                    return (int)date.TimeOfDay.TotalSeconds == (int)otherDate.TimeOfDay.TotalSeconds;
            }

            return true;
        }

        /// <summary>
        /// Determines if this date is on or before the passed in date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="otherDate">The other date.</param>
        /// <returns></returns>
        public static bool DateOnOrBefore(this DateTime date, DateTime otherDate)
        {
            if (date.Year < otherDate.Year)
                return true;

            if (date.Year == otherDate.Year && date.Month < otherDate.Month)
                return true;

            if (date.Year == otherDate.Year && date.Month == otherDate.Month && date.Day <= otherDate.Day)
                return true;

            return false;
        }

        /// <summary>
        /// Determines if this date is before passed in date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="otherDate">The other date.</param>
        /// <returns></returns>
        public static bool DateEarlierThan(this DateTime date, DateTime otherDate)
        {
            if (date.Year < otherDate.Year)
                return true;

            if (date.Year == otherDate.Year && date.Month < otherDate.Month)
                return true;

            if (date.Year == otherDate.Year && date.Month == otherDate.Month && date.Day < otherDate.Day)
                return true;

            return false;
        }

        /// <summary>
        /// Determines if this date occurs after passed in date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="otherDate">The other date.</param>
        /// <returns></returns>
        public static bool DateAfter(this DateTime date, DateTime otherDate)
        {
            if (date.Year > otherDate.Year)
                return true;

            if (date.Year == otherDate.Year && date.Month > otherDate.Month)
                return true;

            return false;
        }

        /// <summary>
        /// Returns the Date part of the value with a DateTimeKind of Unspecified
        /// </summary>
        /// <param name="value">Value to get the Unspecified Date of</param>
        /// <returns></returns>
        public static DateTime? UnspecifiedDate(this DateTime? value) => value.HasValue
    ? DateTime.SpecifyKind(value.Value.Date, DateTimeKind.Unspecified)
    : (DateTime?)null;

        /// <summary>
        /// Returns the Date part of the value with a DateTimeKind of Unspecified
        /// </summary>
        /// <param name="value">Value to get the Unspecified Date of</param>
        /// <returns></returns>
        public static DateTime UnspecifiedDate(this DateTime value) => DateTime.SpecifyKind(value.Date, DateTimeKind.Unspecified);
    }

    public enum TimePrecision
    {
    	Hour,
        Minute,
        Second,
    }
}