using System;

namespace Semio.ClientService.Data.Intelligence.QuickRules
{
    /// <summary>
    /// Quick Rules History Item
    /// </summary>
    public class QuickRuleHistoryItem : Bindable
    {
        #region PROPERTY: Display

        private string _display;

        /// <summary>
        /// Gets or sets the Display.
        /// </summary>
        /// <value>The start date.</value>
        public string Display
        {
            get { return _display; }
            set
            {
                _display = value;
                NotifyPropertyChanged(() => Display);
            }
        }

        #endregion

        #region PROPERTY: DateTime

        private DateTime _dateTime;

        /// <summary>
        /// Gets or sets the DateTime.
        /// </summary>
        /// <value>The start date.</value>
        public DateTime DateTime
        {
            get { return _dateTime; }
            set
            {
                _dateTime = value;
                NotifyPropertyChanged(() => DateTime);
            }
        }

        #endregion
    }
}