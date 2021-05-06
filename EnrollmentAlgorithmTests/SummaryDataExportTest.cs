
using EnrollmentAlgorithm.Methods;
using EnrollmentAlgorithm.Methods.Export;
using EnrollmentAlgorithmTests.SubClasses;
using EnrollmentAlgorithmTests.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Semio.ClientService.Data.Intelligence.Enrollment;
using Semio.ClientService.OpenXml.Excel;

namespace EnrollmentAlgorithmTests
{
    [TestClass]
    public class SummaryDataExportTest
    {

        private EnrollmentCollection TestEnrollmentCollection { get; set; }
        private TestingBaselineMonteCarlo TestBaselineMonteCarlo { get; set; }

        [TestInitialize]
        public void FetchEnrollmentCollection()
        {
            TestEnrollmentCollection = SampleData.GetData("EnrollmentAlgorithmTests.TestData", "enrollmentCollection.json");
            EnrollmentObjectCreation.CreateTrialParameter(TestEnrollmentCollection);
            TestBaselineMonteCarlo = new TestingBaselineMonteCarlo();
        }


        [TestMethod]
        public void SummaryExportShouldGenerateFile()
        {
           // var testEnrollmentCollection = SampleData.GetData("EnrollmentAlgorithmTests.TestData", "enrollmentCollection_5000Enrollment.json");
            var testTrialParameter = TestBaselineMonteCarlo.Simulate(TestEnrollmentCollection);
            var summaryDataExporter = new SummaryDataExporter();
            var generatedOutput = new EnrollmentDataWorkbookExporter(new ExcelExporter(), summaryDataExporter);
            generatedOutput.ExportTo("C:\\Junk\\Work.xlsx", testTrialParameter);
        }
    }
}
