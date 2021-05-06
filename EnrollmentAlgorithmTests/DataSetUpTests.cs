using EnrollmentAlgorithmTests.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Semio.ClientService.Data.Intelligence.Enrollment;

namespace EnrollmentAlgorithmTests
{
    [TestClass]
    public class DataSetUpTests
    {
        [TestMethod]
        public void GetSampleData_Should_HaveACountryList()
        {
            Assert.IsTrue(TestEnrollmentCollection.CountryCount > 0);
        }

        [TestInitialize]
        public void FetchEnrollmentCollection()
        {
            TestEnrollmentCollection = SampleData.GetData("EnrollmentAlgorithmTests.TestData", "enrollmentCollection.json");
        }
        private EnrollmentCollection TestEnrollmentCollection { get; set; }
    }

}

