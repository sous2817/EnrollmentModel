using System;

namespace EnrollmentAlgorithm.Objects.Additional
{
    public class SummarizedAccrualResults
    {
        public DateTime AccrualDate { get; set; }
        public Percentiles ScreeningPercentiles { get; set; }
        public Percentiles EnrollmentPercentiles { get; set; }
        public Percentiles RandomizationPercentiles { get; set; }
        public MeanAndErrorEstimates ScreeningMeanAndStdDev { get; set; }
        public MeanAndErrorEstimates EnrollmentMeanAndStdDev { get; set; }
        public MeanAndErrorEstimates RandomizationMeanAndStdDev { get; set; }
        public double[] RawScreeningValues { get; set; }

        public double[] RawEnrollmentValues { get; set; }
        public double[] RawRandomizationValues { get; set; }
    }
}
