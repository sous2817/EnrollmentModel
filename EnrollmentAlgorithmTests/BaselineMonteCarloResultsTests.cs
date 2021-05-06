using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnrollmentAlgorithm.Methods;
using EnrollmentAlgorithm.Objects.Additional;
using EnrollmentAlgorithm.Objects.Enrollment;
using EnrollmentAlgorithmTests.SubClasses;
using EnrollmentAlgorithmTests.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Semio.ClientService.Data.Intelligence.Enrollment;

namespace EnrollmentAlgorithmTests
{
    [TestClass]
    public class BaselineMonteCarloResultsTests
    {
        private EnrollmentCollection TestEnrollmentCollection { get; set; }
        private TrialParameter TestTrialParameter { get; set; }
        private TestingBaselineMonteCarlo TestBaselineMonteCarlo { get; set; }

        [TestInitialize]
        public void FetchEnrollmentCollection()
        {
            TestEnrollmentCollection = SampleData.GetData("EnrollmentAlgorithmTests.TestData",
                "enrollmentCollection.json");
            TestTrialParameter = EnrollmentObjectCreation.CreateTrialParameter(TestEnrollmentCollection);
            TestBaselineMonteCarlo = new TestingBaselineMonteCarlo();
        }

        [TestMethod]
        public void GenerateSSUValues_Should_GenerateBetweenBaselineMinAndMaxInEachCountry()
        {
            var siteList = TestTrialParameter.CountryList.SelectMany(s => s.SiteParameters).ToList();

            var testCountryParameters =
                siteList.GroupBy(
                        s =>
                                s.CountryName)
                    .Select(
                        group =>
                            new
                            {
                                CountryName = group.Key,
                                SSUUpperBound = group.Max(s => s.BaselineSSUDistribution.UpperBound),
                                SSULowerBound = group.Min(s => s.BaselineSSUDistribution.LowerBound)
                            }).ToList();

            for (var i = 0; i < TestTrialParameter.NumberOfIterations; i++)
            {
                foreach (var site in siteList)
                {
                    site.ResultsOfSimulation.SimulationValuesList.Add(new SimulationValues
                    {
                        SSUValue = TestBaselineMonteCarlo.GenerateSSUValue_Wrapper(site)
                    });
                }
            }

            var testSiteList =
                siteList.GroupBy(s => s.CountryName)
                    .Select(
                        group =>
                            new
                            {
                                Country = group.Key,
                                MinSSUValue =
                                group.Min(s => s.ResultsOfSimulation.SimulationValuesList.Min(v => v.SSUValue)),
                                MaxSSUValue =
                                group.Max(s => s.ResultsOfSimulation.SimulationValuesList.Max(v => v.SSUValue))
                            });

            foreach (var derivedValue in testSiteList)
            {
                var baseMinValue =
                    testCountryParameters.Where(c => c.CountryName == derivedValue.Country)
                        .Select(s => s.SSULowerBound)
                        .Min();
                var baseMaxValue =
                    testCountryParameters.Where(c => c.CountryName == derivedValue.Country)
                        .Select(s => s.SSUUpperBound)
                        .Max();
                Assert.IsTrue(derivedValue.MaxSSUValue <= baseMaxValue);
                Assert.IsTrue(derivedValue.MinSSUValue >= baseMinValue);
            }
        }

        [TestMethod]
        public void GenerateScreeningValues_Should_BeCloseToTheBaselineAverage()
        {
            var siteList = TestTrialParameter.CountryList.SelectMany(s => s.SiteParameters).ToList();

            for (var i = 0; i < 1000; i++)
            {
                foreach (var site in siteList)
                {
                    site.ResultsOfSimulation.SimulationValuesList.Add(new SimulationValues
                    {
                        InitialScreeningRate = TestBaselineMonteCarlo.GenerateScreeningValue_Wrapper(site)
                    });
                }
            }

            foreach (var site in siteList)
            {
                var siteAvgScreeningRate =
                    site.ResultsOfSimulation.SimulationValuesList.Average(s => s.InitialScreeningRate);
                Assert.IsTrue(
                    Math.Abs(siteAvgScreeningRate - site.BaselineScreeningDistribution.Distribution.Mean) < .0999,
                    $"Sample Avg:{siteAvgScreeningRate} | Distribution Avg:{site.BaselineScreeningDistribution.Distribution.Mean}");
            }
        }

        [TestMethod]
        public void GenerateSIVDate_Should_AddDaysToSpecifiedSSVDate()
        {
            var siteParameter = new SiteParameter
            {
                SiteSelectionVisit = new DateTime(2017, 1, 1)
            };

            Assert.IsTrue(TestBaselineMonteCarlo.GenerateSIVDate_Wrapper(siteParameter, 10) == new DateTime(2017, 1, 11));
        }

        [TestMethod]
        public void GetStartPoint_Should_ReturnTrialStartDateForBaseline()
        {
            var testDate = new DateTime(2016, 01, 01);
            TestTrialParameter.EnrollmentStartDate = testDate;
            var startPoint = TestBaselineMonteCarlo.GetStartPoint_Wrapper(TestTrialParameter.EnrollmentStartDate);
            Assert.IsTrue(startPoint == testDate);
        }

        [TestMethod]
        public void Simulate_Should_ReturnEnrollmentTargetWhenNoConstraintsSet()
        {
            var testEnrollmentCollection = SampleData.GetData("EnrollmentAlgorithmTests.TestData",
                "enrollmentCollectionStopEarlyError.json");
            
            var testTrialParameter = TestBaselineMonteCarlo.Simulate(testEnrollmentCollection);

            foreach (var patientAccrual in testTrialParameter.ResultsOfSimulation.SimulationValuesList)
            {
                Assert.IsTrue(patientAccrual.PatientAccrual.Count(x => x.EnrollmentDate > DateTime.MinValue) ==
                              testTrialParameter.EnrollmentTarget);
            }
        }

        [TestMethod]
        public void Simulate_Should_ReturnMessagesEvenIfCanceled()
        {
            var testEnrollmentCollection = SampleData.GetData("EnrollmentAlgorithmTests.TestData",
                "enrollmentCollection_5000Enrollment.json");
            var cancelTokenSource = new CancellationTokenSource(5000);
            var token = cancelTokenSource.Token;


            var messageList = new List<ProgressReporter>();
            var timeList = new List<DateTime>();
            var progress = new Progress<ProgressReporter>(specificMessage =>
            {
                timeList.Add(DateTime.Now);
                messageList.Add(specificMessage);
            });

            var exceptionCaught = false;

            try
            {
                Task.WaitAll(TestBaselineMonteCarlo.SimulateWithMessages(testEnrollmentCollection, progress, token, 25));
            }
            catch (AggregateException e)
            {
                if (e.InnerExceptions.Any(x => x.GetType() == typeof(TaskCanceledException)))
                {
                    exceptionCaught = true;
                }
            }

            Assert.IsTrue(exceptionCaught);
            Assert.IsTrue(timeList.Count > 0);
            Assert.IsTrue(messageList.Count > 0);
        }

        [TestMethod]
        public void Simulate_Should_Cancel()
        {
            var testEnrollmentCollection = SampleData.GetData("EnrollmentAlgorithmTests.TestData",
                "enrollmentCollection_5000Enrollment.json");
            var cancelTokenSource = new CancellationTokenSource(5000);
            var token = cancelTokenSource.Token;
            

            var progress = new Progress<ProgressReporter>(specificMessage =>
            {
            });

            var exceptionCaught = false;

            try
            {
                Task.WaitAll(TestBaselineMonteCarlo.SimulateWithMessages(testEnrollmentCollection, progress, token, 25));
            }
            catch (AggregateException e)
            {
                if(e.InnerExceptions.Any(x => x.GetType() == typeof(TaskCanceledException)))
                {
                    exceptionCaught = true;
                }
            }

            Assert.IsTrue(exceptionCaught);
        }

        [TestMethod]
        public void  Simulate_Should_ReturnSomeValueWhenRunning()
        {
            var messageList = new List<ProgressReporter>();

            var progress = new Progress<ProgressReporter>(specificMessage => { messageList.Add(specificMessage); });

            var trialParameter = TestBaselineMonteCarlo.SimulateWithMessages(TestEnrollmentCollection,progress, CancellationToken.None, 25);

            foreach (var patientAccrual in trialParameter.Result.ResultsOfSimulation.SimulationValuesList)
            {
                Assert.IsTrue(patientAccrual.PatientAccrual.Count(x => x.EnrollmentDate > DateTime.MinValue) == trialParameter.Result.EnrollmentTarget);
            }

            Assert.IsTrue(messageList.Count >0);
        }

        [TestMethod]
        public void Simulate_Should_ReturnEnrollmentTargetWhenNoConstraintsSetLargeEnrollment()
        {
            var testEnrollmentCollection = SampleData.GetData("EnrollmentAlgorithmTests.TestData", "enrollmentCollection_5000Enrollment.json");
            var testTrialParameter = TestBaselineMonteCarlo.Simulate(testEnrollmentCollection);

            foreach (var patientAccrual in testTrialParameter.ResultsOfSimulation.SimulationValuesList)
            {
                Assert.IsTrue(patientAccrual.PatientAccrual.Count(x => x.EnrollmentDate > DateTime.MinValue) == testTrialParameter.EnrollmentTarget);
            }
        }

        [TestMethod]
        public void Simulate_Should_ReturnNoMoreThanMaximumPatientEnrollmentWhenSet()
        {
            TestEnrollmentCollection = SampleData.GetData("EnrollmentAlgorithmTests.TestData", "enrollmentCollection_MaxTest.json");

            var oneCountryWithMaximum = TestEnrollmentCollection.EnrolledCountries.FirstOrDefault(x => x.RequiredPatientsMax > 0);

            if (oneCountryWithMaximum == null) Assert.Fail("No criteria to test");

            var testTrialParameter = TestBaselineMonteCarlo.Simulate(TestEnrollmentCollection);

            foreach (var patientAccrual in testTrialParameter.ResultsOfSimulation.SimulationValuesList)
            {
                Assert.IsTrue(
                    patientAccrual.PatientAccrual.Where(x => x.Country == oneCountryWithMaximum.Country)
                        .Count(x => x.EnrollmentDate > DateTime.MinValue) <= oneCountryWithMaximum.RequiredPatientsMax,
                    string.Format(
                        patientAccrual.PatientAccrual.Where(x => x.Country == oneCountryWithMaximum.Country).ToString()));
                Assert.IsTrue(
                    patientAccrual.PatientAccrual.Count(x => x.EnrollmentDate > DateTime.MinValue) == testTrialParameter.EnrollmentTarget);
            }
        }
        
        [TestMethod]
        public void Simulate_Should_ReturnAtLeastMinimumPatientEnrollmentWhenSet()
        {
            TestEnrollmentCollection = SampleData.GetData("EnrollmentAlgorithmTests.TestData", "enrollmentCollection_MinTest.json");

            var oneCountryWithMinimum = TestEnrollmentCollection.EnrolledCountries.FirstOrDefault(x => x.RequiredPatients > 0);

            if (oneCountryWithMinimum == null) Assert.Fail("No criteria to test");

            var testTrialParameter = TestBaselineMonteCarlo.Simulate(TestEnrollmentCollection);

            foreach (var patientAccrual in testTrialParameter.ResultsOfSimulation.SimulationValuesList)
            {
                Assert.IsTrue(
                    patientAccrual.PatientAccrual.Where(x => x.Country == oneCountryWithMinimum.Country)
                        .Count(x => x.EnrollmentDate > DateTime.MinValue) >= oneCountryWithMinimum.RequiredPatients,
                    string.Format(
                        patientAccrual.PatientAccrual.Where(x => x.Country == oneCountryWithMinimum.Country).ToString()));
                Assert.IsTrue(
                    patientAccrual.PatientAccrual.Count(x => x.EnrollmentDate > DateTime.MinValue) == testTrialParameter.EnrollmentTarget);
            }
        }
        
        [TestMethod]
        public void Simulate_Should_ReturnExactWhenMinimumAndMaximumPatientEnrollmentWhenEqual()
        {
            TestEnrollmentCollection = SampleData.GetData("EnrollmentAlgorithmTests.TestData", "enrollmentCollection_ExactTest.json");

            var oneCountryWithMinAndMax =
                TestEnrollmentCollection.EnrolledCountries.FirstOrDefault(
                    x =>
                        x.RequiredPatients > 0 && x.RequiredPatientsMax > 0 &&
                        x.RequiredPatients == x.RequiredPatientsMax);

            if (oneCountryWithMinAndMax == null) Assert.Fail("No criteria to test");

            var testTrialParameter = TestBaselineMonteCarlo.Simulate(TestEnrollmentCollection);

            foreach (var patientAccrual in testTrialParameter.ResultsOfSimulation.SimulationValuesList)
            {
                Assert.IsTrue(
                    patientAccrual.PatientAccrual.Where(x => x.Country == oneCountryWithMinAndMax.Country)
                        .Count(x => x.EnrollmentDate > DateTime.MinValue) == oneCountryWithMinAndMax.RequiredPatients);
                Assert.IsTrue(
                    patientAccrual.PatientAccrual.Where(x => x.Country == oneCountryWithMinAndMax.Country)
                        .Count(x => x.EnrollmentDate > DateTime.MinValue) == oneCountryWithMinAndMax.RequiredPatientsMax);
                Assert.IsTrue(
                    patientAccrual.PatientAccrual.Count(x => x.EnrollmentDate > DateTime.MinValue) == testTrialParameter.EnrollmentTarget);
            }
        }

        [TestMethod]
        public void Simulate_Should_ReturnBetweenWhenMinimumAndMaximumPatientEnrollmentWhenSet()
        {
            TestEnrollmentCollection = SampleData.GetData("EnrollmentAlgorithmTests.TestData", "enrollmentCollection_BetweenTest.json");

            var oneCountryWithMinAndMax =
                TestEnrollmentCollection.EnrolledCountries.FirstOrDefault(
                    x =>
                        x.RequiredPatients > 0 && x.RequiredPatientsMax > 0 &&
                        x.RequiredPatients < x.RequiredPatientsMax);

            if (oneCountryWithMinAndMax == null) Assert.Fail("No criteria to test");

            var testTrialParameter = TestBaselineMonteCarlo.Simulate(TestEnrollmentCollection);
            var dummyIterationCounter = 0;
            foreach (var patientAccrual in testTrialParameter.ResultsOfSimulation.SimulationValuesList)
            {
                dummyIterationCounter++;
                Assert.IsTrue(
                    patientAccrual.PatientAccrual.Where(x => x.Country == oneCountryWithMinAndMax.Country)
                        .Count(x => x.EnrollmentDate > DateTime.MinValue) >= oneCountryWithMinAndMax.RequiredPatients);
                Assert.IsTrue(
                    patientAccrual.PatientAccrual.Where(x => x.Country == oneCountryWithMinAndMax.Country)
                        .Count(x => x.EnrollmentDate > DateTime.MinValue) <= oneCountryWithMinAndMax.RequiredPatientsMax, $"Country: {oneCountryWithMinAndMax.Country} | Accrual: {patientAccrual.PatientAccrual.Where(x => x.Country == oneCountryWithMinAndMax.Country).Count(x => x.EnrollmentDate > DateTime.MinValue)} | Expected Accrual: {oneCountryWithMinAndMax.RequiredPatientsMax} | Iteration: {dummyIterationCounter}");
                Assert.IsTrue(
                    patientAccrual.PatientAccrual.Count(x => x.EnrollmentDate > DateTime.MinValue) == testTrialParameter.EnrollmentTarget);
            }
        }


    }
}
