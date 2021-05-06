using System.Collections.Generic;

namespace EnrollmentAlgorithmTests.ExtensionMethods
{
    public static class ExtensionMethods
    {
        public static double Unanimous(this IEnumerable<double> sequence, double other)
        {
            double? first = null;
            foreach (var item in sequence)
            {
                if (first == null)
                    first = item;
                else if (first.Value != item)
                    return other;
            }
            return first ?? other;
        }
    }
}
