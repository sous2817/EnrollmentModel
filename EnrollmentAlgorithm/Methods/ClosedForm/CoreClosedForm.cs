using EnrollmentAlgorithm.Objects.Enrollment;
using MathNet.Numerics.Distributions;

namespace EnrollmentAlgorithm.Methods.ClosedForm
{
    public abstract class CoreClosedForm
    {
        //ToDo: Figure out what this returns
        public void ApproximateEnrollment()
        {
            var fakeTrialParameter = new TrialParameter();
            var foo = GetMoments(fakeTrialParameter);
        }

        private object GetMoments(TrialParameter trialParameter)
        {
            var modifiedProbability = 1 - trialParameter.Probability;
            var modifiedConfidence = (1 + trialParameter.Confidence)/2;

            var averageScreeningTime = (trialParameter.ScreeningPeriodLowerBound +
                                        trialParameter.ScreeningPeriodUpperBound)/2;

            var probabilityZScore = CalculateZScore(modifiedProbability);
            var confidenceZScore = CalculateZScore(modifiedConfidence);


            return null;
        }

        private static double CalculateZScore(double modifiedValue)
        {
            var normalDistribution = new Normal(0, 1);
            return normalDistribution.InverseCumulativeDistribution(modifiedValue);
        }
    }
}
