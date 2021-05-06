using System;
using System.Collections.Generic;
using EnrollmentAlgorithm.Objects.Additional;
using EnrollmentAlgorithm.Objects.Enrollment;
using EnrollmentAlgorithm.Methods;

namespace EnrollmentAlgorithm
{
    public class NumericalIntegration
    {
        public static ClosedFormApproximation UniformAnalytical(IEnumerable<SiteParameter> siteList, int time, int avgScreeningTime)
        {
            var globalApproximation = new ClosedFormApproximation();

            //ToDo: Not sure I need this...
            var adjustedTime = Math.Max(0, time - avgScreeningTime);

            foreach (var site in siteList)
            {
                var ssuLowerBound = site.BaselineSSUDistribution.LowerBound;
                var ssuUpperBound = site.BaselineSSUDistribution.UpperBound;
                var siteScreeningMean = site.BaselineScreeningDistribution.Mean;
                var siteScreeningVariance = site.BaselineScreeningDistribution.Distribution.Variance;

                var opportunityToEnroll = HelperMethods.FPRConstraintAndClosureCheck(time, site);

                //ToDo: Wrap head around why this is needed
                var someTimeValue = opportunityToEnroll ? time : site.EnrollmentStopDate.Subtract(site.EnrollmentStartDate).Days;

                var conditionalValues = CalculateConditionalValues(someTimeValue, ssuLowerBound, ssuUpperBound);

                var siteDailyMean = conditionalValues.ConditionalSSU*siteScreeningMean*
                               (time - conditionalValues.ConditionalMean);
                var siteDailyVariance = conditionalValues.ConditionalSSU*
                                        (siteScreeningMean*(time - conditionalValues.ConditionalMean) +
                                         siteScreeningVariance*Math.Pow((time - conditionalValues.ConditionalMean), 2) +
                                         site.ConditionalVariance*
                                         (siteScreeningVariance + Math.Pow(siteScreeningMean, 2))) +
                                        conditionalValues.ConditionalSSU*(1 - conditionalValues.ConditionalSSU)*
                                        Math.Pow(siteScreeningMean*(time - site.ConditionalMean), 2);

                globalApproximation.GlobalAccrualMean += siteDailyMean;
                globalApproximation.GlobalAccrualVariance += siteDailyVariance;
            }
            return globalApproximation;
        }

        private static ConditionalValues CalculateConditionalValues(int time, double ssuLowerBound, double ssuUpperBound)
        {
            var conditionalValues = new ConditionalValues();

            // no chance of site being open
            if (time < ssuLowerBound) return conditionalValues;

            // some chance of site being open
            if (time > ssuLowerBound && time < ssuUpperBound)
            {
                conditionalValues.ConditionalSSU = (time - ssuLowerBound) /
                             (ssuUpperBound - ssuLowerBound);
                conditionalValues.ConditionalMean = (Math.Pow(time, 2) - Math.Pow(ssuLowerBound, 2)) / (2 * (time - ssuLowerBound));
                conditionalValues.ConditionalVariance = (Math.Pow(time, 2) + time * ssuLowerBound + Math.Pow(ssuLowerBound, 2)) / 3 -
                                      Math.Pow(
                                          ((Math.Pow(time, 2) - Math.Pow(ssuLowerBound, 2)) /
                                           (2 * (time - ssuLowerBound))), 2);
                return conditionalValues;
            }
            
            //site guaranteed to be open
            conditionalValues.ConditionalSSU = 1;
            conditionalValues.ConditionalMean = (ssuLowerBound + ssuUpperBound) / 2;
            conditionalValues.ConditionalVariance = Math.Pow((ssuUpperBound - ssuLowerBound), 2) / 12;
            return conditionalValues;
        }
    }
}
