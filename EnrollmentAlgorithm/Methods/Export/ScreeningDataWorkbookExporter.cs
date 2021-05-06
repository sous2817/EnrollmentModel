using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnrollmentAlgorithm.Objects.Additional;
using EnrollmentAlgorithm.Objects.Enrollment;
using Semio.ClientService.OpenXml.Excel;

namespace EnrollmentAlgorithm.Methods.Export
{
    class ScreeningDataWorkbookExporter : CoreWorkbookExporter<SummarizedAccrualResults>
    {
        public ScreeningDataWorkbookExporter(IExcelExporter excelExporter, SummaryDataExporter summaryDataExporter) : base(excelExporter, summaryDataExporter)
        {
        }


        protected override IEnumerable<SummarizedAccrualResults> GetRawData(BaseEnrollmentObject data)
        {
            return data.AccrualSummary;
        }

        protected override double[] GetArrayInfo(SummarizedAccrualResults accrualType)
        {
            return accrualType.RawScreeningValues;
        }

    }
}
