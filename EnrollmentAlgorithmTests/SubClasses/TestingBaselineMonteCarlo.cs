using System;
using EnrollmentAlgorithm.Methods.MonteCarlo;
using EnrollmentAlgorithm.Objects.Enrollment;

namespace EnrollmentAlgorithmTests.SubClasses
{
    public class TestingBaselineMonteCarlo : BaselineMonteCarlo
    {
        public double GenerateSSUValue_Wrapper(SiteParameter site) => GenerateSSUValue(site);
        public double GenerateScreeningValue_Wrapper(SiteParameter site) => GenerateScreeningValue(site);
        public DateTime GenerateSIVDate_Wrapper(SiteParameter site, double ssuValue) => GenerateSIVDate(site, ssuValue);
        public DateTime GetStartPoint_Wrapper(DateTime startPoint) => GetStartPoint(startPoint);
        public bool ContinueEnrolling_Wrapper(int overallEnrollmentSpan, CountryParameter country, int countryEnrollment,
            int day, int enrollmentTarget) => ContinueEnrolling(country, countryEnrollment,enrollmentTarget);


    }
}
