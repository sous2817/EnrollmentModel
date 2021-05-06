using Semio.ClientService.OpenXml.Excel;
using Semio.ClinWeb.Common.Exporters;

namespace Semio.ClinWeb.Common.Interfaces
{
    public interface IFileHeaderOptionsDataExporter
    {
        int StandardHeader(string sheetName, IExcelExporter exporter, FileHeaderOptions data);
    }
}