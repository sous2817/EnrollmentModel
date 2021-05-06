using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Semio.ClientService.Data.Events;
using Semio.ClientService.Data.Intelligence.QuickRules;
using Semio.Core.Extensions;

namespace Semio.ClientService.Data.Intelligence.Enrollment
{
    /// <summary>
    /// This class contains enrollment information.
    /// </summary>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class EnrollmentCollection : Bindable
    {
        private Collection<CountryEnrollment> _countries = new Collection<CountryEnrollment>();

        private double _siteAcceptanceVariance;
        private double _monthlyAccrualVariance;
        private CountryEnrollment _selectedCountryEnrollment;
        private ObservableCollection<QuickRuleHistoryItem> _quickRulesHistory;
        private double _desiredPatients;
        private double _maximumMonths = 36; //defaults
        private string _eventName;
        private double _eventRate;
        private double _eventOffset;
        private double _eventTreatmentPeriod;
        private DateTime? _metricSelectionDate;
        private HistoricalPatientRuleEnum _historicalPatientRule = HistoricalPatientRuleEnum.Max; //default
        private VariableRateCollection _variableRateCollection;
        private bool _useYearsForEventTreatmentPeriod;
        private bool _isCtMaterialChecked = true;
        private double _trialScreenFailureRate;
        private int _trialScreeningPeriodInDays;
        private int _trialScreeningPeriodMaxInDays;
        private int _trialSsvPerWeekRate;
        private EnrollmentPriority _enrollmentPriority = EnrollmentPriority.PatientsTarget;
        private int _targetPatients;
        private DateTime? _ssvStartDate;
        private DateTime? _lprTargetDate;
        private List<EnrollmentPriority> _priorities;
        private int _countryCount;
        private double _siteCount;
        private double _patientCount;
        private DateTime? _earliestFprCalculated;
        private DateTime? _earliestStartupPlusScreening;
        private DateTime? _lprDate;
        private decimal _globalEnrollmentRate;

        private double _lprTargetPatientCount;
        private int _lprTargetCountryCount;
        private double _lprTargetSiteCount;
        private DateTime? _lprTargetLprDate;
        private decimal _lprTargetGlobalEnrollmentRate;

        private int? _sitesToTarget;
        private DateTime? _fprTargetDate;
        private double? _dropOutRate;
        private DateTime? _earliestFprConstraint;
        private double? _additionalLagToSiv;
        private int _enrollmentSubjectId;
        private decimal _partialPatientInitialRemainder = 0;
        private EnrollmentCohort _enrollmentCohorts = new EnrollmentCohort();

        public bool InDays { get; set; }

        private int _seedValueForProbabilityModel;

        public int SeedValueForProbabilityModel
        {
            get { return _seedValueForProbabilityModel; }
            set
            {
                _seedValueForProbabilityModel = value;
                NotifyPropertyChanged(() => SeedValueForProbabilityModel);
            }
        }

        public decimal PartialPatientInitialRemainder
        {
            get { return _partialPatientInitialRemainder; }
            set
            {
                _partialPatientInitialRemainder = value;
                NotifyPropertyChanged(() => PartialPatientInitialRemainder);
            }
        }


        public int EnrollmentSubjectId
        {
            get { return _enrollmentSubjectId; }
            set
            {
                _enrollmentSubjectId = value;
                NotifyPropertyChanged(() => EnrollmentSubjectId);
            }
        }


        public bool IsCtMaterialChecked
        {
            get { return _isCtMaterialChecked; }
            set
            {
                _isCtMaterialChecked = value;
                NotifyPropertyChanged(() => IsCtMaterialChecked);
            }
        }

        /// <summary>
        /// Gets or sets the metric selection date.
        /// </summary>
        /// <value>The metric selection date.</value>
        [XmlElement]
        public DateTime? MetricSelectionDate
        {
            get
            {
                if (_metricSelectionDate == null)
                {
                    return null;
                }
                return ((DateTime)_metricSelectionDate).Date.UnspecifiedDate();
            }
            set
            {
                if (value.Equals(_metricSelectionDate)) return;
                _metricSelectionDate = value.UnspecifiedDate();
                NotifyPropertyChanged(() => MetricSelectionDate);
            }
        }

        ///<summary>
        /// If true, use years for event period
        ///</summary>
        [XmlAttribute]
        public bool UseYearsForEventTreatmentPeriod
        {
            get { return _useYearsForEventTreatmentPeriod; }
            set
            {
                if (_useYearsForEventTreatmentPeriod == value)
                {
                    return;
                }
                _useYearsForEventTreatmentPeriod = value;
                //Code commented for QC-252 - starts
                //if (!_useYearsForEventTreatmentPeriod)
                //{
                //    EventTreatmentPeriod = EventTreatmentPeriod*12;
                //}
                //else
                //{
                //    EventTreatmentPeriod = EventTreatmentPeriod/12;
                //}
                //Code commented for QC-252 - ends
                NotifyPropertyChanged(() => UseYearsForEventTreatmentPeriod);
            }
        }

        /// <summary>
        /// historical patient rule
        /// </summary>
        [XmlAttribute]
        public HistoricalPatientRuleEnum HistoricalPatientRule
        {
            get { return _historicalPatientRule; }
            set
            {
                _historicalPatientRule = value;
                NotifyPropertyChanged(() => HistoricalPatientRule);
            }
        }


        public double TrialScreenFailureRate
        {
            get { return _trialScreenFailureRate; }
            set
            {
                if (value.Equals(_trialScreenFailureRate)) return;
                _trialScreenFailureRate = value;
                NotifyPropertyChanged(() => TrialScreenFailureRate);
            }
        }


        public int TrialScreeningPeriodInDays
        {
            get { return _trialScreeningPeriodInDays; }
            set
            {
                if (value == _trialScreeningPeriodInDays) return;
                _trialScreeningPeriodInDays = value;
                NotifyPropertyChanged(() => TrialScreeningPeriodInDays);
            }
        }

        public int TrialScreeningPeriodMaxInDays
        {
            get { return _trialScreeningPeriodMaxInDays; }
            set
            {
                if (value == _trialScreeningPeriodMaxInDays) return;
                _trialScreeningPeriodMaxInDays = value;
                NotifyPropertyChanged(() => TrialScreeningPeriodMaxInDays);
            }
        }


        public int TrialSsvPerWeekRate
        {
            get { return _trialSsvPerWeekRate; }
            set
            {
                if (value == _trialSsvPerWeekRate) return;
                _trialSsvPerWeekRate = value;
                NotifyPropertyChanged(() => TrialSsvPerWeekRate);
            }
        }

        /// <summary>
        /// Event name to factor into a Time-To-Event enrollment scenario.
        /// </summary>
        [XmlAttribute]
        public string EventName
        {
            get { return _eventName; }
            set
            {
                _eventName = value;
                NotifyPropertyChanged(() => EventName);
            }
        }

        /// <summary>
        /// Event rate to factor into a Time-To-Event enrollment scenario.
        /// </summary>
        [XmlAttribute]
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
        /// Event month offset to factor into a Time-To-Event enrollment scenario.
        /// </summary>
        [XmlAttribute]
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
        /// Sets how long of treatment period to collect events for.
        /// </summary>
        [XmlAttribute]
        public double EventTreatmentPeriod
        {
            get { return _eventTreatmentPeriod; }
            set
            {
                _eventTreatmentPeriod = value;
                NotifyPropertyChanged(() => EventTreatmentPeriod);
            }
        }

        ///<summary>
        /// Returns month value of event treatment period
        ///</summary>
        public int EventTreatmentPeriodInMonths
        {
            get
            {
                int multiplier = UseYearsForEventTreatmentPeriod ? 12 : 1;
                return (int)(_eventTreatmentPeriod * multiplier);
            }
        }

        /// <summary>
        /// Holds all of the variable rates for an event.
        /// </summary>
        [XmlElement]
        public VariableRateCollection VariableRateCollection
        {
            get { return _variableRateCollection; }
            set { _variableRateCollection = value; }
        }


        //<summary>
        //Holds all of the variable rates for an event.
        //</summary>
        [XmlElement]
        public EnrollmentCohort EnrollmentCohorts
        {
            get { return _enrollmentCohorts; }
            set
            {
                _enrollmentCohorts = value;
                NotifyPropertyChanged(() => EnrollmentCohorts);
            }
        }

        ///<summary>
        /// this is a param used to set maximum months for calcs
        /// 
        ///</summary>
        public double MaximumMonths
        {
            get { return _maximumMonths; }
            set
            {
                _maximumMonths = value;
                NotifyPropertyChanged(() => MaximumMonths);
            }
        }

        ///<summary>
        ///</summary>
        public double DesiredPatients
        {
            get { return _desiredPatients; }
            set
            {
                _desiredPatients = value;
                NotifyPropertyChanged(() => DesiredPatients);
            }
        }

        /// <summary>
        /// Gets or sets the SSU variance.
        /// </summary>
        /// <value>The SSU variance.</value>
        [XmlAttribute]
        public double SiteAcceptanceVariance
        {
            get { return _siteAcceptanceVariance; }
            set
            {
                _siteAcceptanceVariance = value;
                NotifyPropertyChanged(() => SiteAcceptanceVariance);
            }
        }

        /// <summary>
        /// Gets or sets the monthly accrual variance.
        /// </summary>
        /// <value>The monthly accrual variance.</value>
        [XmlAttribute]
        public double MonthlyAccrualVariance
        {
            get { return _monthlyAccrualVariance; }
            set
            {
                _monthlyAccrualVariance = value;
                NotifyPropertyChanged(() => MonthlyAccrualVariance);
            }
        }

        /// <summary>
        /// Gets or sets the SelectedCountryEnrollment.
        /// </summary>
        /// <value>The start date.</value>
        [XmlIgnore, JsonIgnore]
        public CountryEnrollment SelectedCountryEnrollment
        {
            get { return _selectedCountryEnrollment; }
            set
            {
                _selectedCountryEnrollment = value;
                NotifyPropertyChanged(() => SelectedCountryEnrollment);
            }
        }

        /// <summary>
        /// Gets or sets the QuickRulesHistory.
        /// </summary>
        /// <value>The start date.</value>
        public ObservableCollection<QuickRuleHistoryItem> QuickRulesHistory
        {
            get { return _quickRulesHistory; }
            set
            {
                _quickRulesHistory = value;
                NotifyPropertyChanged(() => QuickRulesHistory);
            }
        }

        /// <summary>
        /// Gets or sets the enrolled countries.
        /// </summary>
        /// <value>The enrolled countries.</value>
        [XmlElement("CountryEnrollment", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Collection<CountryEnrollment> EnrolledCountries
        {
            get { return _countries; }
            set
            {
                //if (_countries != value && _countries != null)
                //{
                //    _countries.ItemPropertyChanged -= CountriesItemPropertyChanged;
                //}
                _countries = value;
                //_countries.ItemPropertyChanged +=CountriesItemPropertyChanged;
                NotifyPropertyChanged(() => EnrolledCountries);

                if (EnrolledCountries.Count > 0)
                    SelectedCountryEnrollment = EnrolledCountries[0];
            }
        }

        #region ESP Parameters

        public EnrollmentPriority EnrollmentPriority
        {
            get { return _enrollmentPriority; }
            set
            {
                if (value == _enrollmentPriority) return;
                _enrollmentPriority = value;
                NotifyPropertyChanged(() => EnrollmentPriority);
            }
        }

        public IEnumerable<EnrollmentPriority> Priorities => _priorities ??
       (_priorities = Enum.GetValues(typeof(EnrollmentPriority)).ToList<EnrollmentPriority>());


        public int TargetPatients
        {
            get { return _targetPatients; }
            set
            {
                if (value == _targetPatients) return;
                _targetPatients = value;
                NotifyPropertyChanged(() => TargetPatients);
            }
        }


        public DateTime SsvStartDate
        {
            get { return _ssvStartDate.GetValueOrDefault(DateTime.Today.UnspecifiedDate()); }
            set
            {
                if (value.Equals(_ssvStartDate)) return;
                _ssvStartDate = value.UnspecifiedDate();
                NotifyPropertyChanged(() => SsvStartDate);
            }
        }


        public DateTime LprTargetDate
        {
            get { return _lprTargetDate.GetValueOrDefault(DateTime.Today.UnspecifiedDate()); }
            set
            {
                if (value.Equals(_lprTargetDate)) return;
                _lprTargetDate = value.UnspecifiedDate();
                NotifyPropertyChanged(() => LprTargetDate);
            }
        }

        #endregion

        #region ESP Optional Parameters

        [XmlElement]
        public int? SitesToTarget
        {
            get { return _sitesToTarget; }
            set
            {
                if (value == _sitesToTarget) return;
                _sitesToTarget = value;
                if (_sitesToTarget == 0)
                    _sitesToTarget = null;
                NotifyPropertyChanged(() => SitesToTarget);
            }
        }

        [XmlElement]
        public DateTime? FprTargetDate
        {
            get
            {
                if (_fprTargetDate == null)
                {
                    return null;
                }
                return ((DateTime)_fprTargetDate).Date.UnspecifiedDate();
            }
            set
            {
                if (value.Equals(_fprTargetDate)) return;
                if (value == default(DateTime))
                    _fprTargetDate = null;
                else
                    _fprTargetDate = value.UnspecifiedDate();
                NotifyPropertyChanged(() => FprTargetDate);
            }
        }

        [XmlElement]
        public double? DropOutRate
        {
            get { return _dropOutRate; }
            set
            {
                if (value.Equals(_dropOutRate)) return;
                _dropOutRate = value;
                if (_dropOutRate == 0)
                    _dropOutRate = null;
                NotifyPropertyChanged(() => DropOutRate);
            }
        }

        [XmlElement]
        public DateTime? EarliestFprConstraint
        {
            get
            {
                if (_earliestFprConstraint == null)
                {
                    return null;
                }
                return ((DateTime)_earliestFprConstraint).Date.UnspecifiedDate();
            }
            set
            {
                if (value.Equals(_earliestFprConstraint)) return;
                if (value == default(DateTime))
                    _earliestFprConstraint = null;
                else
                    _earliestFprConstraint = value.UnspecifiedDate();
                NotifyPropertyChanged(() => EarliestFprConstraint);
            }
        }

        [XmlElement]
        public double? AdditionalLagToSiv
        {
            get { return _additionalLagToSiv; }
            set
            {
                if (value.Equals(_additionalLagToSiv)) return;
                _additionalLagToSiv = value;
                if (_additionalLagToSiv == 0)
                    _additionalLagToSiv = null;
                NotifyPropertyChanged(() => AdditionalLagToSiv);
            }
        }

        #endregion

        #region Calculated Parameters

        public int CountryCount
        {
            get { return _countryCount; }
            set
            {
                if (value == _countryCount) return;
                _countryCount = value;
                NotifyPropertyChanged(() => CountryCount);
            }
        }


        public double SiteCount
        {
            get { return _siteCount; }
            set
            {
                if (Math.Abs(value - _siteCount) < .001) return;
                _siteCount = value;
                NotifyPropertyChanged(() => SiteCount);
            }
        }

        private int _numOfProposedSitesCount;

        public int NumOfProposedSitesCount
        {
            get { return _numOfProposedSitesCount; }
            set
            {
                _numOfProposedSitesCount = value;
                NotifyPropertyChanged(() => NumOfProposedSitesCount);
            }
        }

        private int _numOfProposedCountries;

        public int NumOfProposedCountries
        {
            get { return _numOfProposedCountries; }
            set
            {
                _numOfProposedCountries = value;
                NotifyPropertyChanged(() => NumOfProposedCountries);
            }
        }


        public double PatientCount
        {
            get { return _patientCount; }
            set
            {
                if (Math.Abs(value - _patientCount) < .001) return;
                _patientCount = value;
                NotifyPropertyChanged(() => PatientCount);
            }
        }

        [XmlElement]
        public DateTime? EarliestFprCalculated
        {
            get
            {
                if (_earliestFprCalculated == null)
                {
                    return null;
                }
                return ((DateTime)_earliestFprCalculated).Date.UnspecifiedDate();
            }
            set
            {
                if (value.Equals(_earliestFprCalculated)) return;
                _earliestFprCalculated = value.UnspecifiedDate();
                NotifyPropertyChanged(() => EarliestFprCalculated);
            }
        }

        /// <summary>
        /// esp users call this EarliestPossibleFPR
        /// Enrollment rate is not considered
        /// </summary>
        [XmlElement]
        public DateTime? EarliestStartupPlusScreening
        {
            get
            {
                if (_earliestStartupPlusScreening == null)
                {
                    return null;
                }
                return ((DateTime)_earliestStartupPlusScreening).Date.UnspecifiedDate();
            }
            set
            {
                if (value.Equals(_earliestStartupPlusScreening)) return;
                _earliestStartupPlusScreening = value.UnspecifiedDate();
                NotifyPropertyChanged(() => EarliestStartupPlusScreening);
            }
        }

        [XmlElement]
        public DateTime? LprDate
        {
            get
            {
                if (_lprDate == null)
                {
                    return null;
                }
                return ((DateTime)_lprDate).Date.UnspecifiedDate();
            }
            set
            {
                if (value.Equals(_lprDate)) return;
                _lprDate = value.UnspecifiedDate();
                NotifyPropertyChanged(() => LprDate);
            }
        }


        public decimal GlobalEnrollmentRate
        {
            get { return _globalEnrollmentRate; }
            set
            {
                if (value == _globalEnrollmentRate) return;
                _globalEnrollmentRate = value;
                NotifyPropertyChanged(() => GlobalEnrollmentRate);
            }
        }

        #endregion

        #region LPR Target Calculated Parameters

        public double LprTargetPatientCount
        {
            get { return _lprTargetPatientCount; }
            set
            {
                if (value.Equals(_lprTargetPatientCount)) return;
                _lprTargetPatientCount = value;
                NotifyPropertyChanged(() => LprTargetPatientCount);
            }
        }

        public int LprTargetCountryCount
        {
            get { return _lprTargetCountryCount; }
            set
            {
                if (value == _lprTargetCountryCount) return;
                _lprTargetCountryCount = value;
                NotifyPropertyChanged(() => LprTargetCountryCount);
            }
        }

        public double LprTargetSiteCount
        {
            get { return _lprTargetSiteCount; }
            set
            {
                if (value.Equals(_lprTargetSiteCount)) return;
                _lprTargetSiteCount = value;
                NotifyPropertyChanged(() => LprTargetSiteCount);
            }
        }

        public DateTime? LprTargetLprDate
        {
            get { return _lprTargetLprDate; }
            set
            {
                if (value.Equals(_lprTargetLprDate)) return;
                _lprTargetLprDate = value;
                NotifyPropertyChanged(() => LprTargetLprDate);
            }
        }

        public decimal LprTargetGlobalEnrollmentRate
        {
            get { return _lprTargetGlobalEnrollmentRate; }
            set
            {
                if (value.Equals(_lprTargetGlobalEnrollmentRate)) return;
                _lprTargetGlobalEnrollmentRate = value;
                NotifyPropertyChanged(() => LprTargetGlobalEnrollmentRate);
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="EnrollmentCollection"/> class.
        /// </summary>
        public EnrollmentCollection()
        {
            EnrolledCountries = new Collection<CountryEnrollment>();
            VariableRateCollection = new VariableRateCollection();
        }

        /// <summary>
        /// Handles the ItemPropertyChanged event of the countries control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="ItemPropertyChangedEventArgs"/> instance containing the event data.</param>
        private void CountriesItemPropertyChanged(object sender, ItemPropertyChangedEventArgs args)
        {
            NotifyPropertyChanged(() => EnrolledCountries);
        }

        /// <summary>
        /// count total sites
        /// </summary>
        /// <returns>int</returns>
        public int SiteSelectedCount() => _countries.Sum(x => x.Sites.Count(s => s.IsChecked));

        /// <summary>
        /// count total sites
        /// </summary>
        /// <returns>int</returns>
        public int GetNumberOfSites() => _countries.Sum(x => x.Sites.Count);
    }
    
    public enum EnrollmentPriority
    {
        [Description("LPR Target")]
        LprTarget,
        [Description("Pts Target")]
        PatientsTarget
    }
}
