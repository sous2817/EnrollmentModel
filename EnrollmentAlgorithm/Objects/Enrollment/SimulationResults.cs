using System;
using System.Collections.Generic;
using EnrollmentAlgorithm.Objects.Additional;

namespace EnrollmentAlgorithm.Objects.Enrollment
{    public class SimulationResults
    {
        private List<SimulationValues> _simulationValuesList;
        public List<SimulationValues> SimulationValuesList
        {
            get { return _simulationValuesList ?? (_simulationValuesList = new List<SimulationValues>()); }
            set { _simulationValuesList = value; }
        }

        public double AverageSSUTime { get; set; }
        public DateTime AverageSIVDate { get; set; }
    }
}
