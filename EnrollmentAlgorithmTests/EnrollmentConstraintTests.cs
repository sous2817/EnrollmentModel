using EnrollmentAlgorithm.Objects.Enrollment;
using EnrollmentAlgorithm.Objects.Enums;
using EnrollmentAlgorithmTests.SubClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EnrollmentAlgorithmTests
{
    [TestClass]
    public class EnrollmentConstraintTests
    {
        [TestMethod]
        public void ContinueEnrolling_Should_ReturnTrueWhenMinPatientsNotReachedAndUnderEnrollmentSpan()
        {
            const int fakeOverallEnrollmentSpan = 10;
            const int fakeCurrentCountyEnrollment = 90;
            const int fakeCurrentDay = 5;
            const int fakeMaxEnrollment = 200;
            var fakeCountryParameter =
            new CountryParameter
            {
                MinPatientEnrollment = 100,
                AccrualConstraint = EnrollmentAccrualConstraint.MinimumPatient
            };

            var keepEnrolling = TestBaselineMonteCarlo.ContinueEnrolling_Wrapper(fakeOverallEnrollmentSpan, fakeCountryParameter, fakeCurrentCountyEnrollment, fakeCurrentDay, fakeMaxEnrollment);
            Assert.IsTrue(keepEnrolling);
        }
       
        [TestMethod]
        public void ContinueEnrolling_Should_ReturnTrueWhenMinPatientsNotReachedAndOverEnrollmentSpan()
        {
            const int fakeOverallEnrollmentSpan = 10;
            const int fakeCurrentCountyEnrollment = 5;
            const int fakeCurrentDay = 500;
            const int fakeMaxEnrollment = 200;
            var fakeCountryParameter =
            new CountryParameter
            {
                MinPatientEnrollment = 100,
                AccrualConstraint = EnrollmentAccrualConstraint.MinimumPatient
            };

            var keepEnrolling = TestBaselineMonteCarlo.ContinueEnrolling_Wrapper(fakeOverallEnrollmentSpan, fakeCountryParameter, fakeCurrentCountyEnrollment, fakeCurrentDay, fakeMaxEnrollment);
            Assert.IsTrue(keepEnrolling);
        }

        [TestMethod]
        public void ContinueEnrolling_Should_ReturnFalseWhenMinPatientsReachedAndAtEnrollmentSpan()
        {
            const int fakeOverallEnrollmentSpan = 10;
            const int fakeCurrentCountyEnrollment = 100;
            const int fakeCurrentDay = 10;
            const int fakeMaxEnrollment = 200;
            var fakeCountryParameter =
            new CountryParameter
            {
                MinPatientEnrollment = 100,
                AccrualConstraint = EnrollmentAccrualConstraint.MinimumPatient
            };

            var keepEnrolling = TestBaselineMonteCarlo.ContinueEnrolling_Wrapper(fakeOverallEnrollmentSpan, fakeCountryParameter, fakeCurrentCountyEnrollment, fakeCurrentDay, fakeMaxEnrollment);
            Assert.IsFalse(keepEnrolling);
        }

        [TestMethod]
        public void ContinueEnrolling_Should_ReturnFalseWhenMinPatientsReachedAndAtOverEnrollmentSpan()
        {
            const int fakeOverallEnrollmentSpan = 10;
            const int fakeCurrentCountyEnrollment = 100;
            const int fakeCurrentDay = 500;
            const int fakeMaxEnrollment = 200;
            var fakeCountryParameter =
            new CountryParameter
            {
                MinPatientEnrollment = 100,
                AccrualConstraint = EnrollmentAccrualConstraint.MinimumPatient
            };

            var keepEnrolling = TestBaselineMonteCarlo.ContinueEnrolling_Wrapper(fakeOverallEnrollmentSpan, fakeCountryParameter, fakeCurrentCountyEnrollment, fakeCurrentDay, fakeMaxEnrollment);
            Assert.IsFalse(keepEnrolling);
        }

        [TestMethod]
        public void ContinueEnrolling_Should_ReturnFalseWhenMinPatientsExceededAndAtOverEnrollmentSpan()
        {
            const int fakeOverallEnrollmentSpan = 10;
            const int fakeCurrentCountyEnrollment = 1000;
            const int fakeCurrentDay = 500;
            const int fakeMaxEnrollment = 200;
            var fakeCountryParameter =
            new CountryParameter
            {
                MinPatientEnrollment = 100,
                AccrualConstraint = EnrollmentAccrualConstraint.MinimumPatient
            };

            var keepEnrolling = TestBaselineMonteCarlo.ContinueEnrolling_Wrapper(fakeOverallEnrollmentSpan, fakeCountryParameter, fakeCurrentCountyEnrollment, fakeCurrentDay, fakeMaxEnrollment);
            Assert.IsFalse(keepEnrolling);
        }

        [TestMethod]
        public void ContinueEnrolling_Should_ReturnFalseWhenEnrollmentSpanReachedAndNoConstraint()
        {
            const int fakeOverallEnrollmentSpan = 10;
            const int fakeCurrentCountyEnrollment = 1000;
            const int fakeCurrentDay = 10;
            const int fakeMaxEnrollment = 200;
            var fakeCountryParameter =
            new CountryParameter
            {
                AccrualConstraint = EnrollmentAccrualConstraint.None
            };

            var keepEnrolling = TestBaselineMonteCarlo.ContinueEnrolling_Wrapper(fakeOverallEnrollmentSpan, fakeCountryParameter, fakeCurrentCountyEnrollment, fakeCurrentDay, fakeMaxEnrollment);
            Assert.IsFalse(keepEnrolling);
        }

        [TestMethod]
        public void ContinueEnrolling_Should_ReturnFalseWhenEnrollmentSpanNotReachedButTrialTargetReachedByCountry()
        {
            const int fakeOverallEnrollmentSpan = 100;
            const int fakeCurrentCountyEnrollment = 1000;
            const int fakeCurrentDay = 10;
            const int fakeMaxEnrollment = 1000;
            var fakeCountryParameter =
            new CountryParameter
            {
                AccrualConstraint = EnrollmentAccrualConstraint.None
            };

            var keepEnrolling = TestBaselineMonteCarlo.ContinueEnrolling_Wrapper(fakeOverallEnrollmentSpan, fakeCountryParameter, fakeCurrentCountyEnrollment, fakeCurrentDay, fakeMaxEnrollment);
            Assert.IsFalse(keepEnrolling);
        }

        [TestMethod]
        public void ContinueEnrolling_Should_ReturnTrueWhenEnrollmentSpanNotReachedEnrollmentTargetNotReachedAndNoConstraint()
        {
            const int fakeOverallEnrollmentSpan = 10;
            const int fakeCurrentCountyEnrollment = 150;
            const int fakeCurrentDay = 5;
            const int fakeMaxEnrollment = 200;
            var fakeCountryParameter =
            new CountryParameter
            {
                AccrualConstraint = EnrollmentAccrualConstraint.None
            };

            var keepEnrolling = TestBaselineMonteCarlo.ContinueEnrolling_Wrapper(fakeOverallEnrollmentSpan, fakeCountryParameter, fakeCurrentCountyEnrollment, fakeCurrentDay, fakeMaxEnrollment);
            Assert.IsTrue(keepEnrolling);
        }
        

        [TestMethod]
        public void ContinueEnrolling_Should_ReturnTrueWhenMaxPatientsNotReachedAndUnderEnrollmentSpan()
        {
            const int fakeOverallEnrollmentSpan = 10;
            const int fakeCurrentCountyEnrollment = 90;
            const int fakeCurrentDay = 5;
            const int fakeMaxEnrollment = 200;
            var fakeCountryParameter =
            new CountryParameter
            {
                MaxPatientEnrollment = 100,
                AccrualConstraint = EnrollmentAccrualConstraint.MaximumPatient
            };

            var keepEnrolling = TestBaselineMonteCarlo.ContinueEnrolling_Wrapper(fakeOverallEnrollmentSpan, fakeCountryParameter, fakeCurrentCountyEnrollment, fakeCurrentDay, fakeMaxEnrollment);
            Assert.IsTrue(keepEnrolling);
        }

        [TestMethod]
        public void ContinueEnrolling_Should_ReturnFalseWhenMaxPatientsReachedAndUnderEnrollmentSpan()
        {
            const int fakeOverallEnrollmentSpan = 10;
            const int fakeCurrentCountyEnrollment = 100;
            const int fakeCurrentDay = 5;
            const int fakeMaxEnrollment = 200;
            var fakeCountryParameter =
            new CountryParameter
            {
                MaxPatientEnrollment = 100,
                AccrualConstraint = EnrollmentAccrualConstraint.MaximumPatient
            };

            var keepEnrolling = TestBaselineMonteCarlo.ContinueEnrolling_Wrapper(fakeOverallEnrollmentSpan, fakeCountryParameter, fakeCurrentCountyEnrollment, fakeCurrentDay, fakeMaxEnrollment);
            Assert.IsFalse(keepEnrolling);
        }

        [TestMethod]
        public void ContinueEnrolling_Should_ReturnFalseWhenMaxPatientsReachedAndAtEnrollmentSpan()
        {
            const int fakeOverallEnrollmentSpan = 10;
            const int fakeCurrentCountyEnrollment = 100;
            const int fakeCurrentDay = 10;
            const int fakeMaxEnrollment = 200;
            var fakeCountryParameter =
            new CountryParameter
            {
                MaxPatientEnrollment = 100,
                AccrualConstraint = EnrollmentAccrualConstraint.MaximumPatient
            };

            var keepEnrolling = TestBaselineMonteCarlo.ContinueEnrolling_Wrapper(fakeOverallEnrollmentSpan, fakeCountryParameter, fakeCurrentCountyEnrollment, fakeCurrentDay, fakeMaxEnrollment);
            Assert.IsFalse(keepEnrolling);
        }
        
        [TestInitialize]
        public void FetchEnrollmentCollection()
        {
            TestBaselineMonteCarlo = new TestingBaselineMonteCarlo();
        }
        
        private TestingBaselineMonteCarlo TestBaselineMonteCarlo { get; set; }
    }

}

