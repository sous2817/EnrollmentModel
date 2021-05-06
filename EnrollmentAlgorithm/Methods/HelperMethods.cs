using System;
using System.Collections.Generic;
using System.Linq;
using EnrollmentAlgorithm.Objects.Additional;
using EnrollmentAlgorithm.Objects.Enrollment;
using Semio.ClientService.Data.Intelligence.Enrollment;

namespace EnrollmentAlgorithm.Methods
{
    public static class HelperMethods
    {
        /// <summary>
        /// This method is used to give a general "worst case" for enrollment.  This is done to try and optimize the length of time the monte carlo algorithm runs.
        /// The flow is essentially "assume all country opens as late as possible (timeBetweenTrialStartAndSlowestCountryStart), all sites have the latest FPR 
        /// Constraint (timeBetweenTrialStartAndLastFPRConstraint), all sites take the longest SSU Time (SSU Time), all patients take the maximum time to screen
        /// (maxScreeningTime), and all sites enroll at the slowest specified rate (worstCaseAverageEnrTime).  Because this method cannot take in to account things 
        /// like caps and the randomness of the sampling, there is an arbitrary multiplier parameter that can be bumped up if needed (default multiplier is 1).   
        /// </summary>
        /// <param name="trialParameter">The trial parameter.</param>
        /// <param name="arbitraryMultiplier">The arbitrary multiplier.</param>
        /// <returns>System.Int32.</returns>
        /// <exception cref="OverflowException">return value is greater than <see cref="F:System.Int32.MaxValue" /> or less than <see cref="F:System.Int32.MinValue" />. </exception>
        public static int CalculateLengthOfTimeToEnroll(TrialParameter trialParameter, double arbitraryMultiplier = 1.0)
        {
            var timeBetweenTrialStartAndSlowestCountryStart =
                trialParameter.CountryList.Max(x => x.StudyStartDate).Subtract(trialParameter.StudyStartDate).Days;
            var maxSSUUpperBound = (int) trialParameter.CountryList.Max(c => c.BaselineSSUDistribution.UpperBound);

            var timeBetweenTrialStartAndLastFPRConstraint =
                trialParameter.CountryList.Max(c => c.EnrollmentStartDate).Subtract(trialParameter.StudyStartDate).Days;

            var ssuTime = Math.Max(timeBetweenTrialStartAndSlowestCountryStart + maxSSUUpperBound,
                timeBetweenTrialStartAndLastFPRConstraint);

            var maxScreeningTime = trialParameter.ScreeningPeriodUpperBound;
            var worstCaseAverageEnrTime = CalculateAverageWorstCaseEnrollmentTimeDays(trialParameter.CountryList,
                trialParameter.EnrollmentTarget);

            var enrollmentTime = (ssuTime + maxScreeningTime + worstCaseAverageEnrTime)*arbitraryMultiplier;

            return Convert.ToInt32(enrollmentTime);
        }

        /// <summary>
        /// Calculates the average worst case enrollment time in days.  All sites * lowest enrollment = potentially the longest a trial will need to enroll
        /// </summary>
        /// <param name="countryList">The country list.</param>
        /// <param name="targetEnrollment">The target enrollment.</param>
        /// <returns>System.Int32.</returns>
        private static int CalculateAverageWorstCaseEnrollmentTimeDays(List<CountryParameter> countryList,
            int targetEnrollment)
        {
            var numberOfSites = countryList.Sum(country => country.SiteParameters.Count);
            var lowestEnrRate =
                countryList.Min(country => country.SiteParameters.Min(site => site.BaselineEnrollmentDistribution.Mean));

            return (int) Math.Round(targetEnrollment/(numberOfSites*lowestEnrRate), 0);
        }

        /// <summary>
        /// Gets the total enrolled.
        /// </summary>
        /// <param name="enrollmentCollection">The enrollment collection.</param>
        /// <returns>System.Int32.</returns>
        /// <exception cref="OverflowException">The sum is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
        public static int GetTotalEnrolled(EnrollmentCollection enrollmentCollection)
        {
            var enrollmentSum = enrollmentCollection.EnrolledCountries.Sum(x => x.PatientsEnrolledToDate);
            return enrollmentSum;
        }

        /// <summary>
        /// Calculates the length of time a site is open.  Eventually this should be replaced once the various enrollment open statuses are being returned from DB.
        /// </summary>
        /// <param name="sivDate">The siv date.</param>
        /// <param name="reprojectionDate">The reprojection date.</param>
        /// <returns>System.Int32.</returns>
        /// ToDo: Swap CalculateLengthOfTimeOpen out once property exists in ClinWeb
        public static int CalculateLengthOfTimeOpen(DateTime? sivDate, DateTime? reprojectionDate = null)
        {
            var timeOpen = 0;
            if (sivDate == null) return timeOpen;

            var checkDate = DateTime.Today;
            if (reprojectionDate != null) checkDate = reprojectionDate.Value;
            timeOpen = Math.Max(0, (int) (checkDate - sivDate.Value).TotalDays);

            return timeOpen;
        }
        
        /// <summary>
        /// Gets the accrual.
        /// </summary>
        /// <param name="accruals">The accruals.</param>
        /// <param name="fieldToAccrue">The field to accrue.</param>
        /// <param name="accrualDate">The accrual date.</param>
        /// <returns>System.Int32.</returns>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static int GetAccrual(IEnumerable<PatientAccrualInformation> accruals, Func<PatientAccrualInformation, DateTime> fieldToAccrue,
            DateTime accrualDate)
        {
            var accrualValue = accruals.Count(x =>
            {
                var dt = fieldToAccrue(x);
                return dt <= accrualDate && dt != DateTime.MinValue;
            });

            return accrualValue;
        }
        public static int GetAccrual(IEnumerable<SSUAccrualInformation> accruals, Func<SSUAccrualInformation, DateTime> fieldToAccrue,
            DateTime accrualDate)
        {
            var accrualValue = accruals.Count(x =>
            {
                var dt = fieldToAccrue(x);
                return dt <= accrualDate && dt != DateTime.MinValue;
            });

            return accrualValue;
        }

        public static bool FPRConstraintAndClosureCheck(int time, SiteParameter site)
        {
            return site.EnrollmentStopDate.Subtract(site.EnrollmentStartDate).Days > 0 && site.EnrollmentStopDate.Subtract(site.EnrollmentStartDate).Days >= time;
        }
    }
}
