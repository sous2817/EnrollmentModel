using System;
using EnrollmentAlgorithm.Methods.MonteCarlo;
using EnrollmentAlgorithm.Objects.Enrollment;

namespace EnrollmentAlgorithmTests.SubClasses
{
    internal class TestingReprojectionMonteCarlo :ReprojectionMonteCarlo
    {
        public double GenerateSSUValue_Wrapper(SiteParameter site) => GenerateSSUValue(site);
        public double GenerateScreeningValue_Wrapper(SiteParameter site) => GenerateScreeningValue(site);
        public DateTime GetStartPoint_Wrapper(DateTime startPoint) => GetStartPoint(startPoint);
    }
}
