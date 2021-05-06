using System;
using System.Collections.Generic;

namespace EnrollmentAlgorithm.Objects.Additional
{
    public class SimulationValues
    {
        private List<PatientAccrualInformation> _patientAccrual;
        private List<SSUAccrualInformation> _ssuAccrual;
        private SortedList<DateTime, int> _cumulatedScreened;
        private SortedList<DateTime, int> _cumulatedEnrolled;
        private SortedList<DateTime, int> _cumulatedRandomized;
        private SortedList<DateTime, int> _cumulatedSIV;
        private SortedList<DateTime, int> _cumulatedSSV;
        public double SSUValue { get; set; }
        public DateTime SIVDate { get; set; }
        public DateTime SSVDate { get; set; }
        public double InitialScreeningRate { get; set; }
        
        public int[] ScreeningValues { get; set; }

        public List<PatientAccrualInformation> PatientAccrual
        {
            get { return _patientAccrual ?? (_patientAccrual = new List<PatientAccrualInformation>()); }
            set { _patientAccrual = value; }
        }

        public List<SSUAccrualInformation> SSUAccrual
        {
            get { return _ssuAccrual ?? (_ssuAccrual = new List<SSUAccrualInformation>()); }
            set { _ssuAccrual = value; }
        }

        public SortedList<DateTime, int> CumulatedScreened
        {
            get { return _cumulatedScreened ?? (_cumulatedScreened = new SortedList<DateTime, int>()); }
            set { _cumulatedScreened = value; }
        }

        public SortedList<DateTime, int> CumulatedEnrolled
        {
            get { return _cumulatedEnrolled ?? (_cumulatedEnrolled = new SortedList<DateTime, int>()); }
            set { _cumulatedEnrolled = value; }
        }

        public SortedList<DateTime, int> CumulatedRandomized
        {
            get { return _cumulatedRandomized ?? (_cumulatedRandomized = new SortedList<DateTime, int>()); }
            set { _cumulatedRandomized = value; }
        }

        public SortedList<DateTime, int> CumulatedSIV
        {
            get { return _cumulatedSIV ?? (_cumulatedSIV = new SortedList<DateTime, int>()); }
            set { _cumulatedSIV = value; }
        }

        public SortedList<DateTime, int> CumulatedSSV
        {
            get { return _cumulatedSSV ?? (_cumulatedSSV = new SortedList<DateTime, int>()); }
            set { _cumulatedSSV = value; }
        }
        
        public DateTime EarliestAccrualDate { get; set; }
        public DateTime LatestAccrualDate { get; set; }
        public DateTime EarliestSIVDate { get; set; }
        public DateTime LatestSIVDate { get; set; }
        public DateTime EarliestSSVDate { get; set; }
        public DateTime LatestSSVDate { get; set; }

    }
}
