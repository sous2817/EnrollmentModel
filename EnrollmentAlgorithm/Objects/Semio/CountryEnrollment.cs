using Newtonsoft.Json;
using Semio.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Serialization;

namespace Semio.ClientService.Data.Intelligence.Enrollment
{
    /// <summary>
    /// Enrollment data for a country
    /// </summary>
    [Serializable]
    [XmlType(AnonymousType = true)]
    public class CountryEnrollment : Bindable, ICloneable
    {
        public static string[] IrbCountries =
        {
            "CAN",
            "PRI",
            "USA",
            "UMI",
            "The Republic of Woodruff"
        };

        private ObservableCollection<InvestigatorEnrollment> _sites = new ObservableCollection<InvestigatorEnrollment>();

        //private string _country;
        private int _ssu;

        private double _ssuVariance;
        private double _ssUStandardDeviation;
        private double _eventRate;
        private double _eventOffset;

        private int _addedSites;
        private int _averageScore;
        private int _weightedTierScoreId;
        private int _maxPatients;
        private int _maxSites;
        private int _priority;
        private double _monthlyAccrual;
        private double _monthlyAccrualExpected;
        private double? _monthlyAccrualObserved;
        private double _monthlyAccrualVariance;
        private double _monthlyAccrualStandardDeviation;

        //private int regulatoryLeadTime;
        private int _materialLeadTime;

        private bool _isChecked;
        private string _notes;
        private string _comments;
        private string _group;
        private bool _isUserModified;
        private string _priorState;

        private double _additionalSsvLagInWeeks;
        private double _ssvPerWeekRate;
        private int _ssvWindowInDays;

        private int? _siteCountAvailable;
        private int _siteCountContributing;
        private int _siteCountSelected;

        private double _patientFirstMonth;
        private double _patientLastMonth;

        private double _siteOpenFirstMonth;
        private double _siteOpenLastMonth;

        private int? _minSsuPercentage;
        private int _minSsuDays;
        private int? _maxSsuPercentage;
        private int _maxSsuDays;
        private DateTime? _enrollmentStopDate;
        private DateTime? _earliestFprConstraint;

        private DateTime? _earliestFPRCalc;
        private DateTime? _earliestStartupPlusScreening;
        private decimal? _enrollmentWindowInMonths;
        private decimal? _enrollmentWindowInMonthsFull;

        private int _patientCountPredicted;

        // LPR Priority Fields
        private int _lprSiteCountContributing;

        private int _lprPatientCountPredicted;
        private decimal? _lprEnrollmentWindowInMonths;

        // Display fields for binding
        private int _siteCountContributingDisplay;

        private int _patientCountPredictedDisplay;
        private decimal? _enrollmentWindowInMonthsDisplay;

        /// <summary>
        /// Used to capture the original state of an investigator enrollment as it appeard from the
        /// feed in the case that the user modified the original values.
        /// </summary>
        [XmlAttributeAttribute]
        public string PriorState
        {
            get { return _priorState; }
            set
            {
                _priorState = value;
                NotifyPropertyChanged(() => PriorState);
            }
        }

        /// <summary>
        /// Gets or sets the regulatory lead time.
        /// </summary>
        /// <value>The regulatory lead time.</value>
        [XmlAttribute]
        public int RegulatoryLeadTime { get; set; }

        /// <summary>
        /// Flags if the country enrollment was modified by the user. This is in contrast to an
        /// enrollment that is a default from the enrollment feed.
        /// </summary>
        [XmlAttributeAttribute]
        public bool IsUserModified
        {
            get { return _isUserModified; }
            set
            {
                _isUserModified = value;
                NotifyPropertyChanged(() => IsUserModified);
            }
        }

        /// <summary>
        /// </summary>
        [XmlAttributeAttribute]
        public double EventRate
        {
            get { return _eventRate; }
            set
            {
                _eventRate = value;
                NotifyPropertyChanged(() => EventRate);
            }
        }

        /// <summary>
        /// </summary>
        [XmlAttributeAttribute]
        public double EventOffset
        {
            get { return _eventOffset; }
            set
            {
                _eventOffset = value;
                NotifyPropertyChanged(() => EventOffset);
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("{0} :{5}: SSU={1}  MonthlyAccural={2}  MaxPatients={3}  RequiredPatients={6}  IsAdded={7}   AvergeScore={4}",
                                 Country,
                                 SSU,
                                 MonthlyAccrual,
                                 MaxPatients,
                                 AverageScore,
                                 Sites.Count,
                                 RequiredPatients,
                                 IsAdded
                );
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is added.
        /// </summary>
        /// <value><c>true</c> if this instance is added; otherwise, <c>false</c>.</value>
        public bool IsAdded { get; set; }

        private Collection<InvestigatorEnrollment> _serializedSite;

        ///<summary>
        ///</summary>
        public Collection<InvestigatorEnrollment> SerializedSite
        {
            get { return _serializedSite; }
            set { _serializedSite = value; }
        }

        /// <summary>
        /// Gets or sets the enrollment sites.
        /// </summary>
        /// <value>The sites.</value>
        [XmlElementAttribute("InvestigatorEnrollment", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ObservableCollection<InvestigatorEnrollment> Sites
        {
            get { return _sites; }
            set
            {
                //if(_sites != value && _sites != null)
                //{
                //    _sites.ItemPropertyChanged -= SiteSelectionItemsItemPropertyChanged;
                //}
                _sites = value;
                //if (_sites != null)
                //{
                //    _sites.ItemPropertyChanged +=
                //        SiteSelectionItemsItemPropertyChanged;
                //_sites.CollectionChanged +=
                //    SitesCollectionChanged;
                //}
                NotifyPropertyChanged(() => Sites);

                //CalulateTotals(null, null);
            }
        }

        #region PROPERTY: RequiredPatients

        private int _requiredPatients;

        /// <summary>
        /// Gets or sets the RequiredPatients.
        /// </summary>
        [XmlAttributeAttribute]
        public int RequiredPatients
        {
            get { return _requiredPatients; }
            set
            {
                if (_requiredPatients == value) return;
                _requiredPatients = value;
                NotifyPropertyChanged(() => RequiredPatients);
            }
        }

        #endregion PROPERTY: RequiredPatients

        #region PROPERTY: RequiredPatientsMax

        private int _requiredPatientsMax;

        /// <summary>
        /// Gets or sets the RequiredPatientsMax.
        /// </summary>
        [XmlAttributeAttribute]
        public int RequiredPatientsMax
        {
            get { return _requiredPatientsMax; }
            set
            {
                if (_requiredPatientsMax == value) return;
                _requiredPatientsMax = value;
                NotifyPropertyChanged(() => RequiredPatientsMax);
            }
        }

        #endregion PROPERTY: RequiredPatientsMax

        #region PROPERTY: NumberSitesUsedForSelectionDisplay

        private int _numberSitesUsedForSelectionDisplay;

        /// <summary>
        /// Gets or sets the NumberSitesUsedForSelectionDisplay.
        /// </summary>
        /// <value>The start date.</value>
        public int NumberSitesUsedForSelectionDisplay
        {
            get
            {
                if (_numberSitesUsedForSelectionDisplay == 0)
                    _numberSitesUsedForSelectionDisplay = Sites.Count;

                return _numberSitesUsedForSelectionDisplay;
            }
            set
            {
                _numberSitesUsedForSelectionDisplay = value;
                NotifyPropertyChanged(() => NumberSitesUsedForSelectionDisplay);
            }
        }

        #endregion PROPERTY: NumberSitesUsedForSelectionDisplay

        /// <summary>
        /// helper property
        /// </summary>
        public bool HasSites
        {
            get { return Sites.Any(); }
        }

        /// <summary>
        /// Gets the number of include sites.
        /// </summary>
        /// <value>The number of include sites.</value>
        public int NumberOfIncludeSites
        {
            get { return Sites.Count(x => x.IsChecked); }
        }

        //helper property for selected sites
        public bool HasSelectedSites
        {
            get { return Sites.Any(x => x.IsChecked); }
        }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        /// <value>The country.</value>
        [XmlAttributeAttribute]
        public string Country
        {
            get { return InvestigationalEntityLocation.Address.Country.Name; }
            set
            {
                InvestigationalEntityLocation.Address.Country.Name = value;
                NotifyPropertyChanged(() => Country);
            }
        }

        [XmlIgnore, JsonIgnore]
        public InvestigationalEntityLocation InvestigationalEntityLocation { get; set; }

        [XmlIgnore, JsonIgnore]
        public InvestigationalEntityRegion InvestigationalEntityRegion { get; set; }

        /// <summary>
        /// Gets or sets the countryId.
        /// </summary>
        /// <value>The country.</value>
        [XmlAttributeAttribute]
        public string CountryId
        {
            get { return InvestigationalEntityLocation.Address.Country.IsoCode; }
            set { InvestigationalEntityLocation.Address.Country.IsoCode = value; }
        }

        /// <summary>
        /// Gets or sets the number of added sites.
        /// </summary>
        /// <value>The number of added sites.</value>
        [XmlAttributeAttribute]
        public int AddedSites
        {
            get { return _addedSites; }
            set
            {
                _addedSites = value;
                NotifyPropertyChanged(() => AddedSites);
            }
        }

        public double AdditionalSsvLagInWeeks
        {
            get { return _additionalSsvLagInWeeks; }
            set
            {
                _additionalSsvLagInWeeks = value;
                NotifyPropertyChanged(() => AdditionalSsvLagInWeeks);
            }
        }

        public double SsvPerWeekRate
        {
            get { return _ssvPerWeekRate; }
            set
            {
                _ssvPerWeekRate = value;
                NotifyPropertyChanged(() => SsvPerWeekRate);
            }
        }

        public int SsvWindowInDays
        {
            get { return _ssvWindowInDays; }
            set { _ssvWindowInDays = value; }
        }

        /// <summary>
        /// Gets or sets the material lead time.
        /// </summary>
        /// <value>The material lead time.</value>
        [XmlAttributeAttribute]
        public int MaterialLeadTime
        {
            get { return _materialLeadTime; }
            set
            {
                _materialLeadTime = value;
                NotifyPropertyChanged(() => MaterialLeadTime);
            }
        }

        /// <summary>
        /// Gets or sets the SSU.
        /// </summary>
        /// <value>The SSU.</value>
        [XmlAttributeAttribute]

        // ReSharper disable InconsistentNaming
        public int SSU
        // ReSharper restore InconsistentNaming
        {
            get { return _ssu; }
            set
            {
                _ssu = value;
                NotifyPropertyChanged(() => SSU);
            }
        }

        /// <summary>
        /// Gets or sets the SSU variance.
        /// </summary>
        /// <value>The SSU variance.</value>
        [XmlAttributeAttribute]

        // ReSharper disable InconsistentNaming
        public double SSUVariance
        // ReSharper restore InconsistentNaming
        {
            get { return _ssuVariance; }
            set
            {
                _ssuVariance = value;
                NotifyPropertyChanged(() => SSUVariance);
            }
        }

        /// <summary>
        /// Gets or sets the ssu standard deviation.
        /// </summary>
        /// <value>The ssu standard deviation.</value>
        [XmlAttribute("SSUSTDev")]
        public double SsuStandardDeviation
        {
            get { return _ssUStandardDeviation; }
            set
            {
                _ssUStandardDeviation = value;
                NotifyPropertyChanged(() => SsuStandardDeviation);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is checked.
        /// </summary>
        /// <value><c>true</c> if this instance is checked; otherwise, <c>false</c>.</value>
        [XmlAttribute]
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                NotifyPropertyChanged(() => IsChecked);
            }
        }

        /// <summary>
        /// Gets or sets the notes.
        /// </summary>
        /// <value>The notes.</value>
        [XmlAttribute]
        public string Notes
        {
            get { return _notes; }
            set
            {
                if (value != null)
                {
                    var trimmed = value.Trim();
                    if (string.IsNullOrEmpty(trimmed))
                    {
                        _notes = null;
                        NotifyPropertyChanged(() => Notes);
                        return;
                    }
                }

                _notes = value;
                NotifyPropertyChanged(() => Notes);
            }
        }

        // NOTE: I'm not sure how Comments are different from Notes, but Notes is already in use and
        //       I don't want to accidentally co-opt data
        [XmlAttribute]
        public string Comments
        {
            get { return _comments; }
            set
            {
                if (value != null)
                {
                    var trimmed = value.Trim();
                    if (string.IsNullOrEmpty(trimmed))
                    {
                        _comments = null;
                        NotifyPropertyChanged(() => Comments);
                        return;
                    }
                }

                _comments = value;
                NotifyPropertyChanged(() => Comments);
            }
        }

        [XmlAttribute]
        public string Group
        {
            get { return _group; }
            set
            {
                if (string.Equals(_group, value))
                    return;

                _group = value;
                NotifyPropertyChanged(() => Group);
            }
        }

        /// <summary>
        /// Gets or sets the monthly accrual.
        /// </summary>
        /// <value>The monthly accrual.</value>
        [XmlAttribute("PPM")]
        public double MonthlyAccrual
        {
            get { return _monthlyAccrual; }
            set
            {
                if (_monthlyAccrual == value) return;
                _monthlyAccrual = value;
                NotifyPropertyChanged(() => MonthlyAccrual);
            }
        }

        public double MonthlyAccrualExpected
        {
            get { return _monthlyAccrualExpected; }
            set
            {
                if (_monthlyAccrualExpected == value) return;
                _monthlyAccrualExpected = value;
                NotifyPropertyChanged(() => MonthlyAccrualExpected);
            }
        }

        public double? MonthlyAccrualObserved
        {
            get { return _monthlyAccrualObserved; }
            set
            {
                if (_monthlyAccrualObserved == value) return;
                _monthlyAccrualObserved = value;
                NotifyPropertyChanged(() => MonthlyAccrualObserved);
            }
        }

        /// <summary>
        /// Gets or sets the monthly accrual variance.
        /// </summary>
        /// <value>The monthly accrual variance.</value>
        [XmlAttributeAttribute]
        public double MonthlyAccrualVariance
        {
            get { return _monthlyAccrualVariance; }
            set
            {
                if (_monthlyAccrualVariance == value) return;
                _monthlyAccrualVariance = value;
                NotifyPropertyChanged(() => MonthlyAccrualVariance);
            }
        }

        /// <summary>
        /// Gets or sets the monthly accrual standard deviation.
        /// </summary>
        /// <value>The monthly accrual standard deviation.</value>
        [XmlAttribute("PPMSTDev")] //
        public double MonthlyAccrualStandardDeviation
        {
            get { return _monthlyAccrualStandardDeviation; }
            set
            {
                if (_monthlyAccrualStandardDeviation == value) return;
                _monthlyAccrualStandardDeviation = value;
                NotifyPropertyChanged(() => MonthlyAccrualStandardDeviation);
            }
        }

        /// <summary>
        /// Gets or sets the max patients.
        /// </summary>
        /// <value>The max patients.</value>
        [XmlAttribute]
        public int MaxPatients
        {
            get { return _maxPatients; }
            set
            {
                if (_maxPatients == value) return;
                _maxPatients = value;
                NotifyPropertyChanged(() => MaxPatients);
            }
        }

        /// <summary>
        /// Gets or sets the MaxSites.
        /// </summary>
        /// <value>The max sites.</value>
        [XmlAttribute]
        public int MaxSites
        {
            get { return _maxSites; }
            set
            {
                if (_maxSites == value) return;
                _maxSites = value;
                NotifyPropertyChanged(() => MaxSites);
            }
        }

        /// <summary>
        /// Gets or sets the Priority.
        /// </summary>
        /// <value>The priority.</value>
        [XmlAttribute]
        public int Priority
        {
            get { return _priority; }
            set
            {
                if (_priority == value) return;
                _priority = value;
                NotifyPropertyChanged(() => Priority);
            }
        }

        /// <summary>
        /// Gets or sets the average score.
        /// </summary>
        /// <value>The average score.</value>
        [XmlAttribute("CountryAvgScore")]
        public int AverageScore
        {
            get { return _averageScore; }
            set
            {
                _averageScore = value;
                NotifyPropertyChanged(() => AverageScore);
            }
        }

        /// <summary>
        /// Gets or sets the average score.
        /// </summary>
        /// <value>The average score.</value>
        [XmlAttribute]
        public int WeightedTierScoreId
        {
            get { return _weightedTierScoreId; }
            set
            {
                _weightedTierScoreId = value;
                NotifyPropertyChanged(() => WeightedTierScoreId);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CountryEnrollment"/> class.
        /// </summary>
        public CountryEnrollment()
        {
            Sites = new ObservableCollection<InvestigatorEnrollment>();
            InvestigationalEntityLocation = new InvestigationalEntityLocation();
            InvestigationalEntityRegion = new InvestigationalEntityRegion();

            Sites.CollectionChanged -= Sites_CollectionChanged;
            Sites.CollectionChanged += Sites_CollectionChanged;

            EnableChangeMinSsuDayFromPercent = true;
            EnableChangeMinSsuPercentFromDay = true;
            EnableChangeMaxSsuDayFromPercent = true;
            EnableChangeMaxSsuPercentFromDay = true;

        //    Errors = new SerializableDictionary<string, string>();
        }

        private void Sites_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            NotifyPropertyChanged(() => TotalReprojectionsSites);
            NotifyPropertyChanged(() => TotalVirtualSites);
            NotifyPropertyChanged(() => TotalNewActualSites);
            NotifyPropertyChanged(() => TotalActualSites);
            NotifyPropertyChanged(() => PatientsEnrolledToDate);
            NotifyPropertyChanged(() => PatientsScreenedToDate);
            NotifyPropertyChanged(() => PatientsRandomizedToDate);
        }

        #region ICloneable Members

        public object Clone()
        {
            return new CountryEnrollment
            {
                CountryId = CountryId,
                Country = Country,
                RegulatoryLeadTime = RegulatoryLeadTime,
                MaterialLeadTime = MaterialLeadTime,
                SSU = SSU,
                SsuStandardDeviation = SsuStandardDeviation,
                MonthlyAccrual = MonthlyAccrual,
                MonthlyAccrualStandardDeviation = MonthlyAccrualStandardDeviation,
                AverageScore = AverageScore,
                WeightedTierScoreId = WeightedTierScoreId,
                MaxPatients = MaxPatients,
                Sites = new ObservableCollection<InvestigatorEnrollment>(Sites.Select(s => (InvestigatorEnrollment)s.Clone()).ToList())
            };
        }

        #endregion ICloneable Members

        /// <summary>
        /// month of first patient
        /// </summary>
        public double PatientFirstMonth
        {
            get { return _patientFirstMonth; }
            set
            {
                _patientFirstMonth = value;
                NotifyPropertyChanged(() => PatientFirstMonth);
            }
        }

        /// <summary>
        /// month of last patient
        /// </summary>
        public double PatientLastMonth
        {
            get { return _patientLastMonth; }
            set
            {
                _patientLastMonth = value;
                NotifyPropertyChanged(() => PatientLastMonth);
            }
        }

        /// <summary>
        /// first or minimum month that a site is open in the country
        /// </summary>
        public double SiteOpenFirstMonth
        {
            get { return _siteOpenFirstMonth; }
            set
            {
                _siteOpenFirstMonth = value;
                NotifyPropertyChanged(() => SiteOpenFirstMonth);
            }
        }

        /// <summary>
        /// last or maximum month that a site is open in the country
        /// </summary>
        public double SiteOpenLastMonth
        {
            get { return _siteOpenLastMonth; }
            set
            {
                _siteOpenLastMonth = value;
                NotifyPropertyChanged(() => SiteOpenLastMonth);
            }
        }

        #region Reprojections Properties

        public int TotalReprojectionsSites
        {
            get { return Sites.Count(x => x.IsChecked); }
        }

        public int TotalActualSites
        {
            get { return Sites.Count(x => x.SiteStatus != "Virtual"); }
        }

        public int TotalVirtualSites
        {
            get { return Sites.Count(x => x.SiteStatus == "Virtual" && x.IsChecked); }
        }

        private int _totalNewActualSites;

        public int TotalNewActualSites
        {
            get { return _totalNewActualSites; }
            set
            {
                _totalNewActualSites = value;
                NotifyPropertyChanged(() => TotalNewActualSites);
            }
        }

        private decimal _countryScreenFailureRate;

        public decimal CountryScreenFailureRate
        {
            get { return _countryScreenFailureRate; }
            set
            {
                if (value.Equals(_countryScreenFailureRate)) return;
                _countryScreenFailureRate = value;
                NotifyPropertyChanged(() => CountryScreenFailureRate);
            }
        }

        private bool _reprojectionCalcFieldModified;

        public bool ReprojectionCalcFieldModified
        {
            get { return _reprojectionCalcFieldModified; }
            set
            {
                if (value.Equals(_reprojectionCalcFieldModified)) return;
                _reprojectionCalcFieldModified = value;
            }
        }

        public int PatientsScreenedToDate
        {
            get { return Sites.Sum(s => s.SitePtsScreened ?? 0); }
        }

        public int PatientsEnrolledToDate
        {
            get { return Sites.Sum(s => s.SitePtsEnrolled ?? 0); }
        }

        public int PatientsRandomizedToDate
        {
            get { return Sites.Sum(s => s.SitePtsRandomized ?? 0); }
        }

        #endregion Reprojections Properties

        #region ESP_Grid_Columns

        /// <summary>
        /// Key is CountryId from Database not IsoCode , first value of Tuple is Percent and second
        /// value or Tuple is Day.
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public static Dictionary<int, List<Tuple<int, int, string>>> CountrySsuAndPercentLookup;

        public string Region
        {
            get { return InvestigationalEntityRegion.RegionName; }
            set { InvestigationalEntityRegion.RegionName = value; }
        }

        public string RegionId
        {
            get { return InvestigationalEntityRegion.RegionId; }
            set { InvestigationalEntityRegion.RegionId = value; }
        }

        //These are the comments field from the associated Country Model

        public string CountryComment { get; set; }

        //These are comments from the ESP Enrollment Model

        public string EspComment { get; set; }

        //This is the RecommendationID from the associated Country Model

        public int? CountryRecommendationId { get; set; }

        public string CountryRecommendation { get; set; }

        //This is the ReasonID from the associated Country Model

        public int CountryReasonId { get; set; }

        //This is the ReasonID from the ESP Enrollment Model

        public int EspReasonId { get; set; }

        [XmlIgnore, JsonIgnore]
        public bool EspIsCountryRecommended
        {
            get { return SiteCountSelected > 0; }
        }

        public double? CountryScore { get; set; }

        public string CountryTier { get; set; }

        /// <summary>
        /// Count of sites available for selection
        /// </summary>
        public int? SiteCountAvailable
        {
            get { return _siteCountAvailable; }
            set
            {
                _siteCountAvailable = value;
                NotifyPropertyChanged(() => SiteCountAvailable);
            }
        }

        public int SiteCountContributingDisplay
        {
            get { return _siteCountContributingDisplay; }
            set
            {
                _siteCountContributingDisplay = value;
                NotifyPropertyChanged(() => SiteCountContributingDisplay);
            }
        }

        /// <summary>
        /// Gets or sets the site count for sites actually contributing/enrolling patients.
        /// </summary>
        /// <value>The site count.</value>
        public int SiteCountContributing
        {
            get { return _siteCountContributing; }
            set
            {
                _siteCountContributing = value;
                NotifyPropertyChanged(() => SiteCountContributing);
            }
        }

        /// <summary>
        /// Gets or sets the site count for sites actually contributing/enrolling patients.
        /// </summary>
        /// <value>The site count.</value>
        public int LprSiteCountContributing
        {
            get { return _lprSiteCountContributing; }
            set
            {
                _lprSiteCountContributing = value;
                NotifyPropertyChanged(() => LprSiteCountContributing);
            }
        }

        /// <summary>
        /// total number of sites selected
        /// </summary>
        public int SiteCountSelected
        {
            get { return _siteCountSelected; }
            set
            {
                _siteCountSelected = value;
                if (value == 0)
                {
                    UpdateCalculatedFieldsOnSiteCountZero();
                }
                NotifyPropertyChanged(() => SiteCountSelected);
            }
        }

        private void UpdateCalculatedFieldsOnSiteCountZero()
        {
            EarliestFPRCalc = null;
            EarliestStartupPlusScreening = null;
            PatientCountPredicted = 0;
            LprPatientCountPredicted = 0;
            PatientCountPredictedDisplay = 0;
        }

        public string Tier123Sites { get; set; }

        public double? CCTScore { get; set; }

        /// <summary>
        /// Gets or sets Average Enrollment Window. This is really use as FPR Window.
        /// </summary>
        public decimal? EnrollmentWindowInMonths
        {
            get { return _enrollmentWindowInMonths; }
            set
            {
                _enrollmentWindowInMonths = value;
                NotifyPropertyChanged(() => EnrollmentWindowInMonths);
            }
        }

        public decimal? LprEnrollmentWindowInMonths
        {
            get { return _lprEnrollmentWindowInMonths; }
            set
            {
                _lprEnrollmentWindowInMonths = value;
                NotifyPropertyChanged(() => LprEnrollmentWindowInMonths);
            }
        }

        public decimal? EnrollmentWindowInMonthsDisplay
        {
            get { return _enrollmentWindowInMonthsDisplay; }
            set
            {
                _enrollmentWindowInMonthsDisplay = value;
                NotifyPropertyChanged(() => EnrollmentWindowInMonthsDisplay);
            }
        }

        /// <summary>
        /// Gets or sets Average Enrollment Window. this is labeled FULL because it is from
        /// enrollment start not FPR
        /// </summary>
        public decimal? EnrollmentWindowInMonthsFull
        {
            get { return _enrollmentWindowInMonthsFull; }
            set
            {
                _enrollmentWindowInMonthsFull = value;
                NotifyPropertyChanged(() => EnrollmentWindowInMonthsFull);
            }
        }

        /// <summary>
        /// This method is used to Update Min or Max SSU days if Min or Max Percent is changed
        /// respectively and vice versa. CountrySsuAndPercentLookup is populated in
        /// EspCountryListViewModel constructor
        /// </summary>
        /// <param name="whichValue"></param>
        private void UpdateSsuDaysAndPercent(string whichValue)
        {
            if (CountrySsuAndPercentLookup == null) return;

            if (CountrySsuAndPercentLookup.ContainsKey(CountryIdFromDbNotIsoCode))
            {
                switch (whichValue)
                {
                    case "MinSSUPercentage":
                        MinSSUDays = GetSsuDayForGivenPercent(MinSSUPercentage);
                        break;

                    case "MinSSUDays":
                        MinSSUPercentage = GetSsuPercentForGivenDay(MinSSUDays);
                        break;

                    case "MaxSSUPercentage":
                        MaxSSUDays = GetSsuDayForGivenPercent(MaxSSUPercentage);
                        break;

                    case "MaxSSUDays":
                        MaxSSUPercentage = GetSsuPercentForGivenDay(MaxSSUDays);
                        break;
                }
            }
        }

        /// <summary>
        /// Provides Days for whatever Percent, User has entered in Min or Max Percents.
        /// </summary>
        /// <param name="percent"></param>
        /// <returns></returns>
        private int GetSsuDayForGivenPercent(int? percent)
        {
            try
            {
                var countryValues = CountrySsuAndPercentLookup.FirstOrDefault(x => x.Key == CountryIdFromDbNotIsoCode).Value;

                var result = countryValues.FirstOrDefault(x => x.Item1 == percent && x.Item3 == IrbType) ??
                             countryValues.OrderBy(x => x.Item1).FirstOrDefault(x => x.Item1 > percent && x.Item3 == IrbType);

                if (result != null)
                {
                    var resultDay = result.Item2;

                    return resultDay;
                }

                return 0;
            }
            catch (Exception)
            {
                EnableChangeMaxSsuPercentFromDay = true;
                EnableChangeMinSsuPercentFromDay = true;
                return 0;
            }
        }

        /// <summary>
        /// Provides Percent for whatever days, User has entered in Min or Max Days. Max value which
        /// can be returned is 100 because percent cant be more than 100.
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        private int GetSsuPercentForGivenDay(int day)
        {
            try
            {
                var countryValues = CountrySsuAndPercentLookup.FirstOrDefault(x => x.Key == CountryIdFromDbNotIsoCode).Value;

                if (day > countryValues.Where(x => x.Item3 == IrbType).Max(x => x.Item2))
                {
                    return 100;
                }

                var result = countryValues.FirstOrDefault(x => x.Item2 == day && x.Item3 == IrbType) ??
                             countryValues.OrderBy(x => x.Item1).FirstOrDefault(x => x.Item2 > day && x.Item3 == IrbType);

                if (result != null)
                {
                    var resultPercent = result.Item1;

                    return resultPercent;
                }

                return 0;
            }
            catch (Exception)
            {
                EnableChangeMaxSsuDayFromPercent = true;
                EnableChangeMinSsuDayFromPercent = true;
                return 0;
            }
        }

        [XmlIgnore, JsonIgnore]
        //This flag is used to stop cyclic update for MinSsu Days. When user updated MinSsuDays then inside setter of MinSsuDays,
        // a method is called which updates MinSsuPercent. Same method is called on setter of MinSsuPercent, this will cause a cyclic
        //update
        public bool EnableChangeMinSsuDayFromPercent { get; set; }

        [XmlIgnore, JsonIgnore]
        //This flag is used to stop cyclic update for MinSsu Percent. When user updated MinSsuPercent then inside setter of MinSsuPercent,
        // a method is called which updates MinSsuDays. Same method is called on setter of MinSsuDays, this will cause a cyclic
        //update
        public bool EnableChangeMinSsuPercentFromDay { get; set; }

        [XmlIgnore, JsonIgnore]
        //This flag is used to stop cyclic update for MaxSsu Days. When user updated MaxSsuDays then inside setter of MaxSsuDays,
        // a method is called which updates MaxSsuPercent. Same method is called on setter of MaxSsuPercent, this will cause a cyclic
        //update
        public bool EnableChangeMaxSsuDayFromPercent { get; set; }

        [XmlIgnore, JsonIgnore]
        //This flag is used to stop cyclic update for MaxSsu Percent. When user updated MinSsuPercent then inside setter of MaxSsuPercent,
        // a method is called which updates MaxSsuDays. Same method is called on setter of MaxSsuDays, this will cause a cyclic
        //update
        public bool EnableChangeMaxSsuPercentFromDay { get; set; }

        public int? MinSSUPercentage
        {
            get { return _minSsuPercentage; }
            set
            {
                //if CountrySsuAndPercentLookup for particular country has Central and Local Irbtypes then do just simple update.
                if ((CountrySsuAndPercentLookup != null &&
                     CountrySsuAndPercentLookup.FirstOrDefault(x => x.Key == CountryIdFromDbNotIsoCode).Value != null &&
                     CountrySsuAndPercentLookup.FirstOrDefault(x => x.Key == CountryIdFromDbNotIsoCode).Value.Count < 200) || IsIrbChild)
                {
                    //Max value can be 100 only.
                    if (value > 100)
                    {
                        value = 100;
                    }

                    if (_minSsuPercentage == value || !EnableChangeMinSsuPercentFromDay) return;

                    _minSsuPercentage = value;
                    EnableChangeMinSsuPercentFromDay = false;

                    UpdateSsuDaysAndPercent("MinSSUPercentage");
                    EnableChangeMinSsuPercentFromDay = true;

                    //Since MinSSU percent and Min SSu days are interdependent
                    //thus it gives exception if we change one of them from grid and another one is hidden from ShowColumn chooser
                    try
                    {
                        NotifyPropertyChanged(() => MinSSUPercentage);
                    }
                    catch (Exception e)
                    {
                        var a = e;
                    }
                }
                else
                {
                    _minSsuPercentage = value;
                    NotifyPropertyChanged(() => MinSSUPercentage);
                }
            }
        }

        public int MinSSUDays
        {
            get { return _minSsuDays; }
            set
            {
                //if CountrySsuAndPercentLookup for particular country has Central and Local Irbtypes then do just simple update.
                if ((CountrySsuAndPercentLookup != null &&
                     CountrySsuAndPercentLookup.FirstOrDefault(x => x.Key == CountryIdFromDbNotIsoCode).Value != null &&
                     CountrySsuAndPercentLookup.FirstOrDefault(x => x.Key == CountryIdFromDbNotIsoCode).Value.Count < 200) || IsIrbChild)
                {
                    if (_minSsuDays == value || !EnableChangeMinSsuDayFromPercent) return;

                    _minSsuDays = value;
                    EnableChangeMinSsuDayFromPercent = false;

                    UpdateSsuDaysAndPercent("MinSSUDays");
                    EnableChangeMinSsuDayFromPercent = true;

                    //Since MinSSU percent and Min SSu days are interdependent
                    //thus it gives exception if we change one of them from grid and another one is hidden from ShowColumn chooser
                    try
                    {
                        NotifyPropertyChanged(() => MinSSUDays);
                    }
                    catch (Exception e)
                    {
                        var a = e;
                    }
                }
                else
                {
                    _minSsuDays = value;
                    NotifyPropertyChanged(() => MinSSUDays);
                }
            }
        }

        public int? MaxSSUPercentage
        {
            get { return _maxSsuPercentage; }
            set
            {
                //if CountrySsuAndPercentLookup for particular country has Central and Local Irbtypes then do just simple update.
                if ((CountrySsuAndPercentLookup != null &&
                     CountrySsuAndPercentLookup.FirstOrDefault(x => x.Key == CountryIdFromDbNotIsoCode).Value != null &&
                     CountrySsuAndPercentLookup.FirstOrDefault(x => x.Key == CountryIdFromDbNotIsoCode).Value.Count < 200) || IsIrbChild)
                {
                    //Max value can be 100 only.
                    if (value > 100)
                    {
                        value = 100;
                    }

                    if (_maxSsuPercentage == value ||
                        !EnableChangeMaxSsuPercentFromDay) return;

                    _maxSsuPercentage = value;
                    EnableChangeMaxSsuPercentFromDay = false;

                    UpdateSsuDaysAndPercent("MaxSSUPercentage");
                    EnableChangeMaxSsuPercentFromDay = true;

                    //Since MaxSSU percent and Max SSu days are interdependent
                    //thus it gives exception if we change one of them from grid and another one is hidden from ShowColumn chooser
                    try
                    {
                        NotifyPropertyChanged(() => MaxSSUPercentage);
                    }
                    catch (Exception e)
                    {
                        var a = e;
                    }
                }
                else
                {
                    _maxSsuPercentage = value;
                    NotifyPropertyChanged(() => MaxSSUPercentage);
                }
            }
        }

        public int MaxSSUDays
        {
            get { return _maxSsuDays; }
            set
            {
                //if CountrySsuAndPercentLookup for particular country has Central and Local Irbtypes then do just simple update.
                if ((CountrySsuAndPercentLookup != null &&
                     CountrySsuAndPercentLookup.FirstOrDefault(x => x.Key == CountryIdFromDbNotIsoCode).Value != null &&
                     CountrySsuAndPercentLookup.FirstOrDefault(x => x.Key == CountryIdFromDbNotIsoCode).Value.Count < 200) || IsIrbChild)
                {
                    if (_maxSsuDays == value || !EnableChangeMaxSsuDayFromPercent) return;

                    _maxSsuDays = value;
                    EnableChangeMaxSsuDayFromPercent = false;

                    UpdateSsuDaysAndPercent("MaxSSUDays");
                    EnableChangeMaxSsuDayFromPercent = true;

                    //Since MaxSSU percent and Max SSu days are interdependent
                    //thus it gives exception if we change one of them from grid and another one is hidden from ShowColumn chooser
                    try
                    {
                        NotifyPropertyChanged(() => MaxSSUDays);
                    }
                    catch (Exception e)
                    {
                        var a = e;
                    }
                }
                else
                {
                    _maxSsuDays = value;
                    NotifyPropertyChanged(() => MaxSSUDays);
                }
            }
        }

        public DateTime? EnrollmentStopDate
        {
            get
            {
                if (_enrollmentStopDate == null)
                {
                    return null;
                }
                var getDate = ((DateTime)_enrollmentStopDate).Date.UnspecifiedDate();
                return getDate;
            }
            set
            {
                _enrollmentStopDate = value.UnspecifiedDate();
                NotifyPropertyChanged(() => EnrollmentStopDate);
            }
        }

        public double? HistoricalEnrollRate { get; set; }

        public double? HistoricalEnrollStdDev { get; set; }

        public int PatientCountPredictedDisplay
        {
            get { return _patientCountPredictedDisplay; }
            set
            {
                _patientCountPredictedDisplay = value;
                NotifyPropertyChanged(() => PatientCountPredictedDisplay);
            }
        }

        public int PatientCountPredicted
        {
            get { return _patientCountPredicted; }
            set
            {
                _patientCountPredicted = value;
                NotifyPropertyChanged(() => PatientCountPredicted);
            }
        }

        public int LprPatientCountPredicted
        {
            get { return _lprPatientCountPredicted; }
            set
            {
                _lprPatientCountPredicted = value;
                NotifyPropertyChanged(() => LprPatientCountPredicted);
            }
        }

        public DateTime? EarliestFPRConstraint
        {
            get
            {
                if (_earliestFprConstraint == null)
                {
                    return null;
                }
                var getDate = ((DateTime)_earliestFprConstraint).Date.UnspecifiedDate();
                return getDate;
            }
            set
            {
                _earliestFprConstraint = value.UnspecifiedDate();
                NotifyPropertyChanged(() => EarliestFPRConstraint);
            }
        }

        /// <summary>
        /// Earliest First Patient Randomized considering all delays and enrollment rate and params
        /// </summary>
        public DateTime? EarliestFPRCalc
        {
            get
            {
                if (_earliestFPRCalc == null)
                {
                    return null;
                }
                var getDate = ((DateTime)_earliestFPRCalc).Date.UnspecifiedDate();
                return getDate;
            }
            set
            {
                _earliestFPRCalc = value.UnspecifiedDate();
                NotifyPropertyChanged(() => EarliestFPRCalc);
            }
        }

        /// <summary>
        /// esp users call this EarliestPossibleFPR Enrollment rate is not considered
        /// </summary>
        public DateTime? EarliestStartupPlusScreening
        {
            get
            {
                if (_earliestStartupPlusScreening == null)
                {
                    return null;
                }
                var getDate = ((DateTime)_earliestStartupPlusScreening).Date.UnspecifiedDate();
                return getDate;
            }
            set
            {
                _earliestStartupPlusScreening = value.UnspecifiedDate();
                NotifyPropertyChanged(() => EarliestStartupPlusScreening);
            }
        }

        public string TotalStudiesEnrolled { get; set; }

        public string TotalStudies { get; set; }

        public int? CountryScoreId { get; set; }

        public int? PrimaryInvestigatorEnrolledCount { get; set; }

        public int? PrimaryInvestigatorNotEnrolledCount { get; set; }

        public int? PrimaryInvestigatorEnrollmentOpenCount { get; set; }

        public int? PrimaryInvestigatorCount { get; set; }

        public decimal? PatientsEnrollmentRateInMonths { get; set; }

        public string TotalEnrolled { get; set; }

        //CountryId from Database, its not IsoCode.

        public int CountryIdFromDbNotIsoCode { get; set; }

        #endregion ESP_Grid_Columns

        public bool IsIrb { get; set; }

        public string IrbType { get; set; }

        //This property is used to check if changes are made in IrbData pop up or in EspCountryList.

        public bool IsIrbChild { get; set; }

        public ObservableCollection<CountryEnrollment> IrbEnrollments { get; set; }

        [XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Collection<VariableRate> EnrollmentRateAdjustments { get; set; }

        [XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Collection<EnrollmentDelay> EnrollmentDelays { get; set; }

        #region IDataErrorInfo

     //   public SerializableDictionary<string, string> Errors { get; set; }

        //public string this[string propertyName]
        //{
        //    get { return Errors != null && Errors.ContainsKey(propertyName) ? Errors[propertyName] : null; }
        //}

        //public string Error { get { return Errors != null && Errors.Any() ? string.Join(Environment.NewLine, Errors.Values) : null; } }

        #endregion IDataErrorInfo
    }
}