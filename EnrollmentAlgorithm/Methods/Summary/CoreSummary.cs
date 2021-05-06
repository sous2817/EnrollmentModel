using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Wordprocessing;
using EnrollmentAlgorithm.Objects.Additional;
using MathNet.Numerics.Statistics;

namespace EnrollmentAlgorithm.Methods.Summary
{
    public abstract class CoreSummary
    {
        /// <summary>
        /// Calculates the probability of success.  Answers "How sure are you that we can get 10 sites through the SSU process by 31-Dec-2020 (or successfully enroll x subjects)?".
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="accrualTarget">The accrual target.</param>
        /// <param name="simulationValues">The simulation values.</param>
        /// <param name="accrualList">The accrual list.</param>
        /// <returns>System.Double.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Accrual value should should be less than total of the accrual type</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public double CalculateProbabilityOfSuccess(DateTime date, int accrualTarget, List<SimulationValues> simulationValues, Func<SimulationValues, SortedList<DateTime, int>> accrualList)
        {
            var matrixDateSpan = GetDateSpan(simulationValues);
            var accrualListBySimulation = simulationValues.ToDictionary(x => x, x => accrualList(x));
            var checkValue = simulationValues.Max(x => accrualListBySimulation[x].Values.Max());

            if (accrualTarget > checkValue)
            {
                throw new ArgumentOutOfRangeException(nameof(accrualTarget), "Accrual value should be less than/equal to {checkValue}");
            }

            if (date < matrixDateSpan.Start) return 0;
            if (date > matrixDateSpan.End) return 1;

            double percentChance = 0;

            var accrualValues = AccrualValues(date, simulationValues, accrualListBySimulation);

            for (var i = 0; i <= 100; i++)
            {
                var someValue = SortedArrayStatistics.Percentile(accrualValues, i);
                if (someValue < accrualTarget) continue;
                percentChance = Convert.ToDouble(100 - i) / 100;
                break;
            }

            return percentChance;
        }
        /// <summary>
        /// Gets accrual based on date and risk.  Essentially answers the questions "How many people can I enroll (or how many sites can I get up) by 14-Dec-2020 with 80% confidence?".
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="percentile">The percentile.</param>
        /// <param name="simulationValues">The simulation values.</param>
        /// <param name="accrualList">The accrual list.</param>
        /// <returns>System.Int32.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Could be that percentile is invalid (&lt; 0 or &gt; 100), date provided is outside of dates calculated by Monte Carlo,
        /// or supplied AccrualType is invalid.</exception>
        /// <exception cref="OverflowException">Return type is greater than <see cref="F:System.Int32.MaxValue" /> or less than <see cref="F:System.Int32.MinValue" />.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public int GetAccruedBasedOnDateAndRisk(DateTime date, double percentile, List<SimulationValues> simulationValues, Func<SimulationValues, SortedList<DateTime, int>> accrualList)
        {
            percentile *= 100;
            if (percentile < 0 || percentile > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(percentile), "Value should be greater than 0 or less than 1");
            }

            var matrixDateSpan = GetDateSpan(simulationValues);
            var accrualListBySimulation = simulationValues.ToDictionary(x => x, x => accrualList(x));

            if (date < matrixDateSpan.Start || date > matrixDateSpan.End)
            {
                throw new ArgumentOutOfRangeException(nameof(date), $"Value should be greater than {matrixDateSpan.Start} or less than/equal to {matrixDateSpan.End}");
            }

            var accrualValues = AccrualValues(date, simulationValues, accrualListBySimulation);

            return Convert.ToInt32(SortedArrayStatistics.Percentile(accrualValues, 100 - Convert.ToInt32(percentile)));
        }

        /// <summary>
        /// Gets the date based off risk.  If someone were to ask "When will my study complete enrollment (or get all my SIVs done) with 75% confidence?", this method will give you that answer.
        /// </summary>
        /// <param name="percentile">The percentile.</param>
        /// <param name="accrualTarget">The accrual target.</param>
        /// <param name="simulationValues">The simulation values.</param>
        /// <param name="accrualList">The accrual list.</param>
        /// <returns>DateTime.</returns>
        /// <exception cref="ArgumentOutOfRangeException"> Value should be greater than 0 or less than/equal to 100</exception>
        /// <exception cref="ArgumentOutOfRangeException">Percentile is outisde of expected range.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public DateTime GetDateBasedOffRisk(double percentile, int accrualTarget, List<SimulationValues> simulationValues, Func<SimulationValues, SortedList<DateTime, int>> accrualList)
        {
            percentile *= 100;
            if (percentile < 0 || percentile > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(percentile), "Value should be greater than 0 or less than/equal to 1");
            }

            var matrixDateSpan = GetDateSpan(simulationValues);
            var dateOfRisk = DateTime.MinValue;
            var accrualListBySimulation = simulationValues.ToDictionary(x => x, x => accrualList(x));
            var checkValue = simulationValues.Max(x => accrualListBySimulation[x].Values.Max());

            if (accrualTarget > checkValue)
            {
                throw new ArgumentOutOfRangeException(nameof(accrualTarget), $"Accrual value should should be <= {checkValue}");
            }
            
            for (var dateCounter = matrixDateSpan.Start; dateCounter <= matrixDateSpan.End; dateCounter = dateCounter.AddDays(1))
            {
                var counter = dateCounter;
                var accrualValues = AccrualValues(counter, simulationValues, accrualListBySimulation);
                var someValue = SortedArrayStatistics.Percentile(accrualValues, 100 - Convert.ToInt32(percentile));
                if (!(someValue >= accrualTarget)) continue;
                dateOfRisk = dateCounter;
                break;
            }
            return dateOfRisk;
        }

        public Dictionary<int,MeanAndErrorEstimatesDates> GetMeanAndErrorBasedOffRisk(int accrualTarget, List<SimulationValues> simulationValues, Func<SimulationValues, SortedList<DateTime, int>> accrualList)
        {
            var summaryList = new List<int> {1, 25, 50, 75, 100};

            var target = 0;
            var accrualListBySimulation = simulationValues.ToDictionary(x => x, x => accrualList(x));

            var meanAndErrorEstimateDatesDict = new Dictionary<int,MeanAndErrorEstimatesDates>();

            foreach (var value in summaryList)
            {
                switch (value)
                {
                    case 1:
                        target = 1;
                        break;
                    case 25:
                        target = (int)Math.Ceiling(accrualTarget * .25);
                        break;
                    case 50:
                        target = (int)Math.Ceiling(accrualTarget * .5);
                        break;
                    case 75:
                        target = (int)Math.Ceiling(accrualTarget * .75);
                        break;
                    case 100:
                        target = accrualTarget;
                        break;
                }

                var enrollmentDates = AccrualDates(target, simulationValues, accrualListBySimulation);
                var maxDate = enrollmentDates.Max();
                var dateDiffList = enrollmentDates.Select(date => maxDate.Subtract(date).Days).Select(val => (double)val).ToList();

                var averageEnrollment = enrollmentDates.AverageDate();
                var standardDev = (int)Math.Floor(dateDiffList.StandardDeviation());
                var meanAndErrorEstimateDates = new MeanAndErrorEstimatesDates
                {
                    MeanDate = averageEnrollment,
                    StdDevInDays = standardDev,
                    MinusOneStdDevDate = averageEnrollment.AddDays(-standardDev),
                    PlusOneStdDevDate = averageEnrollment.AddDays(standardDev)
                };
                meanAndErrorEstimateDatesDict.Add(value,meanAndErrorEstimateDates);
            }
            
            return meanAndErrorEstimateDatesDict;
        }


        /// <summary>
        /// Returns a dictionary of dates for reaching the accrual target using a list of percentiles.
        /// </summary>
        /// <param name="percentileList">The percentile list.</param>
        /// <param name="accrualTarget">The accrual target.</param>
        /// <param name="simulationValues">The simulation values.</param>
        /// <param name="accrualList">The accrual list.</param>
        /// <returns>Dictionary&lt;System.Double, DateTime&gt;.</returns>
        public Dictionary<double, DateTime> CalculateProbabilityOfSuccessDates(IEnumerable<double> percentileList, int accrualTarget, List<SimulationValues> simulationValues, Func<SimulationValues, SortedList<DateTime, int>> accrualList)
        {
            return percentileList.ToDictionary(percentile => percentile, percentile => GetDateBasedOffRisk(percentile, accrualTarget, simulationValues, accrualList));
        }
        
        /// <summary>
        /// Gets the projected accrual line values.  This returns accrual by day given a probability percentile.  This is useful if the user has a risk value (percentile) and wants to
        /// track actual accrual to the values needed in order to reach their target by some date.
        /// </summary>
        /// <param name="percentile">The percentile.</param>
        /// <param name="simulationValues">The simulation values.</param>
        /// <param name="accrualList">The accrual list.</param>
        /// <returns>List&lt;AccrualValuesByDate&gt;.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Value should be &gt; 0 or &lt; 100</exception>
        public List<AccrualValuesByDate> GetProjectedAccrualLineValues(double percentile, List<SimulationValues> simulationValues, Func<SimulationValues, SortedList<DateTime, int>> accrualList)
        {
            percentile *= 100;
            if (percentile < 0 || percentile > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(percentile), "Value should be > 0 or <= 100");
            }

            var accrualResult = new List<AccrualValuesByDate>();
            var matrixDateSpan = GetDateSpan(simulationValues);
            var accrualListBySimulation = simulationValues.ToDictionary(x => x, x => accrualList(x));
            var dayCounter = 0;

            for (var dateCounter = matrixDateSpan.Start; dateCounter <= matrixDateSpan.End; dateCounter = dateCounter.AddDays(1))
            {
                var counter = dateCounter;

                var accrualValues = AccrualValues(counter, simulationValues, accrualListBySimulation);

                accrualResult.Add(new AccrualValuesByDate
                {
                    AccrualDate = dateCounter,
                    AccrualDay = dayCounter,
                    AccrualValue = SortedArrayStatistics.Percentile(accrualValues, 100 - Convert.ToInt32(percentile))
                });
                dayCounter++;
            }

            return accrualResult;
        }

        private DateTime[] AccrualDates(int accrual, IEnumerable<SimulationValues> simulationValues,
            IDictionary<SimulationValues, SortedList<DateTime, int>> accrualListBySimulation)
        {
            return simulationValues.Select(x => accrualListBySimulation[x].FindKeyByValue(accrual)).ToArray();

        }
        
        private double[] AccrualValues(DateTime date, IEnumerable<SimulationValues> simulationValues, IDictionary<SimulationValues, SortedList<DateTime, int>> accrualListBySimulation)
        {
            var accrualValues = simulationValues.Select(x =>
            {
                var dateSpan = GetDateSpan(x);
                return
                    (double)
                    accrualListBySimulation[x].GetValueOrDefault(date,
                        () => date < dateSpan.Start ? 0 : accrualListBySimulation[x][dateSpan.End]);
            }).OrderBy(v => v).ToArray();
            return accrualValues;
        }
        
        protected abstract DateSpan GetDateSpan(List<SimulationValues> simulationValues);
        protected abstract DateSpan GetDateSpan(SimulationValues simulationValue);

    }
}
