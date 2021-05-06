using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using EnrollmentAlgorithm.Objects.Enrollment;
using Semio.ClientService.OpenXml.Excel;
using Semio.ClinWeb.Common.Exporters.DataExporters;
using Semio.ClinWeb.Common.Exporters.Excel;
using Semio.Core.IO;

namespace EnrollmentAlgorithm.Methods.Export
{
    public abstract class CoreWorkbookExporter<T> : WorkbookExporterBase<TrialParameter>
    {
        private readonly SummaryDataExporter _summaryDataExporter;

        protected CoreWorkbookExporter(IExcelExporter excelExporter, SummaryDataExporter summaryDataExporter)
            : base(new IOFactory(), excelExporter, new FileHeaderOptionsDataExporter())
        {
            _summaryDataExporter = new SummaryDataExporter();
        }

        protected override void Export(string filename, TrialParameter data)
        {
            CreateTrialSheet(data);
            CreateCountrySheets(data.CountryList);
        }
       

        protected override void Dispose(bool disposing)
        {
        }

        private void CreateTrialSheet(TrialParameter data)
        {
            const string trialSheetName = "TrialData";
            var rawData = GetRawData(data).ToList();

            
            AddUniqueSheet(trialSheetName);
            _summaryDataExporter.SummaryDataGenerator(trialSheetName, ExcelExporter, rawData, x=> GetArrayInfo(x));
        }

        private void CreateCountrySheets(List<CountryParameter> countryData)
        {
            foreach (var country in countryData)
            {
                var rawData = GetRawData(country).ToList();
                AddUniqueSheet(country.Name);
                _summaryDataExporter.SummaryDataGenerator(country.Name, ExcelExporter, rawData, x => GetArrayInfo(x));
            }
        }

        protected abstract IEnumerable<T> GetRawData(BaseEnrollmentObject data);
        protected abstract double[] GetArrayInfo(T accrualType);
    }

}
