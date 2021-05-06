using System;
using System.Collections.Generic;
using EnrollmentAlgorithm.Objects.Additional;

namespace EnrollmentAlgorithm.Objects.Enrollment
{
    public class BaseEnrollmentObject
    {
        private SimulationResults _resultsOfSimulation;
        private List<SummarizedAccrualResults> _accrualSummary;
        private List<SummarizedSSUResults> _ssuSummary;
        public int CurrentActualEnrollment { get; set; }
        public DateTime TargetEnrollmentStartDate { get; set; }
        public List<EnrollmentBreakParameter> EnrollmentBreakParameter { get; set; }
        public double ScreenFailRate { get; set; }
        public double SIVDelay { get; set; }
        public DateTime StudyStartDate { get; set; }
        public DistributionParameter BaselineScreeningDistribution { get; set; }
        public DistributionParameter BaselineEnrollmentDistribution { get; set; }
        public DistributionParameter BaselineSSUDistribution { get; set; }
        public string Name { get; set; }
        public int MaxPatientEnrollment { get; set; }
        public int MinPatientEnrollment { get; set; }
        public DistributionParameter ReprojectionScreeningDistribution { get; set; }
        public DistributionParameter ReprojectionEnrollmentDistribution { get; set; }
        public DistributionParameter ReprojectionSSUDistribution { get; set; }
        public DateTime EnrollmentStopDate { get; set; }
        public DateTime EnrollmentStartDate { get; set; }

        public List<SummarizedAccrualResults> AccrualSummary
        {
            get { return _accrualSummary ?? (_accrualSummary = new List<SummarizedAccrualResults>()); }
            set { _accrualSummary = value; }
        }

        public List<SummarizedSSUResults> SSUSummary
        {
            get { return _ssuSummary ?? (_ssuSummary = new List<SummarizedSSUResults>()); }
            set { _ssuSummary = value; }
        }

        public SimulationResults ResultsOfSimulation
        {
            get { return _resultsOfSimulation ?? (_resultsOfSimulation = new SimulationResults()); }
            set { _resultsOfSimulation  = value; }
        }
    }
}


