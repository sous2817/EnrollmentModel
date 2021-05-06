using System.Collections.Generic;
using EnrollmentAlgorithm.Objects.Enums;

namespace EnrollmentAlgorithm.Objects.Enrollment
{
    public class CountryParameter : BaseEnrollmentObject
    {
        public List<SiteParameter> SiteParameters { get; set; }
        public EnrollmentAccrualConstraint AccrualConstraint { get; set; }
    }
}
