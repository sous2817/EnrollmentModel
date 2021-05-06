using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EnrollmentAlgorithm.Methods;
using EnrollmentAlgorithm.Objects.Enums;
using MSTestExtensions;

namespace EnrollmentAlgorithmTests
{
    [TestClass]
    public class DistributionCreationTests : BaseTest
    {
        [TestMethod]
        public void CreateDistributionUsingAlphaAndRate_Should_HaveAMeanEqualTo1()
        {
            var distribution = DistributionCreation.CreateDistributionUsingAlphaAndRate(DistributionType.Gamma, 5, 5);
            Assert.AreEqual(distribution.Distribution.Mean,1);
        }
        [TestMethod]
        public void CreateDistributionUsingAlphaAndRate_Should_ThrowExceptionWhenPassingWrongType()
        {
            
            Assert.Throws<ArgumentException>(
                () => DistributionCreation.CreateDistributionUsingAlphaAndRate(DistributionType.Uniform, 5, 5));
        }

        [TestMethod]
        public void CreateDistributionUsingMeanAndStdDev_Should_HaveAlphaAndRateValueEqualTo2()
        {
            var distribution = DistributionCreation.CreateDistributionUsingMeanAndStdDev(DistributionType.Gamma, 4,
                2);
            Assert.AreEqual(Math.Round(distribution.Alpha,2), 4);
            Assert.AreEqual(Math.Round(distribution.Rate, 2), 1);
        }

        [TestMethod]
        public void CreateDistributionUsingMeanAndStdDev_Should_ThrowExceptionWhenPassingWrongType()
        {
            Assert.Throws<ArgumentException>(
                () => DistributionCreation.CreateDistributionUsingMeanAndStdDev(DistributionType.Uniform, 5, 5));
        }

        [TestMethod]
        public void CreateScreeningDistributionUsingEnrMeanStdDevAndScreenFailure_Should_HaveAMeanEqualTo5ish()
        {
            // the screening mean should be higher than the enrollment mean since 'n' percent of patients fail screening.  In this example, 30% of subjects are expected 
            // to fail screening, so in order to maintain an enrollment rate of 4, we have to screen ~5.7 patients (on average).  This method confirms the calculations.
            var enrMean = 10;
            var screenFailureRate = .3;
            var distribution = DistributionCreation.CreateScreeningDistributionUsingEnrMeanStdDevAndScreenFailure(DistributionType.Gamma, enrMean,
                2, screenFailureRate);
            var meanAdjustedForScreenFailure = enrMean * 1 / (1-screenFailureRate);
            Assert.AreEqual(distribution.Distribution.Mean, meanAdjustedForScreenFailure);
        }

        [TestMethod]
        public void CreateScreeningDistributionUsingEnrMeanStdDevAndScreenFailure_Should_ThrowExceptionWhenPassingWrongType()
        {
            Assert.Throws<ArgumentException>(
                () => DistributionCreation.CreateScreeningDistributionUsingEnrMeanStdDevAndScreenFailure(DistributionType.Uniform, 5, 5,.5));
        }

        [TestMethod]
        public void CreateDistributionUsingBounds_Should_HaveAMeanEqualTo3()
        {
            var distribution = DistributionCreation.CreateDistributionUsingBounds(DistributionType.Uniform, 2, 4);
            Assert.AreEqual(distribution.Distribution.Mean,3);
        }

        [TestMethod]
        public void CreateDistributionUsingBounds_Should_ThrowExceptionWhenPassingWrongType()
        {
            Assert.Throws<ArgumentException>(
                () => DistributionCreation.CreateDistributionUsingBounds(DistributionType.Gamma, 5, 5));
        }

        [TestMethod]
        public void CreateUpdatedDistributionParameter_Should_GenerateAMeanValueReferencedInComments()
        {
            // Creates distribution w/ a mean of 1 (think 1 patients per month)
            var distribution = DistributionCreation.CreateDistributionUsingAlphaAndRate(DistributionType.Gamma, 5, 5);

            // No updates (site not open AND no enrollment)
            var updatedDistribution = DistributionCreation.CreateUpdatedDistributionParameter(distribution, 0, 0);
            Assert.IsTrue(Math.Abs(distribution.Distribution.Mean - updatedDistribution.Distribution.Mean) < .0001);

            // Site meets enrollment expectations (5 subjects in 5 months)
            updatedDistribution = DistributionCreation.CreateUpdatedDistributionParameter(distribution, 5, 5);
            Assert.IsTrue(Math.Abs(distribution.Mean - updatedDistribution.Distribution.Mean) < .0001);

            // Site under performs (5 patients in 15 months)
            updatedDistribution = DistributionCreation.CreateUpdatedDistributionParameter(distribution, 5, 15);
            Assert.IsTrue(updatedDistribution.Distribution.Mean < distribution.Distribution.Mean);

            // Site over performs (15 patients in 5 months)
            updatedDistribution = DistributionCreation.CreateUpdatedDistributionParameter(distribution, 15, 5);
            Assert.IsTrue(updatedDistribution.Distribution.Mean > distribution.Distribution.Mean);
        }
    }
}
