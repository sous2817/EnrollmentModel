using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnrollmentAlgorithm.Objects.Additional
{
    public class ProgressReporter
    {
        public string FriendlyMessage { get; set; }
        public int? IterationCounter { get; set; }
        public int CurrentStep { get; set; }

    }
}
