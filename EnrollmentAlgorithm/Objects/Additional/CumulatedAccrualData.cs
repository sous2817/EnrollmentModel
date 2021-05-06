using System;
using System.Collections.Generic;

namespace EnrollmentAlgorithm.Objects.Additional
{
    public class CumulatedAccrualData
    {
        private SortedList<DateTime, int> _cumulatedScreened;
        private SortedList<DateTime, int> _cumulatedEnrolled;
        private SortedList<DateTime, int> _cumulatedRandomized;

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
    }
}
