using System.Collections.ObjectModel;
using System.Linq;

namespace Semio.ClientService.Data.Intelligence.Enrollment
{
    public class EnrollmentCohort : Bindable
    {
        public EnrollmentCohort()
        {
            VariableCohorts = new ObservableCollection<VariableEnrollmentCohorts>();
        }

        private bool _isMultipleEnrollmentCohorts;

        public bool IsMultipleEnrollmentCohorts
        {
            get { return _isMultipleEnrollmentCohorts; }
            set
            {
                _isMultipleEnrollmentCohorts = value;
                NotifyPropertyChanged(() => IsMultipleEnrollmentCohorts);
            }
        }


        private ObservableCollection<VariableEnrollmentCohorts> _variableCohorts;

        public ObservableCollection<VariableEnrollmentCohorts> VariableCohorts
        {
            get { return _variableCohorts; }
            set
            {
                _variableCohorts = value;
                NotifyPropertyChanged(() => VariableCohorts);

                IsMultipleEnrollmentCohorts = _variableCohorts.Any();

            }
        }


        private int _patientCount;

        public int PatientCount
        {
            get { return _patientCount; }
            set
            {
                _patientCount = value;
                NotifyPropertyChanged(() => PatientCount);
            }
        }
       
        private int _numOfCohorts;

        public int NumOfCohorts
        {
            get { return _numOfCohorts; }
            set
            {
                _numOfCohorts = value;
                NotifyPropertyChanged(() => NumOfCohorts);
            }
        }

        private int _cohortDuration;

        public int CohortDuration
        {
            get { return _cohortDuration; }
            set
            {
                _cohortDuration = value;
                NotifyPropertyChanged(() => CohortDuration);
            }
        }
        
        private bool _isCohortEnabled = true;

        public bool IsCohortEnabled
        {
            get { return _isCohortEnabled; }
            set
            {
                _isCohortEnabled =  value; 
                NotifyPropertyChanged(() => IsCohortEnabled);
            }
        }
        

        private bool _useWeeks;

        public bool UseWeeks
        {
            get { return _useWeeks; }
            set
            {
                _useWeeks = value;
                NotifyPropertyChanged(() => UseWeeks);
            }
        }


    }

    public class VariableEnrollmentCohorts:Bindable
    {
        private string _cohortLabel;

        public string CohortLabel
        {
            get { return _cohortLabel; }
            set
            {
                _cohortLabel = value;
                NotifyPropertyChanged(() => CohortLabel);
            }
        }

        private int _cohortpatientCount;

        public int CohortPatientCount
        {
            get { return _cohortpatientCount; }
            set
            {
                _cohortpatientCount = value;
                NotifyPropertyChanged(() => CohortPatientCount);
            }
        }
        
    }
}
