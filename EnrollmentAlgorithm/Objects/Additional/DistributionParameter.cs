using EnrollmentAlgorithm.Objects.Enums;
using MathNet.Numerics.Distributions;

namespace EnrollmentAlgorithm.Objects.Additional
{
    public class DistributionParameter
    {
        public IContinuousDistribution Distribution { get; set; }
        public double Alpha { get; set; }
        public double Rate { get; set; }
        public double LowerBound { get; set; }
        public double UpperBound { get; set; }
        public double Mean { get; set; }
        public double StandardDeviation { get; set; }
        public DistributionType Type { get; set; }
    }
}
