using DocumentFormat.OpenXml.Spreadsheet;
using Semio.ClientService.OpenXml;
using Semio.ClientService.OpenXml.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using DocumentFormat.OpenXml.Drawing.Charts;

namespace Semio.ClinWeb.Common.Exporters.DataExporters
{
    public abstract class ExcelDataExporterBase
    {
        protected int EmptyLine(string sheetName, IExcelExporter exporter)
        {
            exporter.AddRow(sheetName, new[] { string.Empty });
            return 1;
        }

        protected int AddRow(string sheetName, IExcelExporter exporter, string caption, params string[] textValues)
        {
            var cells = new List<SpreadsheetCell>();

            if (caption != null)
            {
                cells.Add(new SpreadsheetCell(CellValues.String, caption));
            };

            cells.AddRange(textValues.Select(t => new SpreadsheetCell(CellValues.String, t)));

            return AddRow(sheetName, exporter, cells);
        }

        protected int AddRow(string sheetName, IExcelExporter exporter, IEnumerable<SpreadsheetCell> cells)
        {
            exporter.AddRow(sheetName, cells.ToArray());
            return 1;
        }

        ////TODO: [Obsolete("This logic should be in the WORKBOOK exporter, who is responsible for generating new sheets and asking the data exporters to put data into them. Use the method in the Workbook exporter instead")]
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

        //TODO: [Obsolete("This logic should be in the WORKBOOK exporter, who is responsible for generating new sheets and asking the data exporters to put data into them. Use the method in the Workbook exporter instead")]
        protected string GetSheetName(string name, IExcelExporter exporter)
        {
            int attempts = 1;
            string sheetName = name;
            if (name.Length > 30)
            {
                sheetName = name.Substring(0, 30);
            }

            while (attempts < 10 && exporter.HasWorksheet(sheetName))
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

        //TODO: [Obsolete("This logic should be in the WORKBOOK exporter, who is responsible for generating new sheets and asking the data exporters to put data into them. Use the method in the Workbook exporter instead")]
        //protected void AddUniqueSheet(string name, SLDocument exporter)
        //{
        //    AddUniqueSheet(name, null, false, exporter);
        //}

        //TODO: [Obsolete("This logic should be in the WORKBOOK exporter, who is responsible for generating new sheets and asking the data exporters to put data into them. Use the method in the Workbook exporter instead")]
        //protected void AddUniqueSheet(string name, Columns predefinedColumns, bool usesConditionalFormatting, SLDocument exporter)
        //{
        //    if (!exporter.AddWorksheet(name))
        //    {
        //        throw new InvalidOperationException(string.Format("The '{0}' tab has already been added to this spreadsheet.", name));
        //    }

        //    //Delete the existing sheetname Sheet1 from the excel file
        //    exporter.DeleteWorksheet("Sheet1");
        //}

        //TODO: [Obsolete("This logic should be in the WORKBOOK exporter, who is responsible for generating new sheets and asking the data exporters to put data into them. Use the method in the Workbook exporter instead")]
        protected void AddUniqueSheet(string name, IExcelExporter exporter)
        {
            AddUniqueSheet(name, null, false, exporter);
        }

        //TODO: [Obsolete("This logic should be in the WORKBOOK exporter, who is responsible for generating new sheets and asking the data exporters to put data into them. Use the method in the Workbook exporter instead")]
        protected void AddUniqueSheet(string name, Columns predefinedColumns, bool usesConditionalFormatting, IExcelExporter exporter)
        {
            if (!exporter.AddSheet(name, predefinedColumns, usesConditionalFormatting))
            {
                throw new InvalidOperationException(string.Format("The '{0}' tab has already been added to this spreadsheet.", name));
            }
        }

        protected int InsertImage(string sheetName, IExcelExporter exporter, int lineNumber, byte[] imageData)
        {
            if (imageData == null || !imageData.Any())
            {
                return 0;
            }

            int xOffset = 20;
            int yOffset = lineNumber * 20;

            ExportImage siteExportImage = PrepareImage(imageData);
            if (siteExportImage.ImageData != null)
            {
                exporter.AddImage(sheetName, siteExportImage, xOffset, yOffset);
            }

            return (int)Math.Ceiling(siteExportImage.PixelHeight / 20d);
        }

        #region helpers

        // BEGIN untested code (image utility methods fail unless I have a valid image to load)
        protected virtual ExportImage PrepareImage(byte[] imageData)
        {
            using (var stream = new MemoryStream(imageData))
            {
                ImageFormat format;
                System.Windows.Size dimensions = ImageDataInspector.GetDimensions(new BinaryReader(stream), out format);
                return new ExportImage(imageData, format, (int)dimensions.Width, (int)dimensions.Height);
            }
        }

        // END untested code

        protected virtual ConditionalFormattingRule CreateDataBarRule(int minValue, int maxValue, string rgbColor, int priority)
        {
            return WorksheetUtilities.CreateDataBarRule(minValue, maxValue, rgbColor, priority);
        }

        protected virtual ConditionalFormattingRule CreateIconSetRule(int failValue, string targetValueCellRef, int priority)
        {
            return WorksheetUtilities.CreateIconSetRule(failValue, targetValueCellRef, priority);
        }

        protected static SpreadsheetCell CreateCellWithBorder<T>(T text)
        {
            return CreateCellWithStyleId(text, SpreadsheetCell.AllBorderStyle);
        }

        protected static SpreadsheetCell CreateCellWithStyleId<T>(T text, uint? styleId = null, CellValues type = CellValues.String)
        {
            var borderedCell = CreateCell(text, type);
            if (styleId.HasValue)
            {
                borderedCell.StyleId = styleId;
            }
            return borderedCell;
        }

        protected static SpreadsheetCell CreateCell<T>(T text, CellValues type = CellValues.String)
        {
            return new SpreadsheetCell(type, Convert.ToString(text));
        }

        protected static SpreadsheetCell EmptyCell()
        {
            return new SpreadsheetCell(CellValues.String, string.Empty);
        }

        protected static decimal? NullableDecimalRound(decimal? nullableDecimal, int decimals)
        {
            return nullableDecimal.HasValue ? decimal.Round(nullableDecimal.Value, decimals, MidpointRounding.AwayFromZero) : (decimal?)null;
        }

        #endregion helpers

        public static T LookupData<T>(dynamic additionalData, string propertyName)
        {
            if (null == additionalData)
            {
                return default(T);
            }

            var dictionaryOfData = (((IDictionary<string, Object>)additionalData));

            if (!dictionaryOfData.ContainsKey(propertyName))
            {
                return default(T);
            }

            return (T)dictionaryOfData[propertyName];
        }
    }
}