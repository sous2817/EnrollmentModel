using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnrollmentAlgorithm.Objects.Additional
{
    public class MeanAndErrorEstimatesDates
    {
        public DateTime MeanDate { get; set; }
        public int StdDevInDays { get; set; }
        public DateTime PlusOneStdDevDate { get; set; }
        public DateTime MinusOneStdDevDate { get; set; }
    }
}
