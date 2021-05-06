using System;
using System.Linq;
using EnrollmentAlgorithm.Methods;
using EnrollmentAlgorithm.Objects.Additional;
using EnrollmentAlgorithm.Objects.Enrollment;
using EnrollmentAlgorithmTests.ExtensionMethods;
using EnrollmentAlgorithmTests.SubClasses;
using EnrollmentAlgorithmTests.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Semio.ClientService.Data.Intelligence.Enrollment;

namespace EnrollmentAlgorithmTests
{
    [TestClass]
    public class ReprojectionMonteCarloResultsTests
    {

        private EnrollmentCollection TestEnrollmentCollection { get; set; }
        private TrialParameter TestTrialParameter { get; set; }
        private TestingReprojectionMonteCarlo TestReprojectionMonteCarlo { get; set; }
        
        [TestInitialize]
        public void FetchEnrollmentCollection()
        {
            TestEnrollmentCollection = SampleData.GetData("EnrollmentAlgorithmTests.TestData", "enrollmentCollection.json");
            TestTrialParameter = EnrollmentObjectCreation.CreateTrialParameter(TestEnrollmentCollection);
            TestReprojectionMonteCarlo = new TestingReprojectionMonteCarlo();
        }

        [TestMethod]
        public void GenerateScreeningValues_Should_BeCloseToTheReprojectionAverage()
        {
            var siteList = TestTrialParameter.CountryList.SelectMany(s => s.SiteParameters).ToList();

            for (var i = 0; i < 1000; i++)
            {
                foreach (var site in siteList)
                {
                    site.ResultsOfSimulation.SimulationValuesList.Add(new SimulationValues
                    {
                        InitialScreeningRate = TestReprojectionMonteCarlo.GenerateScreeningValue_Wrapper(site)
                    });
                }
            }

            foreach (var site in siteList)
            {
                var siteAvgScreeningRate =
                    site.ResultsOfSimulation.SimulationValuesList.Average(s => s.InitialScreeningRate);
                Assert.IsTrue(
                    Math.Abs(siteAvgScreeningRate - site.ReprojectionScreeningDistribution.Distribution.Mean) < .0999,
                    $"Sample Avg:{siteAvgScreeningRate} | Distribution Avg:{site.ReprojectionScreeningDistribution.Distribution.Mean}");
            }
        }

        [TestMethod]
        public void GenerateSSUValues_Should_GenerateBetweenReprojectionMinAndMaxForVirtualSites()
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
                    var ssuValue = TestReprojectionMonteCarlo.GenerateSSUValue_Wrapper(site);

                    site.ResultsOfSimulation.SimulationValuesList.Add(new SimulationValues
                    {
                        SSUValue = ssuValue
                    });
                }
            }

            foreach (var site in siteList)
            {
                if (site.SiteStatus != "Virtual") continue;

                Assert.IsTrue(site.ResultsOfSimulation.SimulationValuesList.Min(s => s.SSUValue) >=
                              testCountryParameters.Where(c => c.CountryName == site.CountryName)
                                  .Select(s => s.SSULowerBound)
                                  .Min());

                Assert.IsTrue(site.ResultsOfSimulation.SimulationValuesList.Max(s => s.SSUValue) <=
                              testCountryParameters.Where(c => c.CountryName == site.CountryName)
                                  .Select(s => s.SSUUpperBound)
                                  .Max());
            }
        }

        [TestMethod]
        public void GenerateSSUValues_Should_GenerateZeroForNonVirtualSites()
        {
            var siteList = TestTrialParameter.CountryList.SelectMany(s => s.SiteParameters).ToList();

            for (var i = 0; i < TestTrialParameter.NumberOfIterations; i++)
            {
                foreach (var site in siteList)
                {
                    site.ResultsOfSimulation.SimulationValuesList.Add(new SimulationValues
                    {
                        SSUValue = TestReprojectionMonteCarlo.GenerateSSUValue_Wrapper(site)
                    });
                }
            }

            foreach (var site in siteList)
            {
                if (site.SiteStatus == "Virtual") continue;
                Assert.IsTrue(
                    Math.Abs(site.ResultsOfSimulation.SimulationValuesList.Select(s => s.SSUValue).Unanimous(0)) <
                    double.Epsilon);
            }
        }

        [TestMethod]
        public void GetStartPoint_Should_ReturnTodayForReprojectionsIfStartDateIsInThePast()
        {
            var testDate = new DateTime(2016, 01, 01);
            TestTrialParameter.EnrollmentStartDate = testDate;
            var startPoint = TestReprojectionMonteCarlo.GetStartPoint_Wrapper(TestTrialParameter.EnrollmentStartDate);
            Assert.IsTrue(startPoint == DateTime.Now.Date);
        }

        [TestMethod]
        public void GetStartPoint_Should_ReturnTrialStartDateForReprojectionsIfStartDateIsInTheFuture()
        {
            var testDate = DateTime.Now.AddYears(1);
            TestTrialParameter.EnrollmentStartDate = testDate;
            var startPoint = TestReprojectionMonteCarlo.GetStartPoint_Wrapper(TestTrialParameter.EnrollmentStartDate);
            Assert.IsTrue(startPoint == testDate);
        }

        [TestMethod]
        public void Simulate_Should_ReturnCorrectAccrualWithSomeActuals()
        {
            var testEnrollmentCollection = SampleData.GetData("EnrollmentAlgorithmTests.TestData",
                "enrollmentCollection_MinTest.json");
            var oneCountryWithMinimum =
                testEnrollmentCollection.EnrolledCountries.FirstOrDefault(x=>x.Sites.Count >0);

            if (oneCountryWithMinimum == null) return;
            oneCountryWithMinimum.Sites[0].SitePtsEnrolled = oneCountryWithMinimum.Sites[0].SitePtsEnrolled == 0 ? 25 : oneCountryWithMinimum.Sites[0].SitePtsEnrolled;
            
            // this is here for testing
            

            var testTrialParameter = TestReprojectionMonteCarlo.Simulate(testEnrollmentCollection);

            foreach (var patientAccrual in testTrialParameter.ResultsOfSimulation.SimulationValuesList)
            {
                Assert.IsTrue(
                    patientAccrual.PatientAccrual.Count(x => x.EnrollmentDate > DateTime.MinValue) + testEnrollmentCollection.EnrolledCountries.Sum(x => x.PatientsEnrolledToDate) ==
                    TestTrialParameter.EnrollmentTarget);
            }
        }

        [TestMethod]
        public void Simulate_Should_ReturnCorrectAccrualWhenActualsHaveMetMaxRequirement()
        {
            var testEnrollmentCollection = SampleData.GetData("EnrollmentAlgorithmTests.TestData",
                "enrollmentCollection_MaxTest.json");
            var oneCountryWithMaximum =
                testEnrollmentCollection.EnrolledCountries.FirstOrDefault(x => x.RequiredPatientsMax > 0);

            if (oneCountryWithMaximum == null) return;
            oneCountryWithMaximum.Sites[0].SitePtsEnrolled = oneCountryWithMaximum.Sites[0].SitePtsEnrolled != oneCountryWithMaximum.RequiredPatientsMax ? oneCountryWithMaximum.RequiredPatientsMax : oneCountryWithMaximum.Sites[0].SitePtsEnrolled;

            var testTrialParameter = TestReprojectionMonteCarlo.Simulate(testEnrollmentCollection);

            foreach (var patientAccrual in testTrialParameter.ResultsOfSimulation.SimulationValuesList)
            {
                Assert.IsTrue(
                    patientAccrual.PatientAccrual.Where(x => x.Country == oneCountryWithMaximum.Country)
                        .Count(x => x.EnrollmentDate > DateTime.MinValue) <= oneCountryWithMaximum.RequiredPatientsMax,
                    string.Format(
                        patientAccrual.PatientAccrual.Where(x => x.Country == oneCountryWithMaximum.Country).ToString()));
                Assert.IsTrue(
                    patientAccrual.PatientAccrual.Count(x => x.EnrollmentDate > DateTime.MinValue) == TestTrialParameter.EnrollmentTarget);
            }
        }
        
        [TestMethod]
        public void Simulate_Should_ReturnEnrollmentTargetWhenNoConstraintsSetAndNoActuals()
        {
            var testTrialParameter = TestReprojectionMonteCarlo.Simulate(TestEnrollmentCollection);

            foreach (var patientAccrual in testTrialParameter.ResultsOfSimulation.SimulationValuesList)
            {
                Assert.IsTrue(patientAccrual.PatientAccrual.Count(x => x.EnrollmentDate > DateTime.MinValue) == TestTrialParameter.EnrollmentTarget);
            }
        }

        [TestMethod]
        public void Simulate_Should_ReturnAtLeastMinimumPatientEnrollmentWhenSetAndActuals()
        {
            var testEnrollmentCollection = SampleData.GetData("EnrollmentAlgorithmTests.TestData",
                "enrollmentCollection_MinTest.json");
            var oneCountryWithMinimum =
                testEnrollmentCollection.EnrolledCountries.FirstOrDefault(x => x.RequiredPatients > 0);

            if (oneCountryWithMinimum == null) Assert.Fail("No criteria to test");
            oneCountryWithMinimum.Sites[0].SitePtsEnrolled = oneCountryWithMinimum.Sites[0].SitePtsEnrolled == 0 ? 10 : oneCountryWithMinimum.Sites[0].SitePtsEnrolled;

            var testTrialParameter = TestReprojectionMonteCarlo.Simulate(testEnrollmentCollection);

            foreach (var patientAccrual in testTrialParameter.ResultsOfSimulation.SimulationValuesList)
            {
                Assert.IsTrue(
                    patientAccrual.PatientAccrual.Where(x => x.Country == oneCountryWithMinimum.Country)
                        .Count(x => x.EnrollmentDate > DateTime.MinValue) + oneCountryWithMinimum.PatientsEnrolledToDate >= oneCountryWithMinimum.RequiredPatients,
                    $"NumberEnrolled: {patientAccrual.PatientAccrual.Where(x => x.Country == oneCountryWithMinimum.Country).Count(x => x.EnrollmentDate > DateTime.MinValue)}");

                Assert.IsTrue(
                    patientAccrual.PatientAccrual.Count(x => x.EnrollmentDate > DateTime.MinValue) + testEnrollmentCollection.EnrolledCountries.Sum(x => x.PatientsEnrolledToDate) ==
                    TestTrialParameter.EnrollmentTarget);
            }
        }

        [TestMethod]
        public void Simulate_Should_ReturnExactAccrualWhenMinimumAndMaximumPatientEnrollmentEqualAndActuals()
        {
            TestEnrollmentCollection = SampleData.GetData("EnrollmentAlgorithmTests.TestData", "enrollmentCollection_ExactTest.json");

            var oneCountryWithMinAndMax =
                TestEnrollmentCollection.EnrolledCountries.FirstOrDefault(
                    x =>
                        x.RequiredPatients > 0 && x.RequiredPatientsMax > 0 &&
                        x.RequiredPatients == x.RequiredPatientsMax);

            if (oneCountryWithMinAndMax == null) Assert.Fail("No criteria to test");

            oneCountryWithMinAndMax.Sites[0].SitePtsEnrolled = oneCountryWithMinAndMax.Sites[0].SitePtsEnrolled == 0 ? 10 : oneCountryWithMinAndMax.Sites[0].SitePtsEnrolled;

            var testTrialParameter = TestReprojectionMonteCarlo.Simulate(TestEnrollmentCollection);

            foreach (var patientAccrual in testTrialParameter.ResultsOfSimulation.SimulationValuesList)
            {
                Assert.IsTrue(
                    patientAccrual.PatientAccrual.Where(x => x.Country == oneCountryWithMinAndMax.Country)
                        .Count(x => x.EnrollmentDate > DateTime.MinValue) + oneCountryWithMinAndMax.PatientsEnrolledToDate == oneCountryWithMinAndMax.RequiredPatients, 
                    $"Accrued: {patientAccrual.PatientAccrual.Where(x => x.Country == oneCountryWithMinAndMax.Country).Count(x => x.EnrollmentDate > DateTime.MinValue)} | Expected: {oneCountryWithMinAndMax.RequiredPatients} ");
                Assert.IsTrue(
                    patientAccrual.PatientAccrual.Where(x => x.Country == oneCountryWithMinAndMax.Country)
                        .Count(x => x.EnrollmentDate > DateTime.MinValue) + oneCountryWithMinAndMax.PatientsEnrolledToDate == oneCountryWithMinAndMax.RequiredPatientsMax);
                Assert.IsTrue(
                    patientAccrual.PatientAccrual.Count(x => x.EnrollmentDate > DateTime.MinValue) + oneCountryWithMinAndMax.PatientsEnrolledToDate == testTrialParameter.EnrollmentTarget);
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

            oneCountryWithMinAndMax.Sites[0].SitePtsEnrolled = oneCountryWithMinAndMax.Sites[0].SitePtsEnrolled == 0 ? 10 : oneCountryWithMinAndMax.Sites[0].SitePtsEnrolled;

            var testTrialParameter = TestReprojectionMonteCarlo.Simulate(TestEnrollmentCollection);

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
