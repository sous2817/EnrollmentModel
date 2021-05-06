using System.Collections.Generic;

namespace EnrollmentAlgorithm.Objects.Additional
{
    public class AccruedValues
    {
        private List<PatientAccrualInformation> _patientAccrualInformation;

        public List<PatientAccrualInformation> PatientAccrualInformation
        {
            get {return _patientAccrualInformation ?? (_patientAccrualInformation = new List<PatientAccrualInformation>());}
            set { _patientAccrualInformation = value; }
        }
        public int EnrollmentCounter { get; set; }
    }
}
