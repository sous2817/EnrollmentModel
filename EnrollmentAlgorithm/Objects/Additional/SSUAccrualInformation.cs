using System;

namespace EnrollmentAlgorithm.Objects.Additional
{
    public class SSUAccrualInformation : BaseAccrualInformation
    {
        public double SSUTime { get; set; }
        public DateTime SIVDate { get; set; }
        public DateTime SSVDate { get; set; }
    }
}
