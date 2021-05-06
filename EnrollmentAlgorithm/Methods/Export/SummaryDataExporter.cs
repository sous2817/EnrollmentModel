using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Spreadsheet;
using EnrollmentAlgorithm.Objects.Additional;
using EnrollmentAlgorithm.Objects.Enrollment;
using Semio.ClientService.OpenXml.Excel;
using Semio.ClinWeb.Common.Exporters.DataExporters;

namespace EnrollmentAlgorithm.Methods.Export
{
    public class SummaryDataExporter : ExcelDataExporterBase
    {
        public int SummaryDataGenerator<T>(string sheetName, IExcelExporter exporter, List<T> data, Func<T,double[]> dataFunc)
        {
            var maxCounter = data.Count;

            exporter.AddRow(sheetName,BuildHeaderRow(dataFunc(data.First()).Length));

            for (var counter = 0; counter <= maxCounter - 1; counter++)
            {
                var counter1 = counter;
                var cellValues = dataFunc(data[counter1]).Select(item => new SpreadsheetCell(CellValues.Number, item.ToString(CultureInfo.CurrentCulture))).ToList();
                exporter.AddRow(sheetName, new List<SpreadsheetCell> { new SpreadsheetCell(CellValues.Number, (counter1 +1).ToString())}.Union(cellValues));
            }
            return 0;
        }

        private static IEnumerable<string> BuildHeaderRow(int numberOfIterations)
        {
            var headerRow = new string[numberOfIterations +1];

            headerRow[0] = "Day";

            for (var counter = 1; counter <= numberOfIterations; counter++)
            {
                headerRow[counter] = $"Iteration{counter}";
            }
            
            return headerRow;
        }
    }
}
