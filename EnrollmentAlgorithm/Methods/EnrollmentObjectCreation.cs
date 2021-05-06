using System;
using System.Collections.Generic;
using System.Linq;
using EnrollmentAlgorithm.Objects.Additional;
using EnrollmentAlgorithm.Objects.Enrollment;
using EnrollmentAlgorithm.Objects.Enums;
using Semio.ClientService.Data.Intelligence.Enrollment;

namespace EnrollmentAlgorithm.Methods
{
    public static class EnrollmentObjectCreation
    {
        private const double MonthToDayConstant = 30.4368;

        public static TrialParameter CreateTrialParameter(EnrollmentCollection enrollmentCollection, int numberOfIterations = 500)
        {
            //TODO: Change Confidence if / when it gets implemented in to EnrollmentCollection
            //TODO: Change Probability if / when it gets implemented in to EnrollmentCollection
            var trialParameter = new TrialParameter
            {
                NumberOfIterations = numberOfIterations,
                EnrollmentTarget = (int)enrollmentCollection.DesiredPatients,
                CurrentActualEnrollment = HelperMethods.GetTotalEnrolled(enrollmentCollection),
                ScreenFailRate = enrollmentCollection.TrialScreenFailureRate,
                ScreeningPeriodLowerBound = enrollmentCollection.TrialScreeningPeriodInDays, 
                ScreeningPeriodUpperBound = Math.Max(enrollmentCollection.TrialScreeningPeriodInDays +1, enrollmentCollection.TrialScreeningPeriodMaxInDays),
                SIVDelay = enrollmentCollection.AdditionalLagToSiv ?? 0, 
                DropoutRate = enrollmentCollection.DropOutRate ?? 0,
                SimulationSeed = enrollmentCollection.SeedValueForProbabilityModel, 
                Confidence = .95,
                Probability = .99,
                StudyStartDate = enrollmentCollection.SsvStartDate, 
            };

            var countryList = enrollmentCollection.EnrolledCountries.Where(c => c.Sites.Any(x=> x.IsChecked)).ToList();
            
            trialParameter.CountryList = CreateCountryParameter(countryList);
            trialParameter.EstimatedLengthOfEnrollment = HelperMethods.CalculateLengthOfTimeToEnroll(trialParameter, 1.5);
            return trialParameter;
        }

        private static List<CountryParameter> CreateCountryParameter(List<CountryEnrollment> countryList)
        {
            var countryParameterList = new List<CountryParameter>();
            
            foreach (var country in countryList)
            {
                var screeningDistribution = DistributionCreation.CreateScreeningDistributionUsingEnrMeanStdDevAndScreenFailure(DistributionType.Gamma, 
                    country.MonthlyAccrual / MonthToDayConstant, 
                    country.MonthlyAccrualStandardDeviation / MonthToDayConstant, 
                    Convert.ToDouble(country.CountryScreenFailureRate));

                var enrollmentDistribution =
                    DistributionCreation.CreateDistributionUsingMeanAndStdDev(DistributionType.Gamma,
                        country.MonthlyAccrual/MonthToDayConstant,
                        country.MonthlyAccrualStandardDeviation/MonthToDayConstant);

                var ssuDistribution = DistributionCreation.CreateDistributionUsingBounds(DistributionType.Uniform,country.MinSSUDays, country.MaxSSUDays);

                var earliestSIV = country.Sites.Min(s => s.SiteSivDate);
                var totalCountryEnrollment = country.Sites.Sum(s => s.SitePtsEnrolled) ?? 0;
                var lengthOfTimeOpen = HelperMethods.CalculateLengthOfTimeOpen(earliestSIV);

                var reprojectionScreeningDistribution =
                    DistributionCreation.CreateScreeningDistributionUsingAlphaRateAndScreenFailure(
                        DistributionType.Gamma, enrollmentDistribution.Alpha + totalCountryEnrollment,
                        enrollmentDistribution.Rate + lengthOfTimeOpen, Convert.ToDouble(country.CountryScreenFailureRate));

                var reprojectionEnrollmentDistribution = DistributionCreation.CreateDistributionUsingAlphaAndRate(DistributionType.Gamma,
                        enrollmentDistribution.Alpha + totalCountryEnrollment,
                        enrollmentDistribution.Rate +lengthOfTimeOpen);

                var currentCountry = new CountryParameter
                {
                    BaselineEnrollmentDistribution = enrollmentDistribution,
                    BaselineScreeningDistribution = screeningDistribution,
                    BaselineSSUDistribution = ssuDistribution,
                    ReprojectionScreeningDistribution = reprojectionScreeningDistribution,
                    ReprojectionEnrollmentDistribution = reprojectionEnrollmentDistribution,
                    StudyStartDate = country.Sites.Min(s => s.SiteSsvDate ?? DateTime.MinValue), 
                    Name = country.Country, 
                    MinPatientEnrollment = country.RequiredPatients, 
                    MaxPatientEnrollment = country.RequiredPatientsMax, 
                    EnrollmentStartDate = country.EarliestFPRConstraint ?? DateTime.MinValue,
                    SiteParameters = CreateSiteParameter(country, ssuDistribution, screeningDistribution, enrollmentDistribution), 
                    ScreenFailRate = Convert.ToDouble(country.CountryScreenFailureRate), 
                    AccrualConstraint = SetEnrollmentConstraint(country.RequiredPatients, country.RequiredPatientsMax),
                    CurrentActualEnrollment = country.PatientsEnrolledToDate
                };

                countryParameterList.Add(currentCountry);
            }
          
            return countryParameterList;
        }

        private static List<SiteParameter> CreateSiteParameter(CountryEnrollment country, DistributionParameter ssuDistribution,DistributionParameter screeningDistribution, DistributionParameter enrollmentDistribution)
        {
            var siteList = new List<SiteParameter>();
            foreach (var site in country.Sites.Where(x => x.IsChecked))
            {
                var lengthOfTimeOpen = HelperMethods.CalculateLengthOfTimeOpen(site.SiteSivDate);
                var currentSiteEnrollment = site.SitePtsEnrolled ?? 0;
                var updatedEnrollmentRate = site.SiteUpdatedEnrRate ?? 0;
                var expectedEnrollmentRate = site.SiteExpectedEnrRate ?? 0;

                var enrReprojDistribution =
                    DistributionCreation.CreateUpdatedDistributionParameter(enrollmentDistribution,
                        currentSiteEnrollment, lengthOfTimeOpen);

                if (Math.Abs(updatedEnrollmentRate - expectedEnrollmentRate) > (decimal).0001)
                {
                    enrReprojDistribution = DistributionCreation.CreateDistributionUsingMeanAndStdDev(DistributionType.Gamma,
                        (double)expectedEnrollmentRate / MonthToDayConstant, (double)expectedEnrollmentRate * .25 / MonthToDayConstant);
                }

                var reprojScreeningDist =
                    DistributionCreation.CreateScreeningDistributionUsingEnrMeanStdDevAndScreenFailure(
                        DistributionType.Gamma, enrReprojDistribution.Mean, enrReprojDistribution.StandardDeviation,
                        Convert.ToDouble(country.CountryScreenFailureRate));

                var siteObj = new SiteParameter
                {
                    Name = site.SiteName,
                    BaselineSSUDistribution = ssuDistribution,
                    BaselineEnrollmentDistribution = enrollmentDistribution,
                    BaselineScreeningDistribution = screeningDistribution, 
                    ReprojectionEnrollmentDistribution = enrReprojDistribution,
                    ReprojectionScreeningDistribution = reprojScreeningDist,
                    CurrentActualEnrollment = currentSiteEnrollment,
                    EnrollmentStopDate = site.SiteEnrollmentClosedDate ?? DateTime.MaxValue,
                    SiteStatus = site.SiteStatus,
                    SiteSelectionVisit = site.SiteSsvDate ?? site.DateSiteSelectedForReprojections,
                    SiteInitiationVisit = ReturnSIVDate(site),
                    CountryName = country.Country, 
                    ScreenFailRate = Convert.ToDouble(country.CountryScreenFailureRate)
                };
                siteList.Add(siteObj);   
            }

            return siteList;
        }

        private static DateTime? ReturnSIVDate(InvestigatorEnrollment site)
        {
            if(site.SiteSivDate == null || site.DateProjectedSiteInitiationForReprojection == null) return null;
            return site.SiteSivDate ?? site.DateProjectedSiteInitiationForReprojection.Value;
        }

        private static EnrollmentAccrualConstraint SetEnrollmentConstraint(int minimumPatient, int maximumPatient)
        {
            if (minimumPatient == 0 && maximumPatient == 0)
            {
                return EnrollmentAccrualConstraint.None;
            }

            if (minimumPatient == maximumPatient)
            {
                return EnrollmentAccrualConstraint.ExactPatient;
            }

            if (minimumPatient > 1 && maximumPatient > 1)
            {
                return EnrollmentAccrualConstraint.Between;
            }

            if (minimumPatient > 1)
            {
                return EnrollmentAccrualConstraint.MinimumPatient;
            }

            return maximumPatient > 1 ? EnrollmentAccrualConstraint.MaximumPatient : EnrollmentAccrualConstraint.None;
        }
    }
}
