using System;
using System.Collections.Generic;
using System.Linq;
using EnrollmentAlgorithm.Methods;
using EnrollmentAlgorithm.Methods.Summary;
using EnrollmentAlgorithm.Objects.Additional;
using EnrollmentAlgorithm.Objects.Enrollment;
using EnrollmentAlgorithmTests.SubClasses;
using EnrollmentAlgorithmTests.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Semio.ClientService.Data.Intelligence.Enrollment;
using MSTestExtensions;

namespace EnrollmentAlgorithmTests
{
    [TestClass]
    public class AccrualSummaryTests : BaseTest
    {
        private EnrollmentCollection TestEnrollmentCollection { get; set; }
        private TrialParameter TestTrialParameter { get; set; }

        private static List<SimulationValues> TestSimulationValuesList { get; set; }

        [TestInitialize]
        public void FetchEnrollmentCollection()
        {
            TestEnrollmentCollection = SampleData.GetData("EnrollmentAlgorithmTests.TestData", "enrollmentCollection.json");
            TestTrialParameter = EnrollmentObjectCreation.CreateTrialParameter(TestEnrollmentCollection);
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            TestSimulationValuesList = new List<SimulationValues>();

            var testEnrollmentCollection = SampleData.GetData("EnrollmentAlgorithmTests.TestData", "enrollmentCollection.json");
            var testBaselineMonteCarlo = new TestingBaselineMonteCarlo();
            var trialParameter = testBaselineMonteCarlo.Simulate(testEnrollmentCollection);
            TestSimulationValuesList = trialParameter.ResultsOfSimulation.SimulationValuesList;
        }

        [TestMethod]
        public void GetMeanAndErrorBasedOffRisk_Should_ReturnSomething()
        {
            var testValue =
                new Accrual().GetMeanAndErrorBasedOffRisk(
                    TestTrialParameter.EnrollmentTarget, TestSimulationValuesList,
                    x => x.CumulatedEnrolled);

            Assert.IsTrue(testValue.Count == 5);
        }

        [TestMethod]
        public void GetDateBasedOffRisk_Should_ReturnSomethingGreaterThanMinDate()
        {
            var testValue = new Accrual().GetDateBasedOffRisk(.75, TestTrialParameter.EnrollmentTarget, TestSimulationValuesList, x => x.CumulatedEnrolled);

            Assert.IsTrue(testValue > DateTime.MinValue);
        }


        [TestMethod]
        public void CalculateProbabilityOfSuccessDates_Should_ReturnListOfValidDates()
        {
            var percentileList = new List<double>();
            for (var i = .1; i <= 1; i += .1)
            {
                percentileList.Add(i);
            }

            var testValues = new Accrual().CalculateProbabilityOfSuccessDates(percentileList,
                TestTrialParameter.EnrollmentTarget, TestSimulationValuesList, x => x.CumulatedEnrolled);

            foreach (var percentile in percentileList)
            {
                Assert.IsTrue(testValues[percentile] > DateTime.MinValue);
            }
        }

        [TestMethod]
        public void CalculateProbabilityOfSuccess_Should_ReturnSomethingGreaterThan0()
        {

            var testTime = TestSimulationValuesList.Max(x => x.LatestAccrualDate).AddDays(-150);

            var testValue = new Accrual().CalculateProbabilityOfSuccess(testTime, TestTrialParameter.EnrollmentTarget,
                TestSimulationValuesList, x => x.CumulatedEnrolled);

            Assert.IsTrue(testValue > 0);
        }

        [TestMethod]
        public void GetAccruedBasedOnDateAndRisk_Should_ReturnSomethingGreaterThan0()
        {
            var testTime = TestSimulationValuesList.Max(x => x.LatestAccrualDate).AddDays(-500);

            var testValue = new Accrual().GetAccruedBasedOnDateAndRisk(testTime,
                .65, TestSimulationValuesList, x => x.CumulatedEnrolled);

            Assert.IsTrue(testValue > 0);
        }

        [TestMethod]
        public void GetProjectedAccrualLineValues_Should_ReturnErrorIfPercentileIsLessThan0()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
               () => new Accrual().GetProjectedAccrualLineValues(-1, TestSimulationValuesList, x => x.CumulatedEnrolled));
        }

        [TestMethod]
        public void GetProjectedAccrualLineValues_Should_ReturnErrorIfPercentileIsGreaterThan100()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
               () => new Accrual().GetProjectedAccrualLineValues(1.1, TestSimulationValuesList, x => x.CumulatedEnrolled));
        }

        [TestMethod]
        public void GetProjectedAccrualLineValues_Should_ReturnListOfValuesForSpecificPercentile()
        {
            var accrualList = new Accrual().GetProjectedAccrualLineValues(.50, TestSimulationValuesList, x => x.CumulatedEnrolled);
            Assert.IsTrue(accrualList.Any());
        }

        [TestMethod]
        public void CalculateProbabilityOfSuccess_Should_Return0IfDateEarlierThanStart()
        {
            var testValue = new Accrual().CalculateProbabilityOfSuccess(DateTime.MinValue, 1, TestSimulationValuesList, x => x.CumulatedEnrolled);

            Assert.IsTrue(Math.Abs(testValue) < double.Epsilon);
        }
        [TestMethod]
        public void CalculateProbabilityOfSuccess_Should_Return1IfDateLaterThanEnd()
        {
            var testValue = new Accrual().CalculateProbabilityOfSuccess(DateTime.MaxValue, 100, TestSimulationValuesList, x => x.CumulatedEnrolled);

            Assert.IsTrue(Math.Abs(testValue - 1) < double.Epsilon);
        }

        [TestMethod]
        public void GetAccruedBasedOnDateAndRisk_Should_ReturnErrorIfPercentileIsGreaterThan100()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
               () => new Accrual().GetAccruedBasedOnDateAndRisk(DateTime.MaxValue, 101, TestSimulationValuesList, x => x.CumulatedEnrolled));
        }

        [TestMethod]
        public void GetAccruedBasedOnDateAndRisk_Should_ReturnErrorIfPercentileIsLessThan0()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
               () => new Accrual().GetAccruedBasedOnDateAndRisk(TestSimulationValuesList[0].LatestAccrualDate, -1, TestSimulationValuesList, x => x.CumulatedEnrolled));
        }

        [TestMethod]
        public void GetAccruedBasedOnDateAndRisk_Should_ReturnErrorIfDateIsGreaterThan100()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
               () => new Accrual().GetAccruedBasedOnDateAndRisk(TestSimulationValuesList[0].LatestAccrualDate, 1.1, TestSimulationValuesList, x => x.CumulatedEnrolled));
        }

        [TestMethod]
        public void GetAccruedBasedOnDateAndRisk_Should_ReturnErrorIfDateLessThanStart()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
               () => new Accrual().GetAccruedBasedOnDateAndRisk(DateTime.MaxValue, -1, TestSimulationValuesList, x => x.CumulatedEnrolled));
        }

        [TestMethod]
        public void GetAccruedBasedOnDateAndRisk_Should_ReturnErrorIfDateGreaterThanEnd()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
               () => new Accrual().GetAccruedBasedOnDateAndRisk(DateTime.MaxValue, -1, TestSimulationValuesList, x => x.CumulatedEnrolled));
        }

        [TestMethod]
        public void GetDateBasedOffRisk_Should_ReturnErrorIfPercentileIsLessThan0()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
               () => new Accrual().GetDateBasedOffRisk(-1, 100, TestSimulationValuesList, x => x.CumulatedEnrolled));
        }

        [TestMethod]
        public void GetDateBasedOffRisk_Should_ReturnErrorIfDateIsGreaterThan100()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
               () => new Accrual().GetDateBasedOffRisk(1.1, 100, TestSimulationValuesList, x => x.CumulatedEnrolled));
        }

        [TestMethod]
        public void CalculateProbabilityOfSuccess_Should_ReturnErrorIfTargetGreaterThanHighestValue()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
               () => new Accrual().CalculateProbabilityOfSuccess(TestSimulationValuesList[0].LatestAccrualDate, int.MaxValue, TestSimulationValuesList, x => x.CumulatedEnrolled));
        }

        [TestMethod]
        public void GetDateBasedOffRisk_Should_ReturnErrorIfTargetGreaterThanHighestValue()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
               () => new Accrual().GetDateBasedOffRisk(.50, int.MaxValue, TestSimulationValuesList, x => x.CumulatedEnrolled));
        }

    }
}
