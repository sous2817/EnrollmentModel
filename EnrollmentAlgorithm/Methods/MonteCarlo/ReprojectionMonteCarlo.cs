using System;
using EnrollmentAlgorithm.Objects.Enrollment;

namespace EnrollmentAlgorithm.Methods.MonteCarlo
{
    internal class ReprojectionMonteCarlo : CoreMonteCarlo
    {
        protected override double GenerateSSUValue(SiteParameter site) => site.SiteInitiationVisit == null ? site.BaselineSSUDistribution.Distribution.Sample() : 0;

        protected override double GenerateScreeningValue(SiteParameter site) => site.ReprojectionScreeningDistribution.Distribution.Sample();

        protected override DateTime GenerateSIVDate(SiteParameter site, double ssuValue)
            => site.SiteInitiationVisit ?? site.SiteSelectionVisit.GetValueOrDefault(DateTime.Today).AddDays(Convert.ToInt32(ssuValue) + site.SIVDelay);

        protected override DateTime GetStartPoint(DateTime trialStartDate) => trialStartDate > DateTime.Today ? trialStartDate : DateTime.Today;

    }
}
