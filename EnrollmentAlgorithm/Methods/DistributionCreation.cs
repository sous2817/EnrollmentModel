using System;
using EnrollmentAlgorithm.Objects.Additional;
using EnrollmentAlgorithm.Objects.Enums;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;

namespace EnrollmentAlgorithm.Methods
{
    public static class DistributionCreation
    {
        public static DistributionParameter CreateDistributionUsingAlphaAndRate(DistributionType distributionType, double alpha, double rate)
        {
            
            var distributionParameter = new DistributionParameter { Alpha = alpha, Rate = rate, Type=distributionType };

            switch (distributionType)
            {
                case DistributionType.Gamma:
                    distributionParameter.Distribution = new Gamma(alpha,rate, new SystemRandomSource());
                    distributionParameter.LowerBound = 0;
                    distributionParameter.UpperBound = double.PositiveInfinity;
                    break;
                default:
                    throw new ArgumentException("distributionType is invalid. Valid value is 'Gamma',", nameof(distributionType));
            }

            distributionParameter.Mean = distributionParameter.Distribution.Mean;
            distributionParameter.StandardDeviation = distributionParameter.Distribution.StdDev;

            return distributionParameter;
        }

        public static DistributionParameter CreateScreeningDistributionUsingAlphaRateAndScreenFailure(
            DistributionType distributionType, double alpha, double rate, double screenFailureRate)
        {
            var distributionParameter = new DistributionParameter { Alpha = alpha, Rate = rate, Type = distributionType };

            switch (distributionType)
            {
                case DistributionType.Gamma:
                    distributionParameter.Distribution = new Gamma(alpha, rate, new SystemRandomSource());
                    distributionParameter.LowerBound = 0;
                    distributionParameter.UpperBound = double.PositiveInfinity;
                    break;
                default:
                    throw new ArgumentException("distributionType is invalid. Valid value is 'Gamma',", nameof(distributionType));
            }

            distributionParameter = CreateScreeningDistributionUsingEnrMeanStdDevAndScreenFailure(
                DistributionType.Gamma, distributionParameter.Distribution.Mean, distributionParameter.Distribution.StdDev,
                screenFailureRate);
            
            return distributionParameter;
        }

        public static DistributionParameter CreateDistributionUsingMeanAndStdDev(DistributionType distributionType, double mean, double standardDeviation)
        {
            var distributionParameter = new DistributionParameter
            {
                Mean = mean,
                StandardDeviation = standardDeviation,
            };

            switch (distributionType)
            {
                case DistributionType.Gamma:
                    var variance = standardDeviation * standardDeviation;
                    var rate = mean  / variance;
                    var alpha = mean * rate;
                    distributionParameter.Alpha = alpha;
                    distributionParameter.Rate = rate;

                    distributionParameter.Distribution = new Gamma(distributionParameter.Alpha, distributionParameter.Rate, new SystemRandomSource());
                    break;
                default:
                    throw new ArgumentException("distributionType is invalid. Valid value is 'Gamma',", nameof(distributionType));
            }

            distributionParameter.StandardDeviation = distributionParameter.Distribution.StdDev;

            return distributionParameter;
        }
        //ToDo: Fix this!

        public static DistributionParameter CreateScreeningDistributionUsingEnrMeanStdDevAndScreenFailure(DistributionType distributionType, double mean, double standardDeviation, double screenFailRate)
        {
            var distributionParameter = new DistributionParameter
            {
                Mean = Math.Abs(screenFailRate) < double.Epsilon ? mean : mean * (1/(1-screenFailRate)),
                StandardDeviation = standardDeviation,
                LowerBound = 0
            };

            switch (distributionType)
            {
                case DistributionType.Gamma:
                    var variance = distributionParameter.StandardDeviation * distributionParameter.StandardDeviation;
                    var rate = distributionParameter.Mean / variance;
                    var alpha = distributionParameter.Mean * rate;
                    distributionParameter.Alpha = alpha;
                    distributionParameter.Rate = rate;

                    distributionParameter.Distribution = new Gamma(distributionParameter.Alpha, distributionParameter.Rate, new SystemRandomSource());
                    break;
                default:
                    throw new ArgumentException("distributionType is invalid. Valid value is 'Gamma',", nameof(distributionType));
            }

            distributionParameter.StandardDeviation = distributionParameter.Distribution.StdDev;

            return distributionParameter;
        }

        public static DistributionParameter CreateDistributionUsingBounds(DistributionType distributionType,double lowerBound, double upperBound)
        {
            var distributionParameter = new DistributionParameter
            {
                UpperBound = upperBound,
                LowerBound = lowerBound
            };

            switch (distributionType)
            {
                case DistributionType.Uniform:
                    distributionParameter.Distribution = new ContinuousUniform(lowerBound, upperBound, new SystemRandomSource());
                    break;
                default:
                    throw new ArgumentException("distributionType is invalid. Valid value is 'Gamma',", nameof(distributionType));
            }

            distributionParameter.StandardDeviation = distributionParameter.Distribution.StdDev;
            distributionParameter.Mean = distributionParameter.Distribution.Mean;

            return distributionParameter;
        }

        public static DistributionParameter CreateUpdatedDistributionParameter(
            DistributionParameter baselineDistributionParameter, int currentEnrollment, int lengthOfTimeOpen) => CreateDistributionUsingAlphaAndRate(DistributionType.Gamma,
            baselineDistributionParameter.Alpha + currentEnrollment,
            baselineDistributionParameter.Rate + lengthOfTimeOpen);

    }
}
