using System;
using System.Runtime.CompilerServices;
using EnrollmentAlgorithm.Objects.Enrollment;

[assembly: InternalsVisibleTo("EnrollmentAlgorithmTests")]
namespace EnrollmentAlgorithm.Methods.MonteCarlo
{
    public class BaselineMonteCarlo : CoreMonteCarlo
    {
        protected override double GenerateSSUValue(SiteParameter site) => site.BaselineSSUDistribution.Distribution.Sample();
        protected override double GenerateScreeningValue(SiteParameter site) => site.BaselineScreeningDistribution.Distribution.Sample();
        protected override DateTime GenerateSIVDate(SiteParameter site, double ssuValue) => site.SiteSelectionVisit?.AddDays(Convert.ToInt32(ssuValue) + site.SIVDelay) ?? DateTime.Today;
        protected override DateTime GetStartPoint(DateTime trialStartDate) => trialStartDate;
    }
}
