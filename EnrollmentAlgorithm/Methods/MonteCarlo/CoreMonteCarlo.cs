using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnrollmentAlgorithm.Objects.Additional;
using EnrollmentAlgorithm.Objects.Enrollment;
using EnrollmentAlgorithm.Objects.Enums;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using Semio.ClientService.Data.Intelligence.Enrollment;

namespace EnrollmentAlgorithm.Methods.MonteCarlo
{
    /// <summary>
    /// Represents the core functionality need to execute the Monte Carlo simulation for both baseline and reprojection scenarios.
    /// </summary>
    public abstract class CoreMonteCarlo
    {
        public Task<TrialParameter> SimulateWithMessages(EnrollmentCollection enrollmentCollection,
            Progress<ProgressReporter> progressTracker)
        {
            var taskArray = new Task[]
                {Task<TrialParameter>.Factory.StartNew(() => Simulate(enrollmentCollection, progressTracker))};

            Task.WaitAll(taskArray);

            var trialParameter = (Task<TrialParameter>) taskArray[0];

            return trialParameter;
        }

        public async Task<TrialParameter> SimulateWithMessages(EnrollmentCollection enrollmentCollection,
            Progress<ProgressReporter> progressTracker, CancellationToken cancel, int reportingInterval = 50)
        {
            var trialParameter = await Task.Run(() => SimulateAsync(enrollmentCollection, progressTracker, cancel, reportingInterval), cancel);

            return trialParameter;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        /// <summary>
        /// Executes the enrollment simulation as a pseudo-async method.  This allows messages to be pumped back as well as ability to receive a cancellation token but the method itself is
        /// lacking the await method by design.
        /// </summary>
        /// <param name="enrollmentCollection">The enrollment collection.</param>
        /// <param name="progressReporter">The progress reporter.</param>
        /// <param name="cancel">The cancel.</param>
        /// <param name="reportingInterval">The reporting interval for the progress Reporter</param>
        /// <returns>Task&lt;TrialParameter&gt;.</returns>
        private async Task<TrialParameter> SimulateAsync(EnrollmentCollection enrollmentCollection, IProgress<ProgressReporter> progressReporter, CancellationToken cancel, int reportingInterval)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var trialParameter = EnrollmentObjectCreation.CreateTrialParameter(enrollmentCollection);
            var siteList = trialParameter.CountryList.SelectMany(s => s.SiteParameters).ToList();

            var screenSuccessDistribution = new Bernoulli(1 - trialParameter.ScreenFailRate);
            // step 1
            GenerateValuesForSimulationUse(siteList, trialParameter.NumberOfIterations, trialParameter.EstimatedLengthOfEnrollment, progressReporter, cancel, reportingInterval);

            var startPoint = GetStartPoint(trialParameter.StudyStartDate);
            var screeningWindowDistribution = new DiscreteUniform(trialParameter.ScreeningPeriodLowerBound, trialParameter.ScreeningPeriodUpperBound);

            // step 2
            trialParameter.ResultsOfSimulation.SimulationValuesList = new List<SimulationValues>(GetPatientAccrual(trialParameter, startPoint, screeningWindowDistribution, screenSuccessDistribution, progressReporter, cancel, reportingInterval));

            try
            {
                // step 3
                SetCaps(trialParameter, progressReporter, cancel, reportingInterval);
            }
            catch (AggregateException aggregateException)
            {
                foreach (var e in aggregateException.Flatten().InnerExceptions)
                {
                    Console.WriteLine(e.Message);
                }
            }

            // step 4
            GetSSUAccrual(trialParameter, progressReporter, cancel, reportingInterval);

            // step 5
            // since there is no simulation iteration for this step, no reportingInterval is needed.  Results are pumped back every 2 countries.
            PrepareForSummarization(trialParameter, progressReporter, cancel);

            // step 6
            BuildSSUMatrix(trialParameter, progressReporter, cancel, reportingInterval);

            // step 7
            BuildSummaryMatrix(trialParameter, progressReporter, cancel, reportingInterval);

            // step 8
            // since there is no simulation iteration for this step, no reportingInterval is needed.  Results are pumped back every 2 countries.
            GenerateAccrualSummary(trialParameter, progressReporter, cancel);

            return trialParameter;
        }








        /// <summary>
        /// Simulates enrollment for a given Infosario Planning canvas
        /// </summary>
        /// <param name="enrollmentCollection">The enrollment collection.</param>
        /// <param name="progressReporter">The progress reporter.</param>
        /// <returns><see cref="TrialParameter" />TrialParameter.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the Bernoulli parameter is not in the range [0,1].</exception>
        public TrialParameter Simulate(EnrollmentCollection enrollmentCollection, IProgress<ProgressReporter> progressReporter = null)
        {
            var trialParameter = EnrollmentObjectCreation.CreateTrialParameter(enrollmentCollection);
            var siteList = trialParameter.CountryList.SelectMany(s => s.SiteParameters).ToList();

            var screenSuccessDistribution = new Bernoulli(1 - trialParameter.ScreenFailRate);
            // step 1
            GenerateValuesForSimulationUse(siteList, trialParameter.NumberOfIterations, trialParameter.EstimatedLengthOfEnrollment, progressReporter, new CancellationToken(),1);

            var startPoint = GetStartPoint(trialParameter.StudyStartDate);
            var screeningWindowDistribution = new DiscreteUniform(trialParameter.ScreeningPeriodLowerBound, trialParameter.ScreeningPeriodUpperBound);
            
            // step 2
            trialParameter.ResultsOfSimulation.SimulationValuesList = new List<SimulationValues>(GetPatientAccrual(trialParameter, startPoint, screeningWindowDistribution, screenSuccessDistribution, progressReporter, new CancellationToken(),1));

            try
            {
                // step 3
                SetCaps(trialParameter, progressReporter, new CancellationToken(),1);
            }
            catch (AggregateException aggregateException)
            {
                foreach (var e in aggregateException.Flatten().InnerExceptions)
                {
                    Console.WriteLine(e.Message);
                }
            }
            
            // step 4
            GetSSUAccrual(trialParameter, progressReporter, new CancellationToken(),1);

            // step 5
            PrepareForSummarization(trialParameter, progressReporter, new CancellationToken());

            // step 6
            BuildSSUMatrix(trialParameter, progressReporter, new CancellationToken(),1);
            
            // step 7
            BuildSummaryMatrix(trialParameter, progressReporter, new CancellationToken(),1);
            
            // step 8
            GenerateAccrualSummary(trialParameter, progressReporter, new CancellationToken());

            return trialParameter;
        }

        /// <summary>
        /// Creates the object(s) necessary for summarizing the data.  This is here so a For loop can be used, which is easier to keep the simulaions in sync between trial and country level
        /// </summary>
        /// <param name="trialParameter">The trial parameter.</param>
        /// <param name="progressReporter">The progress reporter.</param>
        /// <param name="cancel">A token in case user wants to cancel the operation</param>
        private static void PrepareForSummarization(TrialParameter trialParameter, IProgress<ProgressReporter> progressReporter, CancellationToken cancel)
        {
            var someCounter = 0;
            Parallel.ForEach(trialParameter.CountryList, country =>
            {
                country.ResultsOfSimulation.SimulationValuesList = Enumerable.Repeat(new SimulationValues(), trialParameter.NumberOfIterations).ToList();
                someCounter++;
                progressReporter?.Report(GenerateProgressReport("Preparing For Summarization", 5, someCounter, cancel));
            });
        }

        /// <summary>
        /// Wrapper method to facilitate generating the accrual summary.
        /// </summary>
        /// <param name="trialParameter">The trial parameter.</param>
        /// <param name="progressReporter">The progress reporter.</param>
        /// <param name="cancel">A cancellation token that is monitored if user cancels the process.</param>
        private static void GenerateAccrualSummary(TrialParameter trialParameter, IProgress<ProgressReporter> progressReporter, CancellationToken cancel)
        {
            var someCounter = trialParameter.CountryList.Count;
            foreach (var country in trialParameter.CountryList)
            {
                country.SSUSummary = GenerateSSUSummary(country.ResultsOfSimulation.SimulationValuesList);
                country.AccrualSummary = GenerateAccrualSummary(country.ResultsOfSimulation.SimulationValuesList);

                someCounter++;

                if (someCounter % 2 == 0)
                {
                    progressReporter?.Report(GenerateProgressReport("Generating Country Accrual Summary", 8, someCounter,cancel));
                }
            }

            progressReporter?.Report(GenerateProgressReport("Generating Trial Accrual Summary", 8, null, cancel));
            trialParameter.SSUSummary = GenerateSSUSummary(trialParameter.ResultsOfSimulation.SimulationValuesList);
            trialParameter.AccrualSummary = GenerateAccrualSummary(trialParameter.ResultsOfSimulation.SimulationValuesList);
        }

        /// <summary>
        /// Wrapper method for <see cref="ApplyCaps" /> method. Also sets the earliest and latest accrual dates for each iteration (used to standardize date range for summarization).
        /// </summary>
        /// <param name="trialParameter">The trial parameter.</param>
        /// <param name="progressReporter">The progress reporter.</param>
        /// <param name="cancel">A cancellation token that is monitored if user cancels the process.</param>
        /// <param name="reportingInterval">The reporting interval.</param>
        private static void SetCaps(TrialParameter trialParameter, IProgress<ProgressReporter> progressReporter, CancellationToken cancel, int reportingInterval)
        {
            var someCounter = 0;
            //Parallel.ForEach(trialParameter.ResultsOfSimulation.SimulationValuesList, currentIteration =>
            //{
            //    currentIteration.PatientAccrual = ApplyCaps(currentIteration.PatientAccrual, trialParameter.CountryList,
            //        trialParameter.EnrollmentTarget).ToList();
            //    currentIteration.EarliestAccrualDate = currentIteration.PatientAccrual.Min(x => x.ScreeningDate);
            //    currentIteration.LatestAccrualDate = currentIteration.PatientAccrual.Max(
            //        x => new DateTime(Math.Max(x.EnrollmentDate.Ticks, x.RandomizedDate.Ticks)));
            //    someCounter++;
            //    if ((someCounter) % reportingInterval == 0)
            //    {
            //        progressReporter?.Report(GenerateProgressReport("Setting Patient Caps", 3, someCounter,cancel));
            //    }
            //});

            Parallel.For(0, trialParameter.NumberOfIterations, i =>
            {
                var currentIteration = trialParameter.ResultsOfSimulation.SimulationValuesList[i];
                currentIteration.PatientAccrual = ApplyCaps(currentIteration.PatientAccrual, trialParameter.CountryList,trialParameter.EnrollmentTarget).ToList();
                currentIteration.EarliestAccrualDate = currentIteration.PatientAccrual.Min(x => x.ScreeningDate);
                currentIteration.LatestAccrualDate = currentIteration.PatientAccrual.Max(
                    x => new DateTime(Math.Max(x.EnrollmentDate.Ticks, x.RandomizedDate.Ticks)));
                someCounter++;
                if ((someCounter) % reportingInterval == 0)
                {
                    progressReporter?.Report(GenerateProgressReport("Setting Patient Caps", 3, someCounter, cancel));
                }
            });
        }

        /// <summary>
        /// Wrapper for Simulate Patient Accrual.
        /// </summary>
        /// <param name="trialParameter">The trial parameter.</param>
        /// <param name="startPoint">The start point.</param>
        /// <param name="screeningWindowDistribution">The screening window distribution.</param>
        /// <param name="screenSuccessDistribution">The screen success distribution.</param>
        /// <param name="progressReporter">The progress reporter.</param>
        /// <param name="cancel">A cancellation token that is monitored if user cancels the process.</param>
        /// <param name="reportingInterval">The reporting interval.</param>
        /// <returns>ConcurrentBag&lt;SimulationValues&gt;.</returns>
        private static IEnumerable<SimulationValues> GetPatientAccrual(TrialParameter trialParameter, DateTime startPoint,
            DiscreteUniform screeningWindowDistribution, Bernoulli screenSuccessDistribution, IProgress<ProgressReporter> progressReporter, CancellationToken cancel, int reportingInterval)
        {
            // this block is used to generate the number of SimulationValues equal to the number of iterations.  This is done so that a Parallel.For loop can be used to keep data in sync
            var simulationValuesList =
                Enumerable.Repeat(new SimulationValues(), trialParameter.NumberOfIterations).ToList();

            var someCounter = 0;

            Parallel.For(0, trialParameter.NumberOfIterations, i =>
            {
                var simulationValues = new SimulationValues
                {
                    PatientAccrual =
                        SimulatePatientAccrual(trialParameter.CountryList, startPoint, i, trialParameter.EnrollmentTarget,
                            screeningWindowDistribution, screenSuccessDistribution)
                };

                simulationValuesList[i] = simulationValues;
                someCounter++;
                if (someCounter % reportingInterval == 0)
                {
                    progressReporter?.Report(GenerateProgressReport("Generating Patient Accrual", 2, someCounter, cancel));
                }
            });

            return simulationValuesList;
        }



        /// <summary>
        /// Rolls up SSU Accrual to the trial level.
        /// </summary>
        /// <param name="trialParameter">The trial parameter.</param>
        /// <param name="progressReporter">The progress reporter.</param>
        /// <param name="cancel">A cancellation token that is monitored if user cancels the process.</param>
        /// <param name="reportingInterval">The reporting interval.</param>
        private static void GetSSUAccrual(TrialParameter trialParameter, IProgress<ProgressReporter> progressReporter, CancellationToken cancel, int reportingInterval)
        {
            var siteList = trialParameter.CountryList.SelectMany(s => s.SiteParameters).ToList();
            for (var i = 0; i <= trialParameter.NumberOfIterations -1; i++)
            {
                trialParameter.ResultsOfSimulation.SimulationValuesList[i].EarliestSIVDate = siteList.Min(x => x.ResultsOfSimulation.SimulationValuesList[i].SIVDate);
                trialParameter.ResultsOfSimulation.SimulationValuesList[i].LatestSIVDate = siteList.Max(x => x.ResultsOfSimulation.SimulationValuesList[i].SIVDate);
                trialParameter.ResultsOfSimulation.SimulationValuesList[i].EarliestSSVDate = siteList.Min(x => x.ResultsOfSimulation.SimulationValuesList[i].SSVDate);
                trialParameter.ResultsOfSimulation.SimulationValuesList[i].LatestSSVDate = siteList.Max(x => x.ResultsOfSimulation.SimulationValuesList[i].SSVDate);
                
                foreach (var siteRecord in siteList)
                {
                    trialParameter.ResultsOfSimulation.SimulationValuesList[i].SSUAccrual.Add(new SSUAccrualInformation
                    {
                        Country=siteRecord.CountryName,
                        Site = siteRecord.Name,
                        SIVDate = siteRecord.ResultsOfSimulation.SimulationValuesList[i].SIVDate,
                        SSVDate = siteRecord.ResultsOfSimulation.SimulationValuesList[i].SSVDate,
                        SSUTime = siteRecord.ResultsOfSimulation.SimulationValuesList[i].SSUValue
                    });

                }
                if ((i+1) % reportingInterval == 0)
                {
                    progressReporter?.Report(GenerateProgressReport("Calculating SSU Accrual", 4, i+1,cancel));
                }
            }
        }

        /// <summary>
        /// Generates the ssu summary by day for a given set of simulations.
        /// </summary>
        /// <param name="simulationValues">The simulation values.</param>
        /// <returns>List&lt;SummarizedSSUResults&gt;.</returns>
        private static List<SummarizedSSUResults> GenerateSSUSummary(List<SimulationValues> simulationValues)
        {
            var summaryResults = new ConcurrentBag<SummarizedSSUResults>();

            var matrixStart = simulationValues.Min(x => x.EarliestSSVDate);
            var matrixEnd = simulationValues.Max(x => x.LatestSIVDate);

            var maxSSV = simulationValues.Select(x => x.CumulatedSSV[x.LatestSSVDate]).First();
            var maxSIV = simulationValues.Select(x => x.CumulatedSIV[x.LatestSIVDate]).First();

            var loopMax = (int)matrixEnd.Subtract(matrixStart).TotalDays + 1;

            if (matrixStart == DateTime.MinValue) return new List<SummarizedSSUResults>();

            Parallel.For(0, loopMax, i =>
            {
                var counter = matrixStart.AddDays(i);

                var ssvDayValues =
                    simulationValues.Select(x => (double)x.CumulatedSSV.GetValueOrDefault(counter,
                        () => counter < x.EarliestSSVDate ? 0 : x.CumulatedSSV[x.LatestSSVDate]))
                        .OrderBy(v => v).ToArray();

                var sivDayValues =
                    simulationValues.Select(x => (double)x.CumulatedSIV.GetValueOrDefault(counter,
                        () => counter < x.EarliestSIVDate ? 0 : x.CumulatedSIV[x.LatestSIVDate]))
                        .OrderBy(v => v)
                        .ToArray();

                summaryResults.Add(new SummarizedSSUResults
                {
                    AccrualDate = counter,
                    SSVPercentiles = CalculatePercentiles(ssvDayValues),
                    SIVPercentiles = CalculatePercentiles(sivDayValues),
                    SSVMeanAndStdDev = CalculateMeanAndStdDev(ssvDayValues, maxSSV),
                    SIVMeanAndStdDev = CalculateMeanAndStdDev(sivDayValues, maxSIV),
                    RawSSVValues = ssvDayValues,
                    RawSIVValues = sivDayValues
                });
            });

            return summaryResults.OrderBy(x => x.AccrualDate).ToList();
        }
        
        /// <summary>
        /// Generates the accrual summary and percentile values by day for reporting purposes.  For accurate summarization, each iteration must account for enrollment at each day across all
        /// iterations.  The extension method used in the parallel loop fills in missing dates at the simulation level.  For instance, if the counter value (which is a date) is before the earliest 
        /// date value for that specific simulation, it is safe to assume that no enrollment occurred for that date in that simulation, so a 0 can be used. If the counter value is greater than
        /// the max date of that simulation, it is assumed enrollment has stopped and the value from the max date of the simulation results can be used for the counter value.  Note that the
        /// date values within each simulation is populated in the <see cref="BuildSummaryMatrix(TrialParameter,System.IProgress{EnrollmentAlgorithm.Objects.Additional.ProgressReporter},CancellationToken,int)"/> method.
        /// </summary>
        /// <param name="simulationValues">The simulation values.</param>
        /// <returns cref="SummarizedAccrualResults">List&lt;SummarizedResults&gt;. A list of summarized results, each item in list represents a SummarizedResult</returns>
        
        private static List<SummarizedAccrualResults> GenerateAccrualSummary(List<SimulationValues> simulationValues)
        {
            var summaryResults = new ConcurrentBag<SummarizedAccrualResults>();

            var matrixStart = simulationValues.Min(x => x.EarliestAccrualDate);
            var matrixEnd = simulationValues.Max(x => x.LatestAccrualDate);

            var loopMax = (int)matrixEnd.Subtract(matrixStart).TotalDays + 1;

            if(matrixStart == DateTime.MinValue) return new List<SummarizedAccrualResults>();

            var maxScreened = simulationValues.Select(x => x.CumulatedScreened[x.LatestAccrualDate]).First();
            var maxEnrolled = simulationValues.Select(x => x.CumulatedEnrolled[x.LatestAccrualDate]).First();
            var maxRandomized = simulationValues.Select(x => x.CumulatedRandomized[x.LatestAccrualDate]).First();

            Parallel.For(0, loopMax, i =>
            {
                var counter = matrixStart.AddDays(i);

                var enrollmentDayValues =
                    simulationValues.Select(x => (double) x.CumulatedEnrolled.GetValueOrDefault(counter,
                        () => counter < x.EarliestAccrualDate ? 0 : x.CumulatedEnrolled[x.LatestAccrualDate]))
                        .OrderBy(v => v).ToArray();

                var screeningDayValues =
                    simulationValues.Select(x => (double) x.CumulatedScreened.GetValueOrDefault(counter,
                        () => counter < x.EarliestAccrualDate ? 0 : x.CumulatedScreened[x.LatestAccrualDate]))
                        .OrderBy(v => v)
                        .ToArray();

                var randomizedDayValues =
                    simulationValues.Select(x => (double) x.CumulatedRandomized.GetValueOrDefault(counter,
                        () => counter < x.EarliestAccrualDate ? 0 : x.CumulatedRandomized[x.LatestAccrualDate]))
                        .OrderBy(v => v)
                        .ToArray();

                summaryResults.Add(new SummarizedAccrualResults
                {
                    AccrualDate = counter,
                    EnrollmentPercentiles = CalculatePercentiles(enrollmentDayValues),
                    ScreeningPercentiles = CalculatePercentiles(screeningDayValues),
                    RandomizationPercentiles = CalculatePercentiles(randomizedDayValues),
                    ScreeningMeanAndStdDev = CalculateMeanAndStdDev(screeningDayValues, maxScreened),
                    EnrollmentMeanAndStdDev = CalculateMeanAndStdDev(enrollmentDayValues, maxEnrolled),
                    RandomizationMeanAndStdDev = CalculateMeanAndStdDev(randomizedDayValues, maxRandomized),
                    RawScreeningValues = screeningDayValues,
                    RawEnrollmentValues = enrollmentDayValues,
                    RawRandomizationValues = randomizedDayValues
                });
            });
            return summaryResults.OrderBy(x => x.AccrualDate).ToList();
        }

        /// <summary>
        /// In order to summarize accurately, each simulation needs to have the same number of days.This is the first step in squaring up the simulations for summarization.
        /// </summary>
        /// <param name="trialParameter">The trial parameter.</param>
        /// <param name="progressReporter">The progress reporter.</param>
        /// <param name="cancel">A cancellation token that is monitored if user cancels the process.</param>
        /// <param name="reportingInterval">The reporting interval.</param>
        private static void BuildSSUMatrix(TrialParameter trialParameter, IProgress<ProgressReporter> progressReporter, CancellationToken cancel, int reportingInterval)
        {
            var someCounter = 0;
            Parallel.For(0, trialParameter.NumberOfIterations, i =>
            {
                var currentSimulation = trialParameter.ResultsOfSimulation.SimulationValuesList[i];
                var matrixStart = currentSimulation.EarliestSSVDate;
                var matrixEnd = currentSimulation.LatestSIVDate;

                var trialCumulatedSIV = new SortedList<DateTime, int>();
                var trialCumulatedSSV = new SortedList<DateTime, int>();

                foreach (var country in trialParameter.CountryList)
                {
                    var countryValues = currentSimulation.SSUAccrual.Where(x => x.Country == country.Name).ToList();

                    if (countryValues.Count <= 0) continue;
                    {
                        var currentCountrySimulationValues = new SimulationValues
                        {
                            EarliestSIVDate = countryValues.Min(x => x.SIVDate),
                            LatestSIVDate = countryValues.Max(x => x.SIVDate),
                            EarliestSSVDate = countryValues.Min(x => x.SSVDate),
                            LatestSSVDate = countryValues.Max(x => x.SSVDate)
                        };

                        for (var dateCounter = matrixStart; dateCounter <= matrixEnd; dateCounter = dateCounter.AddDays(1))
                        {
                            var counter = dateCounter;
                            currentCountrySimulationValues.CumulatedSIV.Add(counter,
                                HelperMethods.GetAccrual(countryValues, y => y.SIVDate, counter));
                            currentCountrySimulationValues.CumulatedSSV.Add(counter,
                                HelperMethods.GetAccrual(countryValues, y => y.SSVDate, counter));

                            if (!trialCumulatedSSV.ContainsKey(counter))
                            {
                                trialCumulatedSSV.Add(counter,
                                    currentCountrySimulationValues.CumulatedSSV[counter]);
                                trialCumulatedSIV.Add(counter,
                                    currentCountrySimulationValues.CumulatedSIV[counter]);
                                continue;
                            }

                            trialCumulatedSSV[counter] +=
                                currentCountrySimulationValues.CumulatedSSV[counter];
                            trialCumulatedSIV[counter] +=
                                currentCountrySimulationValues.CumulatedSIV[counter];
                        }

                        country.ResultsOfSimulation.SimulationValuesList[i] = currentCountrySimulationValues;
                    }
                }
                trialParameter.ResultsOfSimulation.SimulationValuesList[i].CumulatedSSV = trialCumulatedSSV;
                trialParameter.ResultsOfSimulation.SimulationValuesList[i].CumulatedSIV = trialCumulatedSIV;

                someCounter++;
                if (someCounter % reportingInterval == 0)
                {
                    progressReporter?.Report(GenerateProgressReport("Building SSU Matrix", 6, someCounter, cancel));
                }
            });

        }

        //ToDo: Contemplate refactoring this and the method above in to a single method...

        /// <summary>
        /// In order to summarize accurately, each simulation needs to have the same number of days.  This is the first step in squaring up the simulations for summarization.
        /// </summary>
        /// <param name="trialParameter">The trial parameter.</param>
        /// <param name="progressReporter">The progress reporter.</param>
        /// <param name="cancel">A cancellation token that is monitored if user cancels the process.</param>
        /// <param name="reportingInterval">The reporting interval.</param>
        private static void BuildSummaryMatrix(TrialParameter trialParameter, IProgress<ProgressReporter> progressReporter, CancellationToken cancel, int reportingInterval)
        {
            var someCounter = 0;
            Parallel.For(0, trialParameter.NumberOfIterations, i =>
            {
                var currentSimulation = trialParameter.ResultsOfSimulation.SimulationValuesList[i];
                var matrixStart = currentSimulation.EarliestAccrualDate;
                var matrixEnd = currentSimulation.LatestAccrualDate;

                var trialCumulatedScreened = new SortedList<DateTime, int>();
                var trialCumulatedEnrolled = new SortedList<DateTime, int>();
                var trialCumulatedRandomized = new SortedList<DateTime, int>();


                foreach (var country in trialParameter.CountryList)
                {
                    var countryValues = currentSimulation.PatientAccrual.Where(x => x.Country == country.Name).ToList();

                    if (countryValues.Count <= 0) continue;
                    {
                        var currentCountrySimulationValues = country.ResultsOfSimulation.SimulationValuesList[i];

                        currentCountrySimulationValues.EarliestAccrualDate = countryValues.Min(x => x.ScreeningDate);
                        currentCountrySimulationValues.LatestAccrualDate =
                            countryValues.Max(
                                x => new DateTime(Math.Max(x.EnrollmentDate.Ticks, x.RandomizedDate.Ticks)));

                        for (var dateCounter = matrixStart; dateCounter <= matrixEnd; dateCounter = dateCounter.AddDays(1))
                        {
                            var counter = dateCounter;
                            currentCountrySimulationValues.CumulatedScreened.Add(counter,
                                                    HelperMethods.GetAccrual(countryValues, y => y.ScreeningDate, counter));
                            currentCountrySimulationValues.CumulatedEnrolled.Add(counter,
                                HelperMethods.GetAccrual(countryValues, y => y.EnrollmentDate, counter));
                            currentCountrySimulationValues.CumulatedRandomized.Add(counter,
                                HelperMethods.GetAccrual(countryValues, y => y.RandomizedDate, counter));

                            if (!trialCumulatedScreened.ContainsKey(counter))
                            {
                                trialCumulatedScreened.Add(counter,
                                    currentCountrySimulationValues.CumulatedScreened[counter]);
                                trialCumulatedEnrolled.Add(counter,
                                        currentCountrySimulationValues.CumulatedEnrolled[counter]);
                                trialCumulatedRandomized.Add(counter,
                                        currentCountrySimulationValues.CumulatedRandomized[counter]);
                                continue;
                            }

                            trialCumulatedScreened[counter] +=
                                    currentCountrySimulationValues.CumulatedScreened[counter];
                            trialCumulatedEnrolled[counter] +=
                                currentCountrySimulationValues.CumulatedEnrolled[counter];
                            trialCumulatedRandomized[counter] +=
                                currentCountrySimulationValues.CumulatedRandomized[counter];
                        }
                        country.ResultsOfSimulation.SimulationValuesList[i] = currentCountrySimulationValues;
                    }
                }
                trialParameter.ResultsOfSimulation.SimulationValuesList[i].CumulatedScreened = trialCumulatedScreened;
                trialParameter.ResultsOfSimulation.SimulationValuesList[i].CumulatedEnrolled = trialCumulatedEnrolled;
                trialParameter.ResultsOfSimulation.SimulationValuesList[i].CumulatedRandomized = trialCumulatedRandomized;

                someCounter++;
                if (someCounter % reportingInterval == 0)
                {
                    progressReporter?.Report(GenerateProgressReport("Building Accrual Matrix", 7, someCounter, cancel));
                }
            });
        }

        /// <summary>
        /// Calculates percentiles based on accrual numbers.
        /// </summary>
        /// <param name="values">An array of doubles.  Specifically, the accrual values for each iteration for a specific day.</param>
        /// <returns cref="Percentiles">An object that holds the percentiles that are to visualized in Infosario Planning.</returns>
        private static Percentiles CalculatePercentiles(double[] values)
        {
            return new Percentiles
            {
                Fifth = SortedArrayStatistics.Percentile(values, 5),
                Tenth = SortedArrayStatistics.Percentile(values, 10),
                Twentieth = SortedArrayStatistics.Percentile(values, 20),
                TwentyFifth = SortedArrayStatistics.Percentile(values, 25),
                Thirtieth = SortedArrayStatistics.Percentile(values, 30),
                Fortieth = SortedArrayStatistics.Percentile(values, 40),
                Fiftieth = SortedArrayStatistics.Percentile(values, 50),
                Sixtieth = SortedArrayStatistics.Percentile(values, 60),
                Seventieth = SortedArrayStatistics.Percentile(values, 70),
                SeventyFifth = SortedArrayStatistics.Percentile(values, 75),
                Eightieth = SortedArrayStatistics.Percentile(values, 80),
                Ninetieth = SortedArrayStatistics.Percentile(values, 90),
                NinetyFifth = SortedArrayStatistics.Percentile(values, 95)
            };
        }

        /// <summary>
        /// Calculates the mean, standard dev., and error estimates.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="maxValue">The maximum value, used to keep +1 StdDev from going over max expected value</param>
        /// <returns>MeanAndErrorEstimates.</returns>
        private static MeanAndErrorEstimates CalculateMeanAndStdDev(double[] values, int maxValue)
        {
            var mean = values.Average();
            var stdDev = values.StandardDeviation();
            return new MeanAndErrorEstimates
            {
                Mean = mean,
                StandardDedviation = stdDev,
                PlusOneStdDev = Math.Min(maxValue, mean + stdDev),
                MinusOneStdDev = Math.Max(0,mean - stdDev)
            };
        }

        /// <summary>
        /// Simulates the patient accrual.
        /// </summary>
        /// <param name="countryList">The country list.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="iterationCounter">The iteration counter.</param>
        /// <param name="overallEnrollmentTarget">The overall enrollment target.</param>
        /// <param name="screeningWindowDistribution">The screening window distribution.</param>
        /// <param name="screenSuccessDistribution">The screen success distribution.</param>
        /// <returns cref="PatientAccrualInformation">List&lt;PatientAccrualInformation&gt;.</returns>
        private static List<PatientAccrualInformation> SimulatePatientAccrual(IEnumerable<CountryParameter> countryList, DateTime startDate, int iterationCounter, int overallEnrollmentTarget, IDiscreteDistribution screeningWindowDistribution, IDiscreteDistribution screenSuccessDistribution)
        {
            var trialEnrollment = new List<PatientAccrualInformation>();
            foreach (var country in countryList)
            {
                var continueEnrolling = true;
                var day = -1;
                var countryEnrollment = country.CurrentActualEnrollment;

                while (continueEnrolling)
                {
                    day++;
                    var currentDateOfSimulation = startDate.AddDays(day);

                    foreach (var site in country.SiteParameters)
                    {
                        if (currentDateOfSimulation < site.ResultsOfSimulation.SimulationValuesList[iterationCounter].SIVDate || currentDateOfSimulation >= site.EnrollmentStopDate) continue;

                        //ToDO: Adjustments to screening rates (like slowdown to enrollment or a break) need to happen here (recreate a Poisson Distribution w/ the adjusted screening rate)

                        var numberScreened = site.ResultsOfSimulation.SimulationValuesList[iterationCounter].ScreeningValues.ElementAtOrDefault(day, () => new Poisson(site.ResultsOfSimulation.SimulationValuesList[iterationCounter].InitialScreeningRate).Sample());

                        if (numberScreened <= 0) continue;
                        var patientAccrualInformation = PopulateEnrollment(day, numberScreened, screeningWindowDistribution, screenSuccessDistribution, currentDateOfSimulation, site.Name, site.CountryName);

                        trialEnrollment.AddRange(patientAccrualInformation.PatientAccrualInformation);
                        countryEnrollment += patientAccrualInformation.EnrollmentCounter;
                    }
                    continueEnrolling = ContinueEnrolling(country, countryEnrollment, overallEnrollmentTarget);
                }
            }
            return trialEnrollment;
        }

        /// <summary>
        /// A check on current country level values for that day to see if enrollment should continue for another day.
        /// </summary>
        /// <param name="country">The country.</param>
        /// <param name="countryEnrollment">The country enrollment.</param>
        /// <param name="overallEnrollmentTarget">The overall enrollment target.</param>
        /// <returns><c>true</c> if enrollment should continue for another day, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Unexpected EnrollmentAccrualConstraint.</exception>
        protected static bool ContinueEnrolling(CountryParameter country, int countryEnrollment, int overallEnrollmentTarget)
        {
            switch (country.AccrualConstraint)
            {
                case EnrollmentAccrualConstraint.MinimumPatient:
                case EnrollmentAccrualConstraint.ExactPatient:
                    return countryEnrollment < country.MinPatientEnrollment;
                case EnrollmentAccrualConstraint.MaximumPatient:
                case EnrollmentAccrualConstraint.Between:
                    return countryEnrollment < country.MaxPatientEnrollment;
                case EnrollmentAccrualConstraint.None:
                    return countryEnrollment < overallEnrollmentTarget;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Determines if virtual patients in screening period are successfully screened and enrolled.
        /// </summary>
        /// <param name="day">The day.</param>
        /// <param name="numberScreened">The number screened.</param>
        /// <param name="screeningWindowDistribution">The screening window distribution.</param>
        /// <param name="screenSuccessDistribution">The screen success distribution.</param>
        /// <param name="currentDate">The current date.</param>
        /// <param name="siteName">Name of the site.</param>
        /// <param name="countryName">Name of the country.</param>
        /// <returns cref="AccruedValues">AccruedValues.</returns>
        private static AccruedValues PopulateEnrollment(int day, int numberScreened, IDiscreteDistribution screeningWindowDistribution, IDiscreteDistribution screenSuccessDistribution, DateTime currentDate, string siteName, string countryName)
        {
            var patientAccrualList = new List<PatientAccrualInformation>();
            var enrollmentCounter = 0;
            for (var i = 1; i <= numberScreened; i++)
            {
                var patientInformation = new PatientAccrualInformation
                {
                    ScreeningDate = currentDate, ScreeningDay = day, Site = siteName, Country = countryName
                };

                var enrollmentSuccess = screenSuccessDistribution.Sample();

                if (enrollmentSuccess != 1)
                {
                    patientAccrualList.Add(patientInformation);
                    continue;
                }

                var daysToCompleteScreening = screeningWindowDistribution.Sample();

                patientInformation.EnrollmentDay = day + daysToCompleteScreening;
                patientInformation.EnrollmentDate = currentDate.AddDays(daysToCompleteScreening);
                patientAccrualList.Add(patientInformation);
                enrollmentCounter++;
            }
            return new AccruedValues
            {
                PatientAccrualInformation = patientAccrualList, EnrollmentCounter = enrollmentCounter
            };
        }

        /// <summary>
        /// Generates the values that can be pre-calculated which are used in the Monte Carlo simulation.
        /// </summary>
        /// <param name="siteList">The site list.</param>
        /// <param name="numberOfIterations">The number of iterations.</param>
        /// <param name="calculatedEnrollmentDays">The calculated enrollment days.</param>
        /// <param name="progressReporter">The progress reporter.</param>
        /// <param name="cancel">A cancellation token that is monitored if user cancels the process.</param>
        /// <param name="reportingInterval">The reporting interval.</param>
        private void GenerateValuesForSimulationUse(List<SiteParameter> siteList, int numberOfIterations, int calculatedEnrollmentDays, IProgress<ProgressReporter> progressReporter, CancellationToken cancel, int reportingInterval)
        {
            
            for (var i = 0; i < numberOfIterations; i++)
            {
                foreach (var site in siteList)
                {
                    //generated from SSU uniform distribution
                    var ssuValue = GenerateSSUValue(site);
                    //Screening rate developed from sample of baselineScreening Distribution.
                    var initialScreeningRate = GenerateScreeningValue(site);
                    var screeningValues = new int[calculatedEnrollmentDays];

                    //CalculatedEnrollmentDays is populated based on initialScreenRate sampled from baselineScreening Distribution.
                    new Poisson(initialScreeningRate).Samples(screeningValues);

                    site.ResultsOfSimulation.SimulationValuesList.Add(new SimulationValues
                    {
                        InitialScreeningRate = initialScreeningRate,
                        SSUValue = ssuValue,
                        SIVDate = GenerateSIVDate(site, ssuValue),
                        ScreeningValues = screeningValues,
                        SSVDate = site.SiteSelectionVisit ?? DateTime.Now
                    });
                }

                if ((i+1) % reportingInterval == 0)
                {
                    progressReporter?.Report(GenerateProgressReport("Generating Values For Simulation", 1, i+1,cancel));
                }
            }
        }

        /// <summary>
        /// Applies the caps to each simulation iteration.
        /// </summary>
        /// <param name="totalAccrualInformation">The total accrual information.</param>
        /// <param name="countryList">The country list.</param>
        /// <param name="totalEnrollment">The total enrollment.</param>
        /// <returns cref="PatientAccrualInformation">IEnumerable&lt;PatientAccrualInformation&gt;.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static IEnumerable<PatientAccrualInformation> ApplyCaps(List<PatientAccrualInformation> totalAccrualInformation, List<CountryParameter> countryList, int totalEnrollment)
        {
            var trimmedList = new List<PatientAccrualInformation>();
            var mandatoryList = new List<PatientAccrualInformation>();
            var possibleList = new List<PatientAccrualInformation>();

            var totalMinimum = countryList.Sum(x => x.MinPatientEnrollment);
            var remainingEnrollment = totalEnrollment - totalMinimum;

            var screenFailureList = totalAccrualInformation.Where(x => x.EnrollmentDate == DateTime.MinValue).ToList();
            totalAccrualInformation.RemoveAll(x => x.EnrollmentDate == DateTime.MinValue);

            foreach (var country in countryList)
            {
                if (country.CurrentActualEnrollment > 0 && country.CurrentActualEnrollment >= country.MaxPatientEnrollment) continue;

                var tempList = totalAccrualInformation.Where(x => x.Country == country.Name).OrderBy(y => y.EnrollmentDate).ToList();

                var adjustedMinimum = Math.Max(0, country.MinPatientEnrollment - country.CurrentActualEnrollment);

                var currentPossibleCountryEnrollment = country.CurrentActualEnrollment + tempList.Count;

                switch (country.AccrualConstraint)
                {
                    case EnrollmentAccrualConstraint.MinimumPatient:
                        mandatoryList.AddRange(tempList.Take(adjustedMinimum));
                        tempList.RemoveRange(0, adjustedMinimum - 1);
                        break;
                    case EnrollmentAccrualConstraint.MaximumPatient:
                        tempList.RemoveRange(country.MaxPatientEnrollment - 1, currentPossibleCountryEnrollment - country.MaxPatientEnrollment);
                        break;
                    case EnrollmentAccrualConstraint.ExactPatient:
                        mandatoryList.AddRange(tempList.Take(adjustedMinimum));
                        tempList.RemoveAll(x => x.Country == country.Name);
                        break;
                    case EnrollmentAccrualConstraint.Between:
                        mandatoryList.AddRange(tempList.Take(adjustedMinimum));
                        tempList.RemoveRange(0, adjustedMinimum);
                        var maxPossibleRemaining = country.MaxPatientEnrollment - adjustedMinimum;
                        if (tempList.Count > maxPossibleRemaining)
                        {
                            tempList = tempList.Take(maxPossibleRemaining).ToList();
                        }
                        break;
                    case EnrollmentAccrualConstraint.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                possibleList.AddRange(tempList);
            }

            trimmedList.AddRange(mandatoryList);
            trimmedList.AddRange(possibleList.OrderBy(x => x.EnrollmentDate).Take(remainingEnrollment));
            trimmedList.AddRange(screenFailureList);
            return trimmedList.OrderBy(x => x.EnrollmentDate);
        }
        
        private static ProgressReporter GenerateProgressReport(string friendlyMessage, int currentStep, int? currentIteration, CancellationToken cancel)
        {
            if (cancel.IsCancellationRequested)
            {
                cancel.ThrowIfCancellationRequested();
            }

            return new ProgressReporter
            {
                FriendlyMessage = friendlyMessage,
                IterationCounter = currentIteration,
                CurrentStep = currentStep
            };
        }

        protected abstract double GenerateSSUValue(SiteParameter site);
        protected abstract double GenerateScreeningValue(SiteParameter site);
        protected abstract DateTime GenerateSIVDate(SiteParameter site, double ssuValue);
        protected abstract DateTime GetStartPoint(DateTime trialStartDate);
    }
}
