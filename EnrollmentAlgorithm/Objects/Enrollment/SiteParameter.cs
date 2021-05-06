using System;
using EnrollmentAlgorithm.Objects.Enums;

namespace EnrollmentAlgorithm.Objects.Enrollment
{
    public class SiteParameter : BaseEnrollmentObject
    {
        public double ConditionalMean { get; set; }
        public double ConditionalVariance { get; set; }
        public DateTime? SiteInitiationVisit { get; set; }
        public DateTime? SiteSelectionVisit { get; set; }
        public string SiteStatus { get; set; }
        public DistributionType DistributionType { get; set; }
        public string CountryName { get; set; }
    }
}
