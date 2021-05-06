using Semio.ClientService.OpenXml.Excel;
using Semio.ClinWeb.Common.Interfaces;

namespace Semio.ClinWeb.Common.Exporters.DataExporters
{
    public class FileHeaderOptionsDataExporter : ExcelDataExporterBase, IFileHeaderOptionsDataExporter
    {
        public int StandardHeader(string sheetName, IExcelExporter exporter, FileHeaderOptions data)
        {
            var rowCount = 0;
            rowCount += AddRow(sheetName, exporter, "File Name", data.File.Name);
            rowCount += AddRow(sheetName, exporter, "Export Date", data.ExportDateFormatted);
            rowCount += AddRow(sheetName, exporter, "Export Time", data.ExportTimeFormatted);
            rowCount += AddRow(sheetName, exporter, "Exported By", data.UserName);
            return rowCount;
        }
    }
}