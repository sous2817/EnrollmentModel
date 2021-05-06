namespace Semio.ClinWeb.Common.Constants
{
    public static class Formats
    {
        /// <summary>
        /// Standard date format: dd-MMM-yyyy
        /// </summary>
        public const string DATE = "dd-MMM-yyyy";

        /// <summary>
        /// Standard time format: h:mm tt
        /// </summary>
        public const string TIME = "h:mm tt";

        /// <summary>
        /// Standard date and time format: dd-MMM-yyyy h:mm tt
        /// </summary>
        public const string DATE_TIME = DATE + " " + TIME;

        /// <summary>
        /// Standard number format: 12,345.67
        /// </summary>
        public const string NUMBER = "N";
    }
}