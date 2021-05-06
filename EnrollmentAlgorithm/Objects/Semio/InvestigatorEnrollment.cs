using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Semio.ClientService.Data.Intelligence.Enrollment
{
    /// <summary>
    /// Enrollment object
    /// </summary>
    [Serializable]
    [XmlType(AnonymousType = true)]
    public class InvestigatorEnrollment : Bindable, ICloneable
    {
        private int _ssu;
        private string _siteName;
        private string _primaryInvestigatorNameForReprojections;
        private string _siteNameForQGetExport;
        private bool _isAdded;
        private bool _isChecked;
        private double _monthlyAccrual;
        private double _monthlyStandardDeviation;
        private double _ssuStandardDeviation;
        private int _maxPatients;
        private int _medianPatients;
        private string _notes;
        private int _averageScore;
        private int _weightedTierScoreId;
        private string _city;
        private string _state;
        private string _dataSourceOwner;

        private string _actualMetricsMetricName;
        private int _actualMetricsSelectedStatus;
        private int _actualMetricsSourceOnly;
        private bool _isUserModified;
        private string _priorState;

        private bool _isViewModified;
        private double _ssvOffsetInDays;
        private int _sivOffsetInDays;
        private string _siteComments;
        private string _siteStatus;
        private DateTime? _siteSsvDate;
        private DateTime? _siteSivDate;
        private DateTime? _siteEnrollmentClosedDate;
        private decimal? _siteObservedEnrRate;
        private decimal? _siteUpdatedEnrRate;
        private decimal? _siteExpectedEnrRate;
        private decimal? _siteExpectedEnrRateForReprojections;
        private int? _sitePtsScreened;
        private int? _sitePtsEnrolled;
        private int? _sitePtsRandomized;
        private string _siteNameForReprojections;
        private bool _isVirtual;
        private string _siteKey;
        private string _siteNumber;

        public enum SiteStatusEnun
        {
            [Description("Virtual")]
            Virtual,

            [Description("Actual")]
            Actual
        }

        #region Reprojections Properties

        private DateTime? _dateProjectedSiteInitiationForReprojection;

        public DateTime? DateProjectedSiteInitiationForReprojection
        {
            get { return _dateProjectedSiteInitiationForReprojection; }
            set
            {
                _dateProjectedSiteInitiationForReprojection = value;
                NotifyPropertyChanged(() => DateProjectedSiteInitiationForReprojection);
            }
        }

        private DateTime? _dateSiteSelectedForReprojections;

        public DateTime? DateSiteSelectedForReprojections
        {
            get { return _dateSiteSelectedForReprojections; }
            set
            {
                _dateSiteSelectedForReprojections = value;
                NotifyPropertyChanged(() => DateSiteSelectedForReprojections);
            }
        }

        public string SiteNumber
        {
            get { return _siteNumber; }
            set
            {
                _siteNumber = value;
                NotifyPropertyChanged(() => SiteNumber);
            }
        }

        public string SiteKey
        {
            get { return _siteKey; }
            set
            {
                _siteKey = value;
                NotifyPropertyChanged(() => SiteKey);
            }
        }

        private bool _isSsvDateNullFromActual;

        public bool IsSsvDateNullFromActual
        {
            get { return _isSsvDateNullFromActual; }
            set
            {
                _isSsvDateNullFromActual = value;
                NotifyPropertyChanged(() => IsSsvDateNullFromActual);
            }
        }

        private bool _isSivDateNullFromActual;

        public bool IsSivDateNullFromActual
        {
            get { return _isSivDateNullFromActual; }
            set
            {
                _isSivDateNullFromActual = value;
                NotifyPropertyChanged(() => IsSivDateNullFromActual);
            }
        }

        private bool _isEnrollmentClosedNullFromActual;

        public bool IsEnrollmentClosedNullFromActual
        {
            get { return _isEnrollmentClosedNullFromActual; }
            set
            {
                _isEnrollmentClosedNullFromActual = value;
                NotifyPropertyChanged(() => IsEnrollmentClosedNullFromActual);
            }
        }

        public bool IsSiteSsvDateReadOnly
        {
            get { return (!IsSsvDateNullFromActual && SiteStatus != "Virtual"); }
        }

        public bool IsSiteSivDateReadOnly
        {
            get { return (!IsSivDateNullFromActual && SiteStatus != "Virtual"); }
        }

        public bool IsSiteEnrollmentClosedDateReadOnly
        {
            get { return (!IsEnrollmentClosedNullFromActual && SiteStatus != "Virtual"); }
        }

        public bool IsVirtual
        {
            get { return _isVirtual; }
            set
            {
                _isVirtual = value;
                NotifyPropertyChanged(() => IsVirtual);
            }
        }

        private string _countryId;

        public string CountryId
        {
            get { return _countryId; }
            set
            {
                _countryId = value;
                NotifyPropertyChanged(() => CountryId);
            }
        }

        private string _countryName;

        public string CountryName
        {
            get { return _countryName; }
            set
            {
                _countryName = value;
                NotifyPropertyChanged(() => CountryName);
            }
        }

        [XmlElement(IsNullable = true)]
        public string SiteNameForReprojections
        {
            get { return _siteNameForReprojections; }
            set
            {
                _siteNameForReprojections = value;
                NotifyPropertyChanged(() => SiteNameForReprojections);
            }
        }

        [XmlElement(IsNullable = true)]
        public int? SitePtsRandomized
        {
            get { return _sitePtsRandomized; }
            set
            {
                _sitePtsRandomized = value;
                NotifyPropertyChanged(() => SitePtsRandomized);
            }
        }

        [XmlElement(IsNullable = true)]
        public int? SitePtsEnrolled
        {
            get { return _sitePtsEnrolled; }
            set
            {
                _sitePtsEnrolled = value;
                NotifyPropertyChanged(() => SitePtsEnrolled);
            }
        }

        [XmlElement(IsNullable = true)]
        public int? SitePtsScreened
        {
            get { return _sitePtsScreened; }
            set
            {
                _sitePtsScreened = value;
                NotifyPropertyChanged(() => SitePtsScreened);
            }
        }

        [XmlElement(IsNullable = true)]
        public decimal? SiteExpectedEnrRate
        {
            get { return _siteExpectedEnrRate; }
            set
            {
                _siteExpectedEnrRate = value;
                NotifyPropertyChanged(() => SiteExpectedEnrRate);
            }
        }

        [XmlElement(IsNullable = true)]
        public decimal? SiteExpectedEnrRateForReprojections
        {
            get { return _siteExpectedEnrRateForReprojections; }
            set
            {
                _siteExpectedEnrRateForReprojections = value;
                NotifyPropertyChanged(() => SiteExpectedEnrRateForReprojections);
            }
        }

        [XmlElement(IsNullable = true)]
        public decimal? SiteUpdatedEnrRate
        {
            get { return _siteUpdatedEnrRate; }
            set
            {
                _siteUpdatedEnrRate = value;
                NotifyPropertyChanged(() => SiteUpdatedEnrRate);
            }
        }

        [XmlElement(IsNullable = true)]
        public decimal? SiteObservedEnrRate
        {
            get { return _siteObservedEnrRate; }
            set
            {
                _siteObservedEnrRate = value;
                NotifyPropertyChanged(() => SiteObservedEnrRate);
            }
        }

        [XmlElement(IsNullable = true)]
        public DateTime? SiteEnrollmentClosedDate
        {
            get { return _siteEnrollmentClosedDate; }
            set
            {
                if (value == DateTime.MinValue)
                {
                    value = null;
                }
                _siteEnrollmentClosedDate = value;
                NotifyPropertyChanged(() => SiteEnrollmentClosedDate);
            }
        }

        [XmlElement(IsNullable = true)]
        public DateTime? SiteSivDate
        {
            get { return _siteSivDate; }
            set
            {
                if (value == DateTime.MinValue)
                {
                    value = null;
                }
                _siteSivDate = value;
                NotifyPropertyChanged(() => SiteSivDate);
            }
        }

        [XmlElement(IsNullable = true)]
        public DateTime? SiteSsvDate
        {
            get { return _siteSsvDate; }
            set
            {
                if (value == DateTime.MinValue)
                {
                    value = null;
                }
                _siteSsvDate = value;
                NotifyPropertyChanged(() => SiteSsvDate);
            }
        }

        [XmlElement(IsNullable = true)]
        public string SiteStatus
        {
            get { return _siteStatus; }
            set
            {
                _siteStatus = value;
                NotifyPropertyChanged(() => SiteStatus);
            }
        }

        [XmlElement(IsNullable = true)]
        public string SiteComments
        {
            get { return _siteComments; }
            set
            {
                _siteComments = value;
                NotifyPropertyChanged(() => SiteComments);
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

        #endregion Reprojections Properties

        /// <summary>
        /// it is used to show IsModified User icon as once when some value is changed in
        /// Investigatorselection Grid. User dont have to wait for saving the changes and then see
        /// the icon.
        /// </summary>
        [XmlAttribute]
        public bool IsViewModified
        {
            get { return _isViewModified; }
            set
            {
                if (value.Equals(_isViewModified)) return;
                _isViewModified = value;
                NotifyPropertyChanged(() => IsViewModified);
            }
        }

        /// <summary>
        /// Used to capture the original state of an investigator enrollment as it appeard from the
        /// feed in the case that the user modified the original values.
        /// </summary>
        [XmlAttribute]
        public string PriorState
        {
            get { return _priorState; }
            set
            {
                if (value == _priorState) return;
                _priorState = value;
                NotifyPropertyChanged(() => PriorState);
            }
        }

        /// <summary>
        /// Flags if the country enrollment was modified by the user. This is in contrast to an
        /// enrollment that is a default from the enrollment feed.
        /// </summary>
        [XmlAttribute]
        public bool IsUserModified
        {
            get { return _isUserModified; }
            set
            {
                if (value.Equals(_isUserModified)) return;
                _isUserModified = value;
                NotifyPropertyChanged(() => IsUserModified);
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("{0} :: SSU={1}  MonthlyAccural={2}  MaxPatients={3}  IsAdded={4}  IsChecked{5}  AverageScore={6}",
                                 _siteName,
                                 SSU,
                                 MonthlyAccrual,
                                 MaxPatients,
                                 _isAdded,
                                 _isChecked,
                                 AverageScore);
        }

        /// <summary>
        /// Gets or sets the actual name of the metrics metric.
        /// </summary>
        /// <value>The actual name of the metrics metric.</value>
        [XmlAttribute]
        public string ActualMetricsMetricName
        {
            get { return _actualMetricsMetricName; }
            set { _actualMetricsMetricName = value; }
        }

        /// <summary>
        /// Gets or sets the actual metrics selected status.
        /// </summary>
        /// <value>The actual metrics selected status.</value>
        [XmlAttribute]
        public int ActualMetricsSelectedStatus
        {
            get { return _actualMetricsSelectedStatus; }
            set { _actualMetricsSelectedStatus = value; }
        }

        /// <summary>
        /// Gets or sets the actual metrics source only.
        /// </summary>
        /// <value>The actual metrics source only.</value>
        [XmlAttribute]
        public int ActualMetricsSourceOnly
        {
            get { return _actualMetricsSourceOnly; }
            set { _actualMetricsSourceOnly = value; }
        }

        public double SsvOffsetInDays
        {
            get { return _ssvOffsetInDays; }
            set { _ssvOffsetInDays = value; }
        }

        public int SivOffsetInDays
        {
            get { return _sivOffsetInDays; }
            set { _sivOffsetInDays = value; }
        }

        /// <summary>
        /// Gets or sets the SSU.
        /// </summary>
        /// <value>The SSU.</value>
        [XmlAttribute]
        public int SSU
        {
            get { return _ssu; }
            set
            {
                if (value == _ssu) return;
                _ssu = value;
                NotifyPropertyChanged(() => SSU);
                NotifyPropertyChanged(() => DaysToFirstPatient);
            }
        }

        /// <summary>
        /// Gets or sets the name of the site.
        /// </summary>
        /// <value>The name of the site.</value>
        [XmlAttribute]
        public string SiteName
        {
            get { return _siteName; }
            set
            {
                if (value == _siteName) return;
                _siteName = value;
                NotifyPropertyChanged(() => SiteName);
            }
        }

        public string PrimaryInvestigatorNameForReprojections
        {
            get { return _primaryInvestigatorNameForReprojections; }
            set
            {
                if (value == _primaryInvestigatorNameForReprojections) return;
                _primaryInvestigatorNameForReprojections = value;
                NotifyPropertyChanged(() => PrimaryInvestigatorNameForReprojections);
            }
        }

        #region QGet Export Properties

        /// <summary>
        /// Gets and sets Site Name specific for QGet export.
        /// </summary>
        [XmlAttribute]
        public string SiteNameForQGetExport
        {
            get { return _siteNameForQGetExport; }
            set
            {
                if (value == _siteNameForQGetExport) return;
                _siteNameForQGetExport = value;
                NotifyPropertyChanged(() => SiteNameForQGetExport);
            }
        }

        public double SsvDaysCountForQGetExport
        {
            get
            {
                var adjusted = SsvOffsetInDays + Math.Round(AdditionalSsvLagInWeeksForQGetExport * 7, MidpointRounding.AwayFromZero);
                return adjusted;
            }
        }

        public double SivDaysCountForQGetExport
        {
            get
            {
                var adjusted = SSU + SsvOffsetInDays + Math.Round(AdditionalSsvLagInWeeksForQGetExport * 7, MidpointRounding.AwayFromZero);
                return adjusted;
            }
        }

        public double AdditionalSsvLagInWeeksForQGetExport { get; set; }

        #endregion QGet Export Properties

        /// <summary>
        /// Specific location of investigator (city &amp; state)
        /// </summary>
        /// <example>HOUSTON, TX Halifax TAIPEI Pulawy O'Fallon, IL Modesto, CA</example>
        [XmlIgnore, JsonIgnore]
        public string Location
        {
            get
            {
                var loc = City;
                if (!string.IsNullOrEmpty(State))
                {
                    loc += ", " + State;
                }
                return loc;
            }
        }

        /// <summary>
        /// Gets or sets the city of the site.
        /// </summary>
        /// <value>The city of the site.</value>
        [XmlAttribute]
        public string City
        {
            get { return _city; }
            set
            {
                if (value == _city) return;
                _city = value;
                NotifyPropertyChanged(() => City);
                NotifyPropertyChanged(() => Location);
            }
        }

        /// <summary>
        /// Gets or sets the state of the site.
        /// </summary>
        /// <value>The state of the site.</value>
        [XmlAttribute]
        public string State
        {
            get { return _state; }
            set
            {
                if (value == _state) return;
                _state = value;
                NotifyPropertyChanged(() => State);
                NotifyPropertyChanged(() => Location);
            }
        }

        /// <summary>
        /// Gets or sets the data source owner of the site.
        /// </summary>
        /// <value>The data source owner of the site.</value>
        [XmlAttribute]
        public string DataSourceOwner
        {
            get { return _dataSourceOwner; }
            set
            {
                if (value == _dataSourceOwner) return;
                _dataSourceOwner = value;
                NotifyPropertyChanged(() => DataSourceOwner);
            }
        }

        /// <summary>
        /// Gets or sets if this investigator was added by a user or if it was retrieved from the
        /// data store.
        /// </summary>
        /// <value><c>true</c> if this instance is added; otherwise, <c>false</c>.</value>
        [XmlAttribute("IsAdded")]
        public bool IsAdded
        {
            get { return _isAdded; }
            set
            {
                if (value.Equals(_isAdded)) return;
                _isAdded = value;
                NotifyPropertyChanged(() => IsAdded);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is checked.
        /// </summary>
        /// <value><c>true</c> if this instance is checked; otherwise, <c>false</c>.</value>
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (value.Equals(_isChecked)) return;
                _isChecked = value;
                NotifyPropertyChanged(() => IsChecked);
            }
        }

        /// <summary>
        /// Gets or sets the montly accrual.
        /// </summary>
        /// <value>The montly accrual.</value>
        [XmlAttribute("MontlyAccrual")]
        public double MonthlyAccrual
        {
            get { return _monthlyAccrual; }
            set
            {
                if (value.Equals(_monthlyAccrual)) return;
                _monthlyAccrual = value;
                NotifyPropertyChanged(() => MonthlyAccrual);
                NotifyPropertyChanged(() => DaysToFirstPatient);
            }
        }

        /// <summary>
        /// Gets or sets the standard deviation for month.
        /// </summary>
        /// <value>The standard deviation.</value>
        [XmlAttribute(AttributeName = "StandardDeviation")]
        public double MonthlyStandardDeviation
        {
            get { return _monthlyStandardDeviation; }
            set
            {
                if (value.Equals(_monthlyStandardDeviation)) return;
                _monthlyStandardDeviation = value;
                NotifyPropertyChanged(() => MonthlyStandardDeviation);
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
                if (value == _maxPatients) return;
                _maxPatients = value;
                NotifyPropertyChanged(() => MaxPatients);
            }
        }

        /// <summary>
        /// Gets or sets the Median patients.
        /// </summary>
        /// <value>The Median patients.</value>
        [XmlAttribute]
        public int MedianPatients
        {
            get { return _medianPatients; }
            set
            {
                if (value == _medianPatients) return;
                _medianPatients = value;
                NotifyPropertyChanged(() => MedianPatients);
            }
        }

        /// <summary>
        /// Gets or sets the notes.
        /// </summary>
        /// <value>The notes.</value>
        [XmlElement]
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

        /// <summary>
        /// Gets or sets the standard deviation for SSU.
        /// </summary>
        /// <value>The standard deviation.</value>
        [XmlAttribute("SSUSTDev")]
        public double SSUStandardDeviation
        {
            get { return _ssuStandardDeviation; }
            set
            {
                if (value.Equals(_ssuStandardDeviation)) return;
                _ssuStandardDeviation = value;
                NotifyPropertyChanged(() => SSUStandardDeviation);
            }
        }

        /// <summary>
        /// </summary>
        [XmlAttribute("InvestigatorAvgScore")]
        public int AverageScore
        {
            get { return _averageScore; }
            set
            {
                if (value == _averageScore) return;
                _averageScore = value;
                NotifyPropertyChanged(() => AverageScore);
            }
        }

        [XmlAttribute]
        public int WeightedTierScoreId
        {
            get { return _weightedTierScoreId; }
            set
            {
                if (value == _weightedTierScoreId) return;
                _weightedTierScoreId = value;
                NotifyPropertyChanged(() => WeightedTierScoreId);
            }
        }

        /// <summary>
        /// base calculation for days to first patient
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public double DaysToFirstPatient
        {
            get
            {
                if (MonthlyAccrual > 0)
                {
                    return SSU + ((1 / MonthlyAccrual) * 30.4368);
                }
                return 0;
            }
        }

        #region ICloneable Members

        public object Clone()
        {
            return new InvestigatorEnrollment
            {
                SiteName = SiteName,
                MonthlyAccrual = MonthlyAccrual,
                MonthlyStandardDeviation = MonthlyStandardDeviation,
                SSU = SSU,
                SSUStandardDeviation = SSUStandardDeviation,
                MaxPatients = MaxPatients,
                MedianPatients = MedianPatients,
                AverageScore = AverageScore,
                WeightedTierScoreId = WeightedTierScoreId,
                City = City,
                State = State,
                DataSourceOwner = DataSourceOwner,
                ActualMetricsMetricName = ActualMetricsMetricName,
                ActualMetricsSelectedStatus = ActualMetricsSelectedStatus,
                ActualMetricsSourceOnly = ActualMetricsSourceOnly
            };
        }

        #endregion ICloneable Members
    }
}