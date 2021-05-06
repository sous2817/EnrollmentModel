using System.ComponentModel;

namespace Semio.ClientService.Data.Intelligence.Enrollment
{
   ///<summary>
    /// enum for HistoricalPatientRules used in patient modeler
    ///</summary>
    public enum HistoricalPatientRuleEnum
    {

        ///<summary>
        /// Max
        ///</summary>
        [Description("Max")]
        Max,
        ///<summary>
        /// Median
        ///</summary>
        [Description("Median")]
        Median
    }        
}
