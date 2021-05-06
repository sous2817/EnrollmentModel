using System;
using System.Xml.Serialization;
using Semio.Core.Extensions;

namespace Semio.ClientService.Data.Intelligence.Enrollment
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class VariableRate : Bindable
    {
        private int _month;
        private double _rate;
        private DateTime? _rateDate;
        private double _rateVariance;

        /// <summary>
        /// The selected month.
        /// </summary>
        [XmlAttribute]
        public int Month
        {
            get { return _month; }
            set
            {
                _month = value;
                NotifyPropertyChanged(() => Month);
            }
        }

        /// <summary>
        /// The specified dropout rate.
        /// </summary>
        [XmlAttribute]
        public double Rate
        {
            get { return _rate; }
            set
            {
                _rate = value;
                NotifyPropertyChanged(() => Rate);
            }
        }

        /// <summary>
        /// The specified rate variance
        /// </summary>
        [XmlAttribute]
        public double RateVariance
        {
            get { return _rateVariance; }
            set
            {
                _rateVariance = value;
                NotifyPropertyChanged(() => RateVariance);
            }
        }

        /// <summary>
        /// The specified rate Date
        /// </summary>
        [XmlAttribute]
        public DateTime RateDate
        {
            get { return _rateDate.GetValueOrDefault(DateTime.Today.UnspecifiedDate()); }
            set
            {
                if (value.Equals(_rateDate)) return;
                _rateDate = value.UnspecifiedDate();
                NotifyPropertyChanged(() => RateDate);
            }
        }


    }
}
