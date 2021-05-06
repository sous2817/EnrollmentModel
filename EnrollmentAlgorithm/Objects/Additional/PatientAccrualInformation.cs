using System;

namespace EnrollmentAlgorithm.Objects.Additional
{
    public class PatientAccrualInformation : BaseAccrualInformation
    {
        public DateTime ScreeningDate { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public DateTime RandomizedDate { get; set; }
        public int ScreeningDay { get; set; }
        public int EnrollmentDay { get; set; }
        public int RandomizedDay { get; set; }
    }
}
