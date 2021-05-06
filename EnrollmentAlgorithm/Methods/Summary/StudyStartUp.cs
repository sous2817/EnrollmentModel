using System.Collections.Generic;
using System.Linq;
using EnrollmentAlgorithm.Objects.Additional;

namespace EnrollmentAlgorithm.Methods.Summary
{
    public class StudyStartUp : CoreSummary
    {
        protected override DateSpan GetDateSpan(List<SimulationValues> simulationValues)
        {
            return new DateSpan
            {
                Start = simulationValues.Min(x=>x.EarliestSSVDate),
                End = simulationValues.Max(x=>x.LatestSIVDate)
            };
        }
        protected override DateSpan GetDateSpan(SimulationValues simulationValue)
        {
            return new DateSpan
            {
                Start = simulationValue.EarliestSSVDate,
                End = simulationValue.LatestSIVDate
            };
        }
    }
}