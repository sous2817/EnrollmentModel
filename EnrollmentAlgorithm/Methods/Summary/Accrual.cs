using System;
using System.Collections.Generic;
using System.Linq;
using EnrollmentAlgorithm.Objects.Additional;
using MathNet.Numerics.Statistics;

namespace EnrollmentAlgorithm.Methods.Summary
{
    public class Accrual : CoreSummary
    {
        protected override DateSpan GetDateSpan(List<SimulationValues> simulationValues)
        {
            return new DateSpan
            {
                Start = simulationValues.Min(x => x.EarliestAccrualDate),
                End = simulationValues.Max(x => x.LatestAccrualDate)
            };
        }

        protected override DateSpan GetDateSpan(SimulationValues simulationValue)
        {
            return new DateSpan
            {
                Start = simulationValue.EarliestAccrualDate,
                End = simulationValue.LatestAccrualDate
            }; 
        }
    }
}
