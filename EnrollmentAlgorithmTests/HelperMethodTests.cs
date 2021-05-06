using System;
using System.Collections.Generic;
using EnrollmentAlgorithm.Methods;
using EnrollmentAlgorithm.Objects.Additional;
using EnrollmentAlgorithm.Objects.Enrollment;
using EnrollmentAlgorithmTests.SubClasses;
using EnrollmentAlgorithmTests.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTestExtensions;
using Semio.ClientService.Data.Intelligence.Enrollment;

namespace EnrollmentAlgorithmTests
{
    [TestClass]
    public class HelperMethodTests : BaseTest
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
        public void CalculateLengthOfTimeToEnroll_Should_BeGreaterThan24()
        {
            var trialParameters = EnrollmentObjectCreation.CreateTrialParameter(TestEnrollmentCollection);
            trialParameters.CountryList[0].EnrollmentStartDate = new DateTime(2020,1,1);

            var totalEnrTime = HelperMethods.CalculateLengthOfTimeToEnroll(trialParameters);
            Assert.IsTrue(totalEnrTime > 24);

        }


        [TestMethod]
        public void FPRConstraintAndClosureCheck_Should_ReturnTrueIfTimeBetweenStartAndStopDates()
        {
            var stopDate = new DateTime(2016,7,30);
            var fprConstraint = new DateTime(2016,1,1);
            var dummyTime = 50;

            var testSite = new SiteParameter
            {
                EnrollmentStopDate = stopDate,
                EnrollmentStartDate = fprConstraint
            };

            var testValue = HelperMethods.FPRConstraintAndClosureCheck(dummyTime, testSite);

            Assert.IsTrue(testValue);
        }

        [TestMethod]
        public void FPRConstraintAndClosureCheck_Should_ReturnFalseIfTimeGreaterThanClosureDate()
        {
            var stopDate = new DateTime(2016, 7, 30);
            var fprConstraint = new DateTime(2016, 1, 1);
            var dummyTime = int.MaxValue;

            var testSite = new SiteParameter
            {
                EnrollmentStopDate = stopDate,
                EnrollmentStartDate = fprConstraint
            };

            var testValue = HelperMethods.FPRConstraintAndClosureCheck(dummyTime, testSite);

            Assert.IsFalse(testValue);
        }

        [TestMethod]
        public void FPRConstraintAndClosureCheck_Should_ReturnFalseIfFPRBeforeStopDate()
        {
            var stopDate = new DateTime(2015, 7, 30);
            var fprConstraint = new DateTime(2016, 1, 1);
            var dummyTime = 65;

            var testSite = new SiteParameter
            {
                EnrollmentStopDate = stopDate,
                EnrollmentStartDate = fprConstraint
            };

            var testValue = HelperMethods.FPRConstraintAndClosureCheck(dummyTime, testSite);

            Assert.IsFalse(testValue);
        }

        [TestMethod]
        public void FPRConstraintAndClosureCheck_Should_ReturnFalseIfTimeAfterClosure()
        {
            var stopDate = new DateTime(2016, 7, 30);
            var fprConstraint = new DateTime(2016, 1, 1);
            var dummyTime = int.MaxValue;

            var testSite = new SiteParameter
            {
                EnrollmentStopDate = stopDate,
                EnrollmentStartDate = fprConstraint
            };

            var testValue = HelperMethods.FPRConstraintAndClosureCheck(dummyTime, testSite);

            Assert.IsFalse(testValue);
        }


    }
}
