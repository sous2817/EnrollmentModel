using DocumentFormat.OpenXml.Spreadsheet;
using Semio.ClientService.OpenXml.Excel;
using Semio.ClinWeb.Common.Interfaces;
using Semio.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Semio.ClinWeb.Common.Exporters.Excel
{
    public abstract class WorkbookExporterBase<TData> : IWorkbookExporter<TData>
        where TData : class
    {
        private readonly IIOFactory _ioFactory;
        private readonly IFileHeaderOptionsDataExporter _fileHeaderOptionsDataExporter;
        protected readonly IExcelExporter ExcelExporter;

        public string UserName { get; set; }

        protected WorkbookExporterBase(IIOFactory ioFactory, IExcelExporter excelExporter, IFileHeaderOptionsDataExporter fileHeaderOptionsDataExporter)
        {
            _ioFactory = ioFactory;
            ExcelExporter = excelExporter;
            _fileHeaderOptionsDataExporter = fileHeaderOptionsDataExporter;
        }

        public virtual void ExportTo(string filename, TData data)
        {
            if (data == null)
                throw new InvalidOperationException("Unable to export when no data has been configured.");

            var file = _ioFactory.GetFile(filename);
            var directory = file.Directory.FullName;

            if (!file.Directory.Exists)
                _ioFactory.GetDirectory(directory, true);

            if (file.Exists)
                file.Delete();

            ExcelExporter.BeginDocument(filename, 30);

            Export(filename, data);

            ExcelExporter.EndDocument();
        }

        /// <summary>
        ///     Cleanup any resources being used.
        /// </summary>
        public void Dispose()
        {
            if (ExcelExporter != null)
                ExcelExporter.Dispose();

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void EmptyLine(string sheetName)
        {
            EmptyLine(sheetName, 1);
        }

        protected void EmptyLines(string sheetName, int numberOfLines)
        {
            for (var i = 0; i < numberOfLines; i++)
            {
                EmptyLine(sheetName);
            }
        }

        protected void EmptyLine(string sheetName, int columnCount)
        {
            ExcelExporter.AddRow(sheetName, EmptyColumns(columnCount));
        }

        private IEnumerable<string> EmptyColumns(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return string.Empty;
            }
        }

        #region dealing with worksheet creation and naming

        protected void AddFileHeaderInformation(string sheetName, FileHeaderOptions data)
        {
            _fileHeaderOptionsDataExporter.StandardHeader(sheetName, ExcelExporter, data);
            EmptyLine(sheetName);
        }

        protected void AddAdditionalDescriptionHeader(string sheetName, Dictionary<string, string> descriptionData)
        {
            foreach (var val in descriptionData)
            {
                AddRow(sheetName, val.Key, val.Value);
            }
            EmptyLine(sheetName);
        }

        //protected string GetSheetName(string name, SLDocument exporter)
        //{
        //    int attempts = 1;
        //    string sheetName = name;
        //    if (name.Length > 30)
        //    {
        //        sheetName = name.Substring(0, 30);
        //    }

        //    while (attempts < 10 && exporter.SelectWorksheet(sheetName))
        //    {
        //        if (name.Length > 28)
        //        {
        //            sheetName = name.Substring(0, 28) + "_" + attempts;
        //        }
        //        else
        //        {
        //            sheetName = name + "_" + attempts;
        //        }
        //        attempts++;
        //    }

        //    if (attempts == 10)
        //    {
        //        throw new InvalidOperationException(string.Format("The '{0}' tab has already been added to this spreadsheet and a unique name could not be created.", name));
        //    }
        //    return sheetName;
        //}

        protected string GetSheetName(string name)
        {
            int attempts = 1;
            string sheetName = name;
            if (name.Length > 30)
            {
                sheetName = name.Substring(0, 30);
            }

            while (attempts < 10 && ExcelExporter.HasWorksheet(sheetName))
            {
                if (name.Length > 28)
                {
                    sheetName = name.Substring(0, 28) + "_" + attempts;
                }
                else
                {
                    sheetName = name + "_" + attempts;
                }
                attempts++;
            }

            if (attempts == 10)
            {
                throw new InvalidOperationException(string.Format("The '{0}' tab has already been added to this spreadsheet and a unique name could not be created.", name));
            }
            return sheetName;
        }

        //protected void AddUniqueSheet(string name, SLDocument exporter)
        //{
        //    AddUniqueSheet(name, null, false, exporter);
        //}

        //protected void AddUniqueSheet(string name, Columns predefinedColumns, bool usesConditionalFormatting, SLDocument exporter)
        //{
        //    if (!exporter.AddWorksheet(name))
        //    {
        //        throw new InvalidOperationException(string.Format("The '{0}' tab has already been added to this spreadsheet.", name));
        //    }

        //    //Delete the existing sheetname Sheet1 from the excel file
        //    exporter.DeleteWorksheet("Sheet1");
        //}

        protected void AddUniqueSheet(string name)
        {
            AddUniqueSheet(name, null, false);
        }

        protected void AddUniqueSheet(string name, Columns predefinedColumns, bool usesConditionalFormatting)
        {
            if (!ExcelExporter.AddSheet(name, predefinedColumns, usesConditionalFormatting))
            {
                throw new InvalidOperationException(string.Format("The '{0}' tab has already been added to this spreadsheet.", name));
            }
        }

        #endregion dealing with worksheet creation and naming

        protected int AddRow(string sheetName, string caption, params string[] textValues)
        {
            var cells = new List<SpreadsheetCell> { new SpreadsheetCell(CellValues.String, caption) };
            cells.AddRange(textValues.Select(t => new SpreadsheetCell(CellValues.String, t)));

            ExcelExporter.AddRow(sheetName, cells.ToArray());
            return 1;
        }

        protected abstract void Export(string filename, TData data);

        protected abstract void Dispose(bool disposing);
    }
}