using System.Collections.Generic;

namespace EnrollmentAlgorithm.Objects.Enrollment
{
    public class TrialParameter : BaseEnrollmentObject
    {
        public double Probability { get; set; }
        public double Confidence { get; set; }
        public List<CountryParameter> CountryList { get; set; }
        public int EnrollmentTarget { get; set; }
        public int EstimatedLengthOfEnrollment { get; set; }
        public int NumberOfIterations { get; set; }
        public int SimulationSeed { get; set; }
        public double DropoutRate { get; set; }
        public int ScreeningPeriodLowerBound { get; set; }
        public int ScreeningPeriodUpperBound { get; set; }
    }
}
