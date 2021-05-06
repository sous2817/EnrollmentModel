using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnrollmentAlgorithm.Objects.Additional;
using EnrollmentAlgorithm.Objects.Enrollment;
using Semio.ClientService.OpenXml.Excel;

namespace EnrollmentAlgorithm.Methods.Export
{
    class SIVDataWorkbookExporter : CoreWorkbookExporter<SummarizedSSUResults>
    {
        public SIVDataWorkbookExporter(IExcelExporter excelExporter, SummaryDataExporter summaryDataExporter) : base(excelExporter, summaryDataExporter)
        {
        }

        protected override IEnumerable<SummarizedSSUResults> GetRawData(BaseEnrollmentObject data)
        {
            return data.SSUSummary;
        }

        protected override double[] GetArrayInfo(SummarizedSSUResults accrualType)
        {
            return accrualType.RawSIVValues;
        }
    }
}
