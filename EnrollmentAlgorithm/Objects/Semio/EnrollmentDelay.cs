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
    public class EnrollmentDelay
    {
        private DateTime? _delayStartDate;
        private DateTime? _delayFinalDate;
        
        public DateTime DelayStartDate
        {
            get { return _delayStartDate.GetValueOrDefault(DateTime.Today.UnspecifiedDate()); }
            set
            {

                _delayStartDate = value.UnspecifiedDate();
               
            }
        }

        public DateTime DelayFinalDate
        {
            get { return _delayFinalDate.GetValueOrDefault(DateTime.Today.UnspecifiedDate());}
            set
            {

                _delayFinalDate = value.UnspecifiedDate();
               
            }
        }

    }
}
