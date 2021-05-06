using System;
using System.Linq;
using EnrollmentAlgorithm.Methods;
using EnrollmentAlgorithm.Objects.Enrollment;
using EnrollmentAlgorithmTests.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Semio.ClientService.Data.Intelligence.Enrollment;

namespace EnrollmentAlgorithmTests
{
    [TestClass]
    public class BuildParameterTests
    {
        [TestMethod]
        public void CreateTrialParameter_Should_HaveCountryList()
        {
            var trialParameter = EnrollmentObjectCreation.CreateTrialParameter(TestEnrollmentCollection);
            Assert.IsTrue(trialParameter.CountryList.Count > 0);
        }

        [TestMethod]
        public void BaselineVirtualSiteDistributions_Should_MatchCountryDistributions()
        {
            var virtualSiteList =
                TestTrialParameter.CountryList.SelectMany(c => c.SiteParameters.Where(s => s.SiteStatus == "Virtual"));
            foreach (var site in virtualSiteList)
            {
                Assert.IsTrue(Math.Abs(site.BaselineEnrollmentDistribution.Mean - TestTrialParameter.CountryList.Single(c => c.Name == site.CountryName).BaselineEnrollmentDistribution.Mean) < double.Epsilon);
                Assert.IsTrue(Math.Abs(site.BaselineSSUDistribution.Mean - TestTrialParameter.CountryList.Single(c => c.Name == site.CountryName).BaselineSSUDistribution.Mean) < double.Epsilon);
                Assert.IsTrue(Math.Abs(site.BaselineScreeningDistribution.Mean - TestTrialParameter.CountryList.Single(c => c.Name == site.CountryName).BaselineScreeningDistribution.Mean) < double.Epsilon);
                Assert.IsTrue(Math.Abs(site.BaselineScreeningDistribution.Mean - site.ReprojectionScreeningDistribution.Mean) < double.Epsilon);
                Assert.IsTrue(Math.Abs(site.BaselineEnrollmentDistribution.Mean - site.ReprojectionEnrollmentDistribution.Mean) < double.Epsilon);
            }
        }

        [TestMethod]
        public void BaselineActualSiteScreenAndEnrDistributions_ShouldNot_MatchReprojectionsScreenAndEnrDistributions ()
        {
            var virtualSiteList =
                TestTrialParameter.CountryList.SelectMany(c => c.SiteParameters.Where(s => s.SiteStatus != "Virtual"));

            foreach (var site in virtualSiteList)
            {

                Assert.IsTrue(Math.Abs(site.BaselineScreeningDistribution.Mean - site.ReprojectionScreeningDistribution.Mean) > double.Epsilon);
                Assert.IsTrue(Math.Abs(site.BaselineEnrollmentDistribution.Mean - site.ReprojectionEnrollmentDistribution.Mean) > double.Epsilon);
            }
        }

        [TestInitialize]
        public void FetchEnrollmentCollection()
        {
            TestEnrollmentCollection = SampleData.GetData("EnrollmentAlgorithmTests.TestData", "enrollmentCollection.json");
            TestTrialParameter = EnrollmentObjectCreation.CreateTrialParameter(TestEnrollmentCollection);
        }
        private EnrollmentCollection TestEnrollmentCollection { get; set; }
        private TrialParameter TestTrialParameter { get; set; }
    }
}
        