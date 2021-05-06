using System;

namespace EnrollmentAlgorithm.Objects.Additional
{
    public class SummarizedSSUResults
    {
        public DateTime AccrualDate { get; set; }
        public Percentiles SIVPercentiles { get; set; }
        public Percentiles SSVPercentiles { get; set; }
        public MeanAndErrorEstimates SIVMeanAndStdDev { get; set; }
        public MeanAndErrorEstimates SSVMeanAndStdDev { get; set; }
        public double[] RawSIVValues { get; set; }
        public double[] RawSSVValues { get; set; }
    }
}
