using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.VariantTypes;
using Semio.ClientService.Data.Intelligence;
using Semio.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BlipFill = DocumentFormat.OpenXml.Drawing.Spreadsheet.BlipFill;
using BottomBorder = DocumentFormat.OpenXml.Spreadsheet.BottomBorder;
using Chart = DocumentFormat.OpenXml.Drawing.Charts.Chart;
using Fill = DocumentFormat.OpenXml.Spreadsheet.Fill;
using Fonts = DocumentFormat.OpenXml.Spreadsheet.Fonts;
using FontScheme = DocumentFormat.OpenXml.Spreadsheet.FontScheme;
using Formula = DocumentFormat.OpenXml.Drawing.Charts.Formula;
using LeftBorder = DocumentFormat.OpenXml.Spreadsheet.LeftBorder;
using NonVisualDrawingProperties = DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualDrawingProperties;
using NonVisualPictureDrawingProperties = DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualPictureDrawingProperties;
using NonVisualPictureProperties = DocumentFormat.OpenXml.Drawing.Spreadsheet.NonVisualPictureProperties;
using NumberingFormat = DocumentFormat.OpenXml.Drawing.Charts.NumberingFormat;
using OrientationValues = DocumentFormat.OpenXml.Drawing.Charts.OrientationValues;
using PageMargins = DocumentFormat.OpenXml.Spreadsheet.PageMargins;
using PatternFill = DocumentFormat.OpenXml.Spreadsheet.PatternFill;
using Picture = DocumentFormat.OpenXml.Drawing.Spreadsheet.Picture;
using Position = DocumentFormat.OpenXml.Drawing.Spreadsheet.Position;
using RightBorder = DocumentFormat.OpenXml.Spreadsheet.RightBorder;
using ShapeProperties = DocumentFormat.OpenXml.Drawing.Spreadsheet.ShapeProperties;

using SpreadsheetColor = DocumentFormat.OpenXml.Spreadsheet;

using TopBorder = DocumentFormat.OpenXml.Spreadsheet.TopBorder;
using Values = DocumentFormat.OpenXml.Drawing.Charts.Values;

namespace Semio.ClientService.OpenXml.Excel
{
    /// <summary>
    ///     Wrapper class to simplify exporting xlsx documents
    /// </summary>
    public class ExcelExporter : IExcelExporter
    {
        private const double DefaultRowHeight = 15D;
        private const double PixelToHeightConversion = 1.3333333333;
        protected readonly Dictionary<string, bool?> _dataRowMode = new Dictionary<string, bool?>();

        protected readonly Dictionary<string, SpreadsheetGrid> _gridData = new Dictionary<string, SpreadsheetGrid>();
        private readonly Dictionary<string, int> _imageIds = new Dictionary<string, int>();
        protected readonly Dictionary<string, SheetData> _worksheetData = new Dictionary<string, SheetData>();
        private readonly Dictionary<string, DrawingsPart> _worksheetDrawings = new Dictionary<string, DrawingsPart>();
        private readonly Dictionary<string, WorksheetPart> _worksheetParts = new Dictionary<string, WorksheetPart>();

        protected double _defaultColumnWidth;
        protected SpreadsheetDocument _document;
        protected bool _firstSheet = true;
        protected uint _worksheetCounter;

        /// <summary>
        /// Provides a scope for collecting grid data.
        /// </summary>
        /// <param name="sheetName">Name of the sheet to collect data for.</param>
        /// <returns></returns>
        public IDisposable DataCollectionScope(string sheetName)
        {
            return new CollectionScope(this, sheetName);
        }

        /// <summary>
        /// Begins the document and initiates writing it to the specified path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public void BeginDocument(string filePath)
        {
            BeginDocument(filePath, 15);
        }

        /// <summary>
        /// Begins the document and initiates writing it to the specified path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="defaultWidth">The default column width.</param>
        public void BeginDocument(string filePath, double defaultWidth)
        {
            _defaultColumnWidth = defaultWidth;

            _document = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook);

            var extendedFilePropertiesPart = _document.AddNewPart<ExtendedFilePropertiesPart>("rId3");
            GenerateExtendedFileProperties(extendedFilePropertiesPart);

            WorkbookPart workbookPart = _document.AddWorkbookPart();
            GenerateStyles(workbookPart);
            GenerateWorkbook(workbookPart);
        }

        /// <summary>
        /// Ends the document and completes saving it to disk.
        /// </summary>
        public void EndDocument()
        {
            if (_document != null)
            {
                // Note: BEGIN untested code
                // Add standard margins
                foreach (WorksheetPart worksheetPart in _worksheetParts.Values)
                {
                    Worksheet worksheet = worksheetPart.Worksheet;

                    if (!worksheet.Any(elem => elem is ConditionalFormatting)
                        && !worksheet.Any(elem => elem is PageMargins))
                    {
                        worksheet.Append(new PageMargins { Left = 0.7D, Right = 0.7D, Top = 0.75D, Bottom = 0.75D, Header = 0.3D, Footer = 0.3D });
                    }
                }

                // Note: END untested code
                SetPackageProperties(_document);

                _document.Dispose();
                _document = null;
            }

            _worksheetData.Clear();
            _gridData.Clear();
            _dataRowMode.Clear();
            _worksheetCounter = 0;
            _firstSheet = true;
        }

        /// <summary>
        /// Begins the grid data collection.
        /// </summary>
        /// <param name="sheetName">Name of the sheet to collect data for.</param>
        public void BeginGridDataCollection(string sheetName)
        {
            if (_gridData.ContainsKey(sheetName))
            {
                return;
            }

            _dataRowMode[sheetName] = false;
            _gridData.Add(sheetName, new SpreadsheetGrid());
        }

        /// <summary>
        /// Adds the cell to the grid data.
        /// </summary>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <param name="cell">The cell to add.</param>
        public void AddCell(string sheetName, SpreadsheetCell cell)
        {
            if (_dataRowMode[sheetName] == null)
            {
                throw new InvalidOperationException("BeginGridDataCollection must be called to initialize the data before adding cells.");
            }
            if (_dataRowMode[sheetName] == true)
            {
                throw new InvalidOperationException("Rows have already been manually added to this sheet which will interfere with cell data.");
            }

            _gridData[sheetName].AddCell(cell);
        }

        /// <summary>
        /// Ends the grid data collection.
        /// </summary>
        /// <param name="sheetName">Name of the sheet to collect data for.</param>
        public void EndGridDataCollection(string sheetName)
        {
            if (_dataRowMode[sheetName] == null)
            {
                throw new InvalidOperationException("BeginGridDataCollection must be called to initialize the data before ending the data collection.");
            }
            if (_dataRowMode[sheetName] == true)
            {
                throw new InvalidOperationException("Rows have already been manually added to this sheet which will interfere with cell data.");
            }

            SpreadsheetGrid gridData = _gridData[sheetName];
            foreach (var dataRow in gridData)
            {
                AddRow(sheetName, dataRow);
            }

            _gridData.Remove(sheetName);
        }

        /// <summary>
        /// Adds a new sheet to the workbook.
        /// </summary>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <returns></returns>
        public bool AddSheet(string sheetName)
        {
            return AddSheet(sheetName, null);
        }

        /// <summary>
        /// Adds a new sheet to the workbook.
        /// </summary>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <param name="columns">The columns.</param>
        /// <returns></returns>
        public bool AddSheet(string sheetName, Columns columns)
        {
            return AddSheet(sheetName, columns, false);
        }

        /// <summary>
        /// Adds a new sheet to the workbook.
        /// </summary>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="usesConditionalFormatting">if set to <c>true</c> uses conditional formatting.</param>
        /// <returns></returns>
        public bool AddSheet(string sheetName, Columns columns, bool usesConditionalFormatting)
        {
            if (_worksheetData.ContainsKey(sheetName))
            {
                return false;
            }

            string refId = String.Format("rId{0}", _worksheetCounter++);

            VTVector vtVector = _document.ExtendedFilePropertiesPart.Properties.TitlesOfParts.VTVector;
            vtVector.Size++;
            vtVector.Append(new VTLPSTR { Text = sheetName });
            _document.ExtendedFilePropertiesPart.Properties.HeadingPairs = GetHeadingPairs();

            _document.WorkbookPart.Workbook.Sheets.Append(new Sheet { Name = sheetName, SheetId = (UInt32Value)_worksheetCounter, Id = refId });

            var newPart = _document.WorkbookPart.AddNewPart<WorksheetPart>(refId);

            SheetData data = GenerateWorksheetPartContent(newPart, columns, usesConditionalFormatting);

            _worksheetParts.Add(sheetName, newPart);
            _worksheetData.Add(sheetName, data);
            _dataRowMode.Add(sheetName, null);

            _firstSheet = false;
            return true;
        }

        /// <summary>
        /// Add a ConditionalFormattingRule to the worksheet
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="cfRule"></param>
        /// <param name="start"></param>
        public void AddConditionalFormatting(string sheetName, ConditionalFormattingRule cfRule, SpreadsheetCell start)
        {
            AddConditionalFormatting(sheetName, cfRule, start, null);
        }

        /// <summary>
        /// Add a ConditionalFormattingRule to the worksheet
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="cfRule"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public void AddConditionalFormatting(string sheetName, ConditionalFormattingRule cfRule, SpreadsheetCell start, SpreadsheetCell end)
        {
            WorksheetPart worksheetPart = _worksheetParts[sheetName];
            Worksheet worksheet = worksheetPart.Worksheet;
            var conditionalFormatting = new ConditionalFormatting(cfRule);

            if (start != null)
            {
                string sqref = start.CellReference;
                if (end != null)
                {
                    sqref += ":" + end.CellReference;
                }
                conditionalFormatting.SequenceOfReferences = new ListValue<StringValue> { InnerText = sqref };
            }

            worksheet.Append(conditionalFormatting);
        }

        /// <summary>
        /// Adds the image to the specified sheet.
        /// </summary>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <param name="image">The image.</param>
        /// <param name="xOffset">The x offset.</param>
        /// <param name="yOffset">The y offset.</param>
        public void AddImage(string sheetName, ExportImage image, int xOffset, int yOffset)
        {
            WorksheetPart part = _worksheetParts[sheetName];
            if (!_imageIds.ContainsKey(sheetName))
            {
                _imageIds.Add(sheetName, 0);
            }

            _imageIds[sheetName]++;
            int id = _imageIds[sheetName];

            string rId = String.Format("rId{0}", id);
            if (!_worksheetDrawings.ContainsKey(sheetName))
            {
                var drawingsPart = part.AddNewPart<DrawingsPart>(rId);
                drawingsPart.WorksheetDrawing = new WorksheetDrawing();
                _worksheetDrawings.Add(sheetName, drawingsPart);

                part.Worksheet.Append(new Drawing { Id = rId });
            }

            DrawingsPart worksheetDrawing = _worksheetDrawings[sheetName];
            GenerateDrawingContent(worksheetDrawing.WorksheetDrawing, image.PixelWidth, image.PixelHeight, xOffset, yOffset, rId);

            string mimeType = image.Format.GetMimeType();
            if (String.IsNullOrEmpty(mimeType))
            {
                return;
            }

            var imagePart = worksheetDrawing.AddNewPart<ImagePart>(mimeType, rId);
            using (var stream = new MemoryStream(image.ImageData))
            {
                imagePart.FeedData(stream);
            }
        }

        public int AddChartAndData(string sheetName, IEnumerable<IEnumerable<SpreadsheetCell>> allDataWithHeader, string chartTitle)
        {
            return AddChartAndData(sheetName, allDataWithHeader, chartTitle, 0);
        }

        public int AddChartAndData(string sheetName, IEnumerable<IEnumerable<SpreadsheetCell>> allDataWithHeader, string chartTitle, int startingRow)
        {
            return AddChartAndData(sheetName, allDataWithHeader, chartTitle, startingRow, allDataWithHeader.First().Count(), string.Empty, string.Empty);
        }

        public int AddChartAndData(string sheetName, IEnumerable<IEnumerable<SpreadsheetCell>> allDataWithHeader, string chartTitle, int startingRow, int chartWidthInColumns, string xAxisLabel, string yAxisLabel)
        {
            int rowCount = 0;

            int chartHeightInRows = 20;
            int chartWidth = chartWidthInColumns;

            var pixelsToSkip = (int)Math.Ceiling(DefaultRowHeight * PixelToHeightConversion * chartHeightInRows);
            SkipRows(sheetName, pixelsToSkip);
            rowCount += chartHeightInRows;

            // Add data to worksheet
            foreach (var row in allDataWithHeader)
            {
                AddRow(sheetName, row);
                rowCount++;
            }

            // Add line chart to worksheet
            DrawingsPart drawingsPart = GetOrCreateChartPart(sheetName, chartTitle);
            var chartPart = drawingsPart.AddNewPart<ChartPart>();
            chartPart.ChartSpace = new ChartSpace();

            AddChartContent(sheetName, chartPart, allDataWithHeader, xAxisLabel, yAxisLabel);

            TwoCellAnchor twoCellAnchor = ExcelChartHelper.GenerateWorksheetDrawingContents(drawingsPart, chartPart, chartTitle, startingRow, 0, chartHeightInRows, chartWidth);
            drawingsPart.WorksheetDrawing.Append(twoCellAnchor);
            drawingsPart.WorksheetDrawing.Save();

            return rowCount;
        }

        public int AddTierScatterPlotChart(string sheetName, IEnumerable<SpreadsheetCell> facilityCells, IEnumerable<SpreadsheetCell> performanceCells, IEnumerable<IEnumerable<SpreadsheetCell>> tierCells, string chartTitle, int chartStartingRow, int chartHeightInRows, int chartWidthInColumns, int dataStartingRow, int dataEndingRow, int facilityDataColumn, int performanceDataColumn)
        {
            int rowCount = 0;

            var pixelsToSkip = (int)Math.Ceiling(DefaultRowHeight * PixelToHeightConversion * chartHeightInRows);
            SkipRows(sheetName, pixelsToSkip);
            rowCount += chartHeightInRows;

            // Add scatter plot chart to worksheet
            DrawingsPart drawingsPart = GetOrCreateChartPart(sheetName, chartTitle);
            ChartPart chartPart = drawingsPart.AddNewPart<ChartPart>();
            chartPart.ChartSpace = new ChartSpace();

            Chart chart = GenerateScatterChartContent(sheetName, facilityCells, performanceCells, tierCells, chartStartingRow + 1, dataStartingRow, dataEndingRow, facilityDataColumn, performanceDataColumn);
            chartPart.ChartSpace.Append(chart);

            TwoCellAnchor twoCellAnchor = ExcelChartHelper.GenerateWorksheetDrawingContents(drawingsPart, chartPart, chartTitle, chartStartingRow, 0, chartHeightInRows, chartWidthInColumns);
            drawingsPart.WorksheetDrawing.Append(twoCellAnchor);
            drawingsPart.WorksheetDrawing.Save();

            return rowCount;
        }

        /// <summary>
        /// Skips the rows corresponding to the specified height in pixels.
        /// </summary>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <param name="pixelHeight">Height in pixels.</param>
        public void SkipRows(string sheetName, int pixelHeight)
        {
            var skipRowCount = (int)Math.Ceiling(pixelHeight / (DefaultRowHeight * PixelToHeightConversion));
            for (int i = 0; i < skipRowCount; i++)
            {
                AddRow(sheetName, new[] { String.Empty });
            }
        }

        /// <summary>
        /// Adds a new row of data to the sheet.
        /// </summary>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <param name="items">The items to add as cells.</param>
        public void AddRow(string sheetName, IEnumerable<SpreadsheetCell> items)
        {
            SheetData data = _worksheetData[sheetName];
            _dataRowMode[sheetName] = true;

            int rowCount = data.ChildElements.Count;
            int rowNumber = rowCount + 1;

            string calculatedSpan = String.Format("1:{0}", items.Count());
            var row = new Row { RowIndex = Convert.ToUInt32(rowNumber), Spans = new ListValue<StringValue> { InnerText = calculatedSpan } };

            int column = 1;
            foreach (SpreadsheetCell item in items)
            {
                if (item == SpreadsheetCell.Empty)
                {
                    column++;
                    continue;
                }

                if (item.SpecifiesCell && !_gridData.ContainsKey(sheetName))
                {
                 //   SLogger.Debug(String.Format("WARNING: Ignoring cell location for {0}", item));
                }

                var cell = new Cell { CellReference = String.Format("{0}{1}", CellUtilities.ConvertIntToColumnId(column), rowNumber), DataType = item.Type };
                if (item.StyleId.HasValue)
                {
                    cell.StyleIndex = item.StyleId.Value;
                }
                SpreadsheetCellFormula cellFormula = item.CellFormula;
                if (cellFormula != null)
                {
                    cell.CellFormula = new CellFormula(cellFormula.Text);

                    Workbook workbook = _document.WorkbookPart.Workbook;
                    workbook.CalculationProperties.ForceFullCalculation = true;
                    workbook.CalculationProperties.FullCalculationOnLoad = true;
                }

				var cellValue = new CellValue { Text = item.Data };

				if (item.PreserveSpacing)
					cellValue.Space = SpaceProcessingModeValues.Preserve;

				cell.Append(cellValue);
				row.Append(cell);

                // Give coords back to item.
                item.SetColumn(CellUtilities.ConvertIntToColumnId(column));
                item.SetRow(rowNumber);

                column++;
            }

            data.Append(row);
        }

        /// <summary>
        /// Adds a new row of data to the sheet.
        /// </summary>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <param name="items">The items.</param>
        public void AddRow(string sheetName, IEnumerable<string> items)
        {
            AddRow(sheetName, items.Select(item => new SpreadsheetCell(CellValues.String, item)));
        }

        /// <summary>
        /// Adds the row dynamically assigning CellValue based on item.
        /// </summary>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <param name="items">The items.</param>
        public void AddRowDynamic(string sheetName, IEnumerable<string> items)
        {
            AddRow(sheetName, items.Select(item => new SpreadsheetCell(GetCellValue(item), item)));
        }

        /// <summary>
        /// Checks for presence of a worksheet by name.
        /// </summary>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <returns></returns>
        public bool HasWorksheet(string sheetName)
        {
            return (_worksheetData != null && _worksheetData.ContainsKey(sheetName));
        }

        private CellValues GetCellValue(string item)
        {
            double doubleNum;
            int intNum;

            if (double.TryParse(item, out doubleNum))
            {
                return CellValues.Number;
            }

            if (Int32.TryParse(item, out intNum))
            {
                return CellValues.Number;
            }

            return CellValues.String;
        }

        private HeadingPairs GetHeadingPairs()
        {
            var headingPairs = new HeadingPairs();

            var vTVector = new VTVector { BaseType = VectorBaseValues.Variant, Size = 2U };

            var variant1 = new Variant();
            variant1.Append(new VTLPSTR { Text = "Worksheets" });

            var variant2 = new Variant();
            variant2.Append(new VTInt32 { Text = _worksheetCounter.ToString() });

            vTVector.Append(variant1);
            vTVector.Append(variant2);

            headingPairs.Append(vTVector);
            return headingPairs;
        }

        private void GenerateExtendedFileProperties(ExtendedFilePropertiesPart extendedFileProperties)
        {
            var properties = new Properties();

            properties.Append(new DocumentSecurity { Text = "0" });
            properties.Append(new ScaleCrop { Text = "false" });

            properties.Append(GetHeadingPairs());

            var titlesOfParts = new TitlesOfParts();

            var vTVector = new VTVector { BaseType = VectorBaseValues.Lpstr, Size = 0U };

            titlesOfParts.Append(vTVector);
            properties.Append(titlesOfParts);

            properties.Append(new LinksUpToDate { Text = "false" });
            properties.Append(new SharedDocument { Text = "false" });
            properties.Append(new HyperlinksChanged { Text = "false" });
            properties.Append(new ApplicationVersion { Text = "12.0000" });

            extendedFileProperties.Properties = properties;
        }

        private void GenerateWorkbook(WorkbookPart workbookPart)
        {
            var workbook = new Workbook();
            var fileVersion = new FileVersion { ApplicationName = "xl", LastEdited = "4", LowestEdited = "4", BuildVersion = "4506" };
            workbook.Append(fileVersion);
            var workbookProperties = new WorkbookProperties { DefaultThemeVersion = 124226U };
            workbook.Append(workbookProperties);

            var bookViews = new BookViews();
            var workbookView = new WorkbookView { XWindow = 480, YWindow = 45, WindowWidth = 12435U, WindowHeight = 7965U };
            bookViews.Append(workbookView);
            workbook.Append(bookViews);

            var sheets = new Sheets();
            workbook.Append(sheets);

            var calculationProperties = new CalculationProperties { CalculationId = 125725U };
            workbook.Append(calculationProperties);

            workbookPart.Workbook = workbook;
        }

        private SheetData GenerateWorksheetPartContent(WorksheetPart worksheetPart, Columns columns, bool usesConditionalFormatting)
        {
            var worksheet = new Worksheet();
            worksheet.Append(new SheetDimension { Reference = "A1" });

            var sheetViews = new SheetViews();
            sheetViews.Append(new SheetView { TabSelected = _firstSheet, WorkbookViewId = (UInt32Value)0U });
            worksheet.Append(sheetViews);

            worksheet.Append(new SheetFormatProperties { DefaultRowHeight = DefaultRowHeight, DefaultColumnWidth = _defaultColumnWidth });
            var sheetData = new SheetData();

            if (columns != null)
            {
                worksheet.Append(columns);
            }

            worksheet.Append(sheetData);
            if (!usesConditionalFormatting)
            {
                worksheet.Append(new PageMargins { Left = 0.7D, Right = 0.7D, Top = 0.75D, Bottom = 0.75D, Header = 0.3D, Footer = 0.3D });
            }

            worksheetPart.Worksheet = worksheet;

            return sheetData;
        }

        protected virtual void SetPackageProperties(SpreadsheetDocument document)
        {
            document.PackageProperties.Creator = "Infosario ClinWeb";
            document.PackageProperties.Created = SystemTime.Now();
            document.PackageProperties.Modified = SystemTime.Now();
            document.PackageProperties.LastModifiedBy = "";
        }

        private void GenerateStyles(WorkbookPart part)
        {
            var stylesPart = part.AddNewPart<WorkbookStylesPart>();
            var stylesheet = new Stylesheet();

            stylesheet.Append(GenerateFonts());
            stylesheet.Append(GenerateFills());
            stylesheet.Append(GenerateBorders());
            stylesheet.Append(GenerateCellStyleFormats());
            stylesheet.Append(GenerateCellFormats());
            stylesheet.Append(GenerateCellStyles());

            stylesPart.Stylesheet = stylesheet;
        }

        private static Fonts GenerateFonts()
        {
            var fonts = new Fonts { Count = 2U };

            var font1 = new Font();
            font1.Append(new FontSize { Val = 11D });
            font1.Append(new Color { Theme = (UInt32Value)1U });
            font1.Append(new FontName { Val = "Calibri" });
            font1.Append(new FontFamilyNumbering { Val = 2 });
            font1.Append(new FontScheme { Val = FontSchemeValues.Minor });
            fonts.Append(font1);

            var font2 = new Font();
            font2.Append(new FontSize { Val = 11D });
            font2.Append(new Color { Theme = (UInt32Value)1U });
            font2.Append(new FontName { Val = "Calibri" });
            font2.Append(new FontFamilyNumbering { Val = 2 });
            font2.Append(new FontScheme { Val = FontSchemeValues.Minor });
            font2.Bold = new Bold { Val = true };
            fonts.Append(font2);
            return fonts;
        }

        private static Fills GenerateFills()
        {
            var fills = new Fills { Count = 8U };

            var fill1 = new Fill();
            fill1.Append(new PatternFill { PatternType = PatternValues.None });
            fills.Append(fill1);

            var fill2 = new Fill();
            fill2.Append(new PatternFill { PatternType = PatternValues.Gray125 });
            fills.Append(fill2);

            var fill3 = new Fill(new PatternFill(new SpreadsheetColor.ForegroundColor()
            {
                Rgb = new HexBinaryValue() { Value = "FF00CB33" }, //Green color
            })
            { PatternType = PatternValues.Solid });
            fills.Append(fill3);

            var fill4 = new Fill(new PatternFill(new SpreadsheetColor.ForegroundColor()
            {
                Rgb = new HexBinaryValue() { Value = "FFFFFF00" }, //Yellow color
            })
            { PatternType = PatternValues.Solid });
            fills.Append(fill4);

            var fill5 = new Fill(new PatternFill(new SpreadsheetColor.ForegroundColor()
            {
                Rgb = new HexBinaryValue() { Value = "FFFF0000" }, //Red color
            })
            { PatternType = PatternValues.Solid });
            fills.Append(fill5);

            var fill6 = new Fill(new PatternFill(new SpreadsheetColor.ForegroundColor()
            {
                Rgb = new HexBinaryValue() { Value = "FFFFA500" }, //Orange color
            })
            { PatternType = PatternValues.Solid });
            fills.Append(fill6);

            var fill7 = new Fill(new PatternFill(new SpreadsheetColor.ForegroundColor()
            {
                Rgb = new HexBinaryValue() { Value = "FF404040" }, //Dark gray color
            })
            { PatternType = PatternValues.Solid });
            fills.Append(fill7);

            var fill8 = new Fill(new PatternFill(new SpreadsheetColor.ForegroundColor()
            {
                Rgb = new HexBinaryValue() { Value = "FF808080" }, //Light gray color
            })
            { PatternType = PatternValues.Solid });
            fills.Append(fill8);
            return fills;
        }

        private static Borders GenerateBorders()
        {
            var borders = new Borders { Count = 5U };

            // No border
            var border1 = new Border();

            border1.Append(new LeftBorder());
            border1.Append(new RightBorder());
            border1.Append(new TopBorder());
            border1.Append(new BottomBorder());

            borders.Append(border1);

            // Border on all sides
            var border2 = new Border();

            const string borderColor = "FF7F7F7F";
            var leftBorder2 = new LeftBorder { Style = BorderStyleValues.Thin };
            leftBorder2.Append(new Color { Rgb = borderColor });

            var rightBorder2 = new RightBorder { Style = BorderStyleValues.Thin };
            rightBorder2.Append(new Color { Rgb = borderColor });

            var topBorder2 = new TopBorder { Style = BorderStyleValues.Thin };
            topBorder2.Append(new Color { Rgb = borderColor });

            var bottomBorder2 = new BottomBorder { Style = BorderStyleValues.Thin };
            bottomBorder2.Append(new Color { Rgb = borderColor });

            border2.Append(leftBorder2);
            border2.Append(rightBorder2);
            border2.Append(topBorder2);
            border2.Append(bottomBorder2);

            borders.Append(border2);

            // Border: Top, Left, Right
            var borderTLR = new Border();

            var leftBorderTLR = new LeftBorder { Style = BorderStyleValues.Thin };
            leftBorderTLR.Append(new Color { Rgb = borderColor });

            var rightBorderTLR = new RightBorder { Style = BorderStyleValues.Thin };
            rightBorderTLR.Append(new Color { Rgb = borderColor });

            var topBorderTLR = new TopBorder { Style = BorderStyleValues.Thin };
            topBorderTLR.Append(new Color { Rgb = borderColor });

            var bottomBorderTLR = new BottomBorder { Style = BorderStyleValues.None };

            borderTLR.Append(leftBorderTLR);
            borderTLR.Append(rightBorderTLR);
            borderTLR.Append(topBorderTLR);
            borderTLR.Append(bottomBorderTLR);

            borders.Append(borderTLR);

            // Border: Left, Right
            var borderLR = new Border();

            var leftBorderLR = new LeftBorder { Style = BorderStyleValues.Thin };
            leftBorderLR.Append(new Color { Rgb = borderColor });

            var rightBorderLR = new RightBorder { Style = BorderStyleValues.Thin };
            rightBorderLR.Append(new Color { Rgb = borderColor });

            var topBorderLR = new TopBorder { Style = BorderStyleValues.None };

            var bottomBorderLR = new BottomBorder { Style = BorderStyleValues.None };

            borderLR.Append(leftBorderLR);
            borderLR.Append(rightBorderLR);
            borderLR.Append(topBorderLR);
            borderLR.Append(bottomBorderLR);

            borders.Append(borderLR);

            // Border: Left, Right, Bottom
            var borderLRB = new Border();

            var leftBorderLRB = new LeftBorder { Style = BorderStyleValues.Thin };
            leftBorderLRB.Append(new Color { Rgb = borderColor });

            var rightBorderLRB = new RightBorder { Style = BorderStyleValues.Thin };
            rightBorderLRB.Append(new Color { Rgb = borderColor });

            var topBorderLRB = new TopBorder { Style = BorderStyleValues.None };

            var bottomBorderLRB = new BottomBorder { Style = BorderStyleValues.Thin };
            bottomBorderLRB.Append(new Color { Rgb = borderColor });

            borderLRB.Append(leftBorderLRB);
            borderLRB.Append(rightBorderLRB);
            borderLRB.Append(topBorderLRB);
            borderLRB.Append(bottomBorderLRB);

            borders.Append(borderLRB);
            return borders;
        }

        private static CellStyleFormats GenerateCellStyleFormats()
        {
            var cellStyleList = new List<CellFormat>
                                {
                                    new CellFormat
                                    {
                                        NumberFormatId = 0U,
                                        FontId = 0U,
                                        FillId = 0U,
                                        BorderId = 0U
                                    },
                                    new CellFormat
                                    {
                                        NumberFormatId = 0U,
                                        FontId = 0U,
                                        FillId = 0U,
                                        BorderId = 1U
                                    },
                                    new CellFormat
                                    {
                                        NumberFormatId = 0U,
                                        FontId = 0U,
                                        FillId = 0U,
                                        BorderId = 1U,
                                        Alignment = new Alignment {Horizontal = HorizontalAlignmentValues.Center},
                                        ApplyAlignment = true
                                    },
                                    new CellFormat
                                    {
                                        NumberFormatId = 0U,
                                        FontId = 1U,
                                        FillId = 0U,
                                        BorderId = 0U
                                    },
                                    new CellFormat
                                    {
                                        NumberFormatId = 0U,
                                        FontId = 1U,
                                        FillId = 0U,
                                        BorderId = 1U,
                                        Alignment = new Alignment {Horizontal = HorizontalAlignmentValues.Center},
                                        ApplyAlignment = true
                                    },
                                    new CellFormat
                                    {
                                        NumberFormatId = 0U,
                                        FontId = 0U,
                                        FillId = 0U,
                                        BorderId = 2U,
                                        Alignment = new Alignment {Vertical = VerticalAlignmentValues.Top, WrapText = true},
                                        ApplyAlignment = true
                                    },
                                    new CellFormat
                                    {
                                        NumberFormatId = 0U,
                                        FontId = 1U,
                                        FillId = 0U,
                                        BorderId = 2U,
                                        Alignment = new Alignment {Vertical = VerticalAlignmentValues.Top, WrapText = true},
                                        ApplyAlignment = true
                                    },
                                    new CellFormat
                                    {
                                        NumberFormatId = 0U,
                                        FontId = 0U,
                                        FillId = 0U,
                                        BorderId = 3U,
                                        Alignment = new Alignment {Vertical = VerticalAlignmentValues.Top, WrapText = true},
                                        ApplyAlignment = true
                                    },
                                    new CellFormat
                                    {
                                        NumberFormatId = 0U,
                                        FontId = 1U,
                                        FillId = 0U,
                                        BorderId = 3U,
                                        Alignment = new Alignment {Vertical = VerticalAlignmentValues.Top, WrapText = true},
                                        ApplyAlignment = true
                                    },
                                    new CellFormat
                                    {
                                        NumberFormatId = 0U,
                                        FontId = 0U,
                                        FillId = 0U,
                                        BorderId = 4U,
                                        Alignment = new Alignment {Vertical = VerticalAlignmentValues.Top, WrapText = true},
                                        ApplyAlignment = true
                                    },
                                    new CellFormat
                                    {
                                        NumberFormatId = 0U,
                                        FontId = 1U,
                                        FillId = 0U,
                                        BorderId = 4U,
                                        Alignment = new Alignment {Vertical = VerticalAlignmentValues.Top, WrapText = true},
                                        ApplyAlignment = true
                                    },
                                    new CellFormat
                                    {
                                        NumberFormatId = 0U,
                                        FontId = 0U,
                                        FillId = 0U,
                                        BorderId = 0U,
                                        Alignment =
                                            new Alignment
                                            {
                                                Horizontal = HorizontalAlignmentValues.Center,
                                                Vertical = VerticalAlignmentValues.Top,
                                                WrapText = true
                                            },
                                        ApplyAlignment = true
                                    },
                                    new CellFormat
                                    {
                                        NumberFormatId = 0U,
                                        FontId = 0U,
                                        FillId = 0U,
                                        BorderId = 0U,
                                        Alignment = new Alignment {Vertical = VerticalAlignmentValues.Top, WrapText = true},
                                        ApplyAlignment = true
                                    }
                                };

            var cellStyleFormats = new CellStyleFormats { Count = (uint)cellStyleList.Count };
            cellStyleFormats.Append(cellStyleList);
            return cellStyleFormats;
        }

        private static CellStyles GenerateCellStyles()
        {
            var cellStyles = new CellStyles { Count = 1U };
            cellStyles.Append(new OpenXmlElement[]
                              {
                                  new CellStyle
                                  {
                                      Name = "Normal",
                                      FormatId = 0U,
                                      BuiltinId = 0U
                                  }
                              });
            return cellStyles;
        }

        private CellFormats GenerateCellFormats()
        {
            var cellFormatList = new List<CellFormat>
                                 {
                                     new CellFormat
                                     {
                                         NumberFormatId = 0U,
                                         FontId = 0U,
                                         FillId = 0U,
                                         BorderId = 0U,
                                         FormatId = 0U
                                     },
                                     new CellFormat
                                     {
                                         NumberFormatId = 0U,
                                         FontId = 0U,
                                         FillId = 0U,
                                         BorderId = 1U,
                                         FormatId = 0U,
                                         ApplyBorder = true
                                     },
                                     new CellFormat
                                     {
                                         NumberFormatId = 0U,
                                         FontId = 0U,
                                         FillId = 0U,
                                         BorderId = 1U,
                                         FormatId = 0U,
                                         ApplyBorder = true,
                                         Alignment = new Alignment {Horizontal = HorizontalAlignmentValues.Center},
                                         ApplyAlignment = true
                                     },
                                     new CellFormat
                                     {
                                         NumberFormatId = 0U,
                                         FontId = 1U,
                                         FillId = 0U,
                                         BorderId = 0U,
                                         FormatId = 0U
                                     },
                                     new CellFormat
                                     {
                                         NumberFormatId = 0U,
                                         FontId = 1U,
                                         FillId = 0U,
                                         BorderId = 1U,
                                         FormatId = 0U,
                                         ApplyBorder = true,
                                         Alignment = new Alignment {Horizontal = HorizontalAlignmentValues.Center},
                                         ApplyAlignment = true
                                     },
                                     new CellFormat
                                     {
                                         NumberFormatId = 0U,
                                         FontId = 0U,
                                         FillId = 0U,
                                         BorderId = 2U,
                                         FormatId = 0U,
                                         ApplyBorder = true,
                                         Alignment = new Alignment {Vertical = VerticalAlignmentValues.Top, WrapText = true},
                                         ApplyAlignment = true
                                     },
                                     new CellFormat
                                     {
                                         NumberFormatId = 0U,
                                         FontId = 1U,
                                         FillId = 0U,
                                         BorderId = 2U,
                                         FormatId = 0U,
                                         ApplyBorder = true,
                                         Alignment = new Alignment {Vertical = VerticalAlignmentValues.Top, WrapText = true},
                                         ApplyAlignment = true
                                     },
                                     new CellFormat
                                     {
                                         NumberFormatId = 0U,
                                         FontId = 0U,
                                         FillId = 0U,
                                         BorderId = 3U,
                                         FormatId = 0U,
                                         ApplyBorder = true,
                                         Alignment = new Alignment {Vertical = VerticalAlignmentValues.Top, WrapText = true},
                                         ApplyAlignment = true
                                     },
                                     new CellFormat
                                     {
                                         NumberFormatId = 0U,
                                         FontId = 1U,
                                         FillId = 0U,
                                         BorderId = 3U,
                                         FormatId = 0U,
                                         ApplyBorder = true,
                                         Alignment = new Alignment {Vertical = VerticalAlignmentValues.Top, WrapText = true},
                                         ApplyAlignment = true
                                     },
                                     new CellFormat
                                     {
                                         NumberFormatId = 0U,
                                         FontId = 0U,
                                         FillId = 0U,
                                         BorderId = 4U,
                                         FormatId = 0U,
                                         ApplyBorder = true,
                                         Alignment = new Alignment {Vertical = VerticalAlignmentValues.Top, WrapText = true},
                                         ApplyAlignment = true
                                     },
                                     new CellFormat
                                     {
                                         NumberFormatId = 0U,
                                         FontId = 1U,
                                         FillId = 0U,
                                         BorderId = 4U,
                                         FormatId = 0U,
                                         ApplyBorder = true,
                                         Alignment = new Alignment {Vertical = VerticalAlignmentValues.Top, WrapText = true},
                                         ApplyAlignment = true
                                     },
                                     new CellFormat
                                     {
                                         NumberFormatId = 0U,
                                         FontId = 0U,
                                         FillId = 0U,
                                         BorderId = 0U,
                                         FormatId = 0U,
                                         ApplyBorder = true,
                                         Alignment =
                                             new Alignment
                                             {
                                                 Horizontal = HorizontalAlignmentValues.Center,
                                                 Vertical = VerticalAlignmentValues.Top,
                                                 WrapText = true
                                             },
                                         ApplyAlignment = true,
                                     },
                                     new CellFormat
                                     {
                                         NumberFormatId = 0U,
                                         FontId = 0U,
                                         FillId = 0U,
                                         BorderId = 0U,
                                         FormatId = 0U,
                                         ApplyBorder = true,
                                         Alignment = new Alignment {Vertical = VerticalAlignmentValues.Top, WrapText = true},
                                         ApplyAlignment = true,
                                     },
                                     new CellFormat
                                     {
                                         NumberFormatId = 0U,
                                         FontId = 0U,
                                         FillId = 2U,
                                         BorderId = 0U,
                                         FormatId = 0U,
                                         ApplyFill = true,
                                     },
                                     new CellFormat
                                     {
                                         NumberFormatId = 0U,
                                         FontId = 0U,
                                         FillId = 3U,
                                         BorderId = 0U,
                                         FormatId = 0U,
                                         ApplyFill = true,
                                     },
                                     new CellFormat
                                     {
                                         NumberFormatId = 0U,
                                         FontId = 0U,
                                         FillId = 4U,
                                         BorderId = 0U,
                                         FormatId = 0U,
                                         ApplyFill = true,
                                     },
                                     new CellFormat
                                     {
                                         NumberFormatId = 0U,
                                         FontId = 0U,
                                         FillId = 5U,
                                         BorderId = 0U,
                                         FormatId = 0U,
                                         ApplyFill = true,
                                     },
                                     new CellFormat
                                     {
                                         NumberFormatId = 0U,
                                         FontId = 0U,
                                         FillId = 6U,
                                         BorderId = 0U,
                                         FormatId = 0U,
                                         ApplyFill = true,
                                     },
                                     new CellFormat
                                     {
                                         NumberFormatId = 0U,
                                         FontId = 0U,
                                         FillId = 7U,
                                         BorderId = 0U,
                                         FormatId = 0U,
                                         ApplyFill = true,
                                     }
                                 };

            for (uint i = 0; i < 20; i++)
            {
                cellFormatList.Add(new CellFormat
                {
                    NumberFormatId = 0U,
                    FontId = 0U,
                    FillId = 0U,
                    BorderId = 0U,
                    Alignment = new Alignment { Vertical = VerticalAlignmentValues.Top, Indent = new UInt32Value(i) },
                    ApplyAlignment = true
                });
                AddIndentLevelStyle(i, (uint)cellFormatList.Count - 1);
            }

            var cellFormats = new CellFormats { Count = (uint)cellFormatList.Count };
            cellFormats.Append(cellFormatList);
            return cellFormats;
        }

        private Dictionary<uint, uint> _indentationLevelReference;

        private void AddIndentLevelStyle(uint indentLevel, uint styleId)
        {
            if (_indentationLevelReference == null)
                _indentationLevelReference = new Dictionary<uint, uint>();

            _indentationLevelReference.Add(indentLevel, styleId);
        }

        public uint GetIndentLevelStyle(int indentLevel)
        {
            return _indentationLevelReference[(uint)indentLevel];
        }

        private void GenerateDrawingContent(WorksheetDrawing worksheetDrawing, int pixelWidth, int pixelHeight, int xOffset, int yOffset, string rId)
        {
            const long sizeMultiplier96Dpi = 9525;
            var extents = new Extents { Cx = pixelWidth * sizeMultiplier96Dpi, Cy = pixelHeight * sizeMultiplier96Dpi };
            var anchor = new AbsoluteAnchor { Position = new Position { X = xOffset * sizeMultiplier96Dpi, Y = yOffset * sizeMultiplier96Dpi }, Extent = new Extent { Cx = extents.Cx, Cy = extents.Cy } };

            var picture = new Picture();

            var nonVisualPictureProperties = new NonVisualPictureProperties();
            var nonVisualPictureDrawingProperties = new NonVisualPictureDrawingProperties();
            nonVisualPictureDrawingProperties.Append(new PictureLocks { NoChangeAspect = true });

            nonVisualPictureProperties.Append(new NonVisualDrawingProperties { Id = 2U, Name = "Picture 1", Description = "Image" });
            nonVisualPictureProperties.Append(nonVisualPictureDrawingProperties);

            var blipFill = new BlipFill();
            var stretch = new Stretch();
            stretch.Append(new FillRectangle());

            blipFill.Append(new Blip { Embed = rId, CompressionState = BlipCompressionValues.Print });
            blipFill.Append(stretch);

            var shapeProperties = new ShapeProperties();

            var transform2D = new Transform2D();
            transform2D.Append(new Offset { X = anchor.Position.X, Y = anchor.Position.Y });
            transform2D.Append(extents);

            var presetGeometry = new PresetGeometry { Preset = ShapeTypeValues.Rectangle };
            presetGeometry.Append(new AdjustValueList());

            shapeProperties.Append(transform2D);
            shapeProperties.Append(presetGeometry);

            picture.Append(nonVisualPictureProperties);
            picture.Append(blipFill);
            picture.Append(shapeProperties);

            anchor.Append(picture);
            anchor.Append(new ClientData());

            worksheetDrawing.Append(anchor);
        }

        private Chart GenerateScatterChartContent(string worksheetName, IEnumerable<SpreadsheetCell> facilityCells, IEnumerable<SpreadsheetCell> performanceCells, IEnumerable<IEnumerable<SpreadsheetCell>> tierCells, int tierStartingRow, int dataStartingRow, int dataEndingRow, int facilityDataColumn, int performanceDataColumn)
        {
            var rowCount = dataEndingRow - dataStartingRow + 1;

            Chart chart = new Chart
            {
                PlotArea = new PlotArea { Layout = new Layout() },
                PlotVisibleOnly = new PlotVisibleOnly { Val = true }
            };

            ScatterChart scatterChart = new ScatterChart
            {
                ScatterStyle = new ScatterStyle { Val = ScatterStyleValues.LineMarker },
                VaryColors = new VaryColors { Val = false }
            };

            uint seriesIndex = 0;

            ScatterChartSeries performanceSeries = ExcelChartHelper.GenerateScatterChartSeries(seriesIndex);
            XValues xValues = ExcelChartHelper.GenerateScatterXValuesWithStringReferences(worksheetName, facilityDataColumn, facilityDataColumn, dataStartingRow, dataEndingRow, facilityCells);
            YValues yValues = ExcelChartHelper.GenerateScatterYValuesWithNumberReferences(worksheetName, performanceDataColumn, performanceDataColumn, dataStartingRow, dataEndingRow, performanceCells);
            performanceSeries.Append(xValues);
            performanceSeries.Append(yValues);
            scatterChart.Append(performanceSeries);

            int tierIndex = 0;
            IEnumerable<SpreadsheetCell> tierHeaderRow = tierCells.First();
            foreach (List<SpreadsheetCell> row in tierCells.Skip(1))
            {
                tierIndex++;
                string tierText = row[0].Data;
                string tier = tierText.Substring(4);
                var colorScore = ColorCriteriaManager.ColorScoring.FirstOrDefault(cs => cs.Value.ScoreSet == ColorCriteriaManager.V2
                                                                                     && cs.Value.IsTierColor
                                                                                     && cs.Value.TierDisplayText == tier);
                string colorText = colorScore.Value.ColorText;
                string rgb = null;
                if (colorText == "Red")
                {
                    rgb = "FF0000";
                }
                else if (colorText == "Yellow")
                {
                    rgb = "FFFF00";
                }
                else if (colorText == "Green")
                {
                    rgb = "00B050";
                }
                ScatterChartSeries tierSeries = ExcelChartHelper.GenerateScatterChartSeries(++seriesIndex, rgb, MarkerStyleValues.Circle);
                SeriesText seriesText = new SeriesText
                {
                    NumericValue = new NumericValue
                    {
                        Text = tierText
                    }
                };
                tierSeries.Append(seriesText);

                DataLabels dataLabels = new DataLabels();
                DataLabel dataLabel1 = new DataLabel();
                Index dataLabelIndex1 = new Index { Val = (uint)0 };
                Delete delete = new Delete { Val = true };
                dataLabel1.Append(dataLabelIndex1);
                dataLabel1.Append(delete);
                dataLabels.Append(dataLabel1);
                DataLabelPosition dataLabelPosition = new DataLabelPosition { Val = DataLabelPositionValues.Top };
                dataLabels.Append(dataLabelPosition);
                ShowSeriesName showSeriesName = new ShowSeriesName { Val = true };
                dataLabels.Append(showSeriesName);
                tierSeries.Append(dataLabels);

                XValues tierXvalues = ExcelChartHelper.GenerateScatterXValuesWithNumberReferences(worksheetName, 2, 3, tierStartingRow, tierStartingRow, tierHeaderRow.Skip(1));
                YValues tierYvalues = ExcelChartHelper.GenerateScatterYValuesWithNumberReferences(worksheetName, 2, 3, tierStartingRow + tierIndex, tierStartingRow + tierIndex, row.Skip(1));
                tierSeries.Append(tierXvalues);
                tierSeries.Append(tierYvalues);

                scatterChart.Append(tierSeries);
            }

            uint cnAxisId1 = 1;
            uint cnAxisId2 = 2;

            scatterChart.Append(new AxisId { Val = cnAxisId1 });
            scatterChart.Append(new AxisId { Val = cnAxisId2 });

            chart.PlotArea.Append(scatterChart);

            ValueAxis valueAxis1 = new ValueAxis();
            valueAxis1.AxisId = new AxisId { Val = cnAxisId1 };
            valueAxis1.Scaling = new Scaling
            {
                Orientation = new Orientation { Val = OrientationValues.MinMax },
                MinAxisValue = new MinAxisValue { Val = 0D },
                MaxAxisValue = new MaxAxisValue { Val = (rowCount + 10) / 10 * 10 }
            };
            valueAxis1.AxisPosition = new AxisPosition { Val = AxisPositionValues.Bottom };
            valueAxis1.MajorGridlines = ExcelChartHelper.GenerateMajorGridlines();
            valueAxis1.NumberingFormat = new NumberingFormat
            {
                FormatCode = "General",
                SourceLinked = true
            };
            valueAxis1.TickLabelPosition = new TickLabelPosition { Val = TickLabelPositionValues.NextTo };
            valueAxis1.CrossingAxis = new CrossingAxis { Val = cnAxisId2 };
            valueAxis1.Append(new Crosses { Val = CrossesValues.AutoZero });
            valueAxis1.Append(ExcelChartHelper.GenerateAxisTitle("Site", true));
            chart.PlotArea.Append(valueAxis1);

            ValueAxis valueAxis2 = new ValueAxis();
            valueAxis2.AxisId = new AxisId { Val = cnAxisId2 };
            valueAxis2.Scaling = new Scaling
            {
                Orientation = new Orientation { Val = OrientationValues.MinMax }
            };
            valueAxis2.AxisPosition = new AxisPosition { Val = AxisPositionValues.Left };
            valueAxis2.MajorGridlines = ExcelChartHelper.GenerateMajorGridlines();
            valueAxis2.NumberingFormat = new NumberingFormat
            {
                FormatCode = "General",
                SourceLinked = true
            };
            valueAxis2.TickLabelPosition = new TickLabelPosition { Val = TickLabelPositionValues.NextTo };
            valueAxis2.CrossingAxis = new CrossingAxis { Val = cnAxisId1 };
            valueAxis2.Append(new Crosses { Val = CrossesValues.AutoZero });
            valueAxis2.Append(ExcelChartHelper.GenerateAxisTitle("Tier Index", false));
            chart.PlotArea.Append(valueAxis2);

            return chart;
        }

        private void AddChartContent(string worksheetName, ChartPart chartPart, IEnumerable<IEnumerable<SpreadsheetCell>> allData, string xAxisLabel = "", string yAxisLabel = "")
        {
            IEnumerable<SpreadsheetCell> headerRow = allData.First();
            IEnumerable<IEnumerable<SpreadsheetCell>> data = from row in allData.Skip(1)
                                                             select row.Skip(1);

            string[] columnNameIndex = headerRow.Select(c => c.ColumnId).ToArray();
            string[] categoryNames = headerRow.Skip(1).Select(c => c.Data).ToArray();
            int categoryStartColumnIndex = columnNameIndex.Length - categoryNames.Length + 1;
            int dataStartRowIndex = headerRow.First().RowId + 1;
            int columns = categoryNames.Length;

            uint cnAxisId1 = 1;
            uint cnAxisId2 = 2;

            var chart = new Chart { PlotArea = new PlotArea { Layout = new Layout() } };

            LineChartSeries xAxis;

            var lineChart = new LineChart();

            var yAxis = new CategoryAxisData
            {
                StringReference = new StringReference
                {
                    Formula = new Formula
                    {
                        Text = string.Format("'{0}'!${1}${2}:${1}${3}",
                                             worksheetName,
                                             columnNameIndex[categoryStartColumnIndex - 2],
                                             dataStartRowIndex,
                                             data.Count() + dataStartRowIndex - 1)
                    }
                },
            };
            yAxis.StringReference.StringCache = new StringCache();
            for (int j = 0; j < data.Count(); ++j)
            {
                yAxis.StringReference.StringCache.Append(new StringPoint
                {
                    Index = (uint)j,
                    NumericValue = new NumericValue(j.ToString("00"))
                });
            }
            yAxis.StringReference.StringCache.PointCount = new PointCount { Val = (uint)data.Count() };

            Values values;
            for (int columnIndex = 0; columnIndex < columns; ++columnIndex)
            {
                string currentColumnName = columnNameIndex[categoryStartColumnIndex - 1 + columnIndex];
                string categoryName = categoryNames[columnIndex];

                xAxis = new LineChartSeries
                {
                    Index = new Index { Val = (uint)columnIndex },
                    Order = new Order { Val = (uint)columnIndex },
                    SeriesText = new SeriesText
                    {
                        StringReference = new StringReference
                        {
                            Formula = new Formula
                            {
                                Text = string.Format("'{0}'!${1}${2}",
                                                     worksheetName,
                                                     currentColumnName,
                                                     dataStartRowIndex - 1)
                            }
                        }
                    }
                };

                xAxis.SeriesText.StringReference.StringCache = new StringCache { PointCount = new PointCount { Val = 1 } };
                xAxis.SeriesText.StringReference.StringCache.Append(new StringPoint
                {
                    Index = 0,
                    NumericValue = new NumericValue(categoryName)
                });

                // the contents for the category data is the same for every data series
                // But we can't just append it because the variable is appended by reference
                // and not by value. So we need to clone it.
                xAxis.Append((CategoryAxisData)yAxis.CloneNode(true));

                int lastRowOfData = FindLastRowOfDataForCurrentColumn(columnIndex, data) + dataStartRowIndex;

                values = new Values
                {
                    NumberReference = new NumberReference
                    {
                        Formula = new Formula
                        {
                            Text = string.Format("'{0}'!${1}${2}:${1}${3}",
                                                 worksheetName,
                                                 currentColumnName,
                                                 dataStartRowIndex,
                                                 lastRowOfData)
                        }
                    }
                };

                values.NumberReference.NumberingCache = new NumberingCache { FormatCode = new FormatCode("General") };
                for (int j = 0; j < data.Count(); ++j)
                {
                    IEnumerable<SpreadsheetCell> row = data.Skip(j).First();
                    values.NumberReference.NumberingCache.Append(new NumericPoint
                    {
                        Index = (uint)j,
                        NumericValue = new NumericValue(row.Skip(columnIndex).First().Data)
                    });
                }
                values.NumberReference.NumberingCache.PointCount = new PointCount { Val = (uint)data.Count() };
                xAxis.Append(values);

                lineChart.Append(xAxis);
            }

            lineChart.Append(new AxisId { Val = cnAxisId1 });
            lineChart.Append(new AxisId { Val = cnAxisId2 });

            chart.PlotArea.Append(lineChart);

            var categoryAxis = new CategoryAxis();
            categoryAxis.AxisId = new AxisId { Val = cnAxisId1 };
            categoryAxis.Scaling = new Scaling
            {
                Orientation = new Orientation { Val = OrientationValues.MinMax }
            };
            categoryAxis.AxisPosition = new AxisPosition { Val = AxisPositionValues.Bottom };
            categoryAxis.TickLabelPosition = new TickLabelPosition { Val = TickLabelPositionValues.NextTo };
            categoryAxis.CrossingAxis = new CrossingAxis { Val = cnAxisId2 };
            categoryAxis.Append(new Crosses { Val = CrossesValues.AutoZero });
            categoryAxis.Append(new AutoLabeled { Val = true });
            categoryAxis.Append(new LabelAlignment { Val = LabelAlignmentValues.Center });
            categoryAxis.Append(new LabelOffset { Val = 100 });

            if (!string.IsNullOrWhiteSpace(xAxisLabel))
            {
                categoryAxis.Append(ExcelChartHelper.GenerateAxisTitle(xAxisLabel, true));
            }

            chart.PlotArea.Append(categoryAxis);

            var valueAxis = new ValueAxis
            {
                AxisId = new AxisId { Val = cnAxisId2 },
                Scaling = new Scaling
                {
                    Orientation = new Orientation { Val = OrientationValues.MinMax }
                },
                AxisPosition = new AxisPosition { Val = AxisPositionValues.Left },
                MajorGridlines = new MajorGridlines(),
                NumberingFormat = new NumberingFormat
                {
                    FormatCode = "General",
                    SourceLinked = true
                },
                TickLabelPosition = new TickLabelPosition { Val = TickLabelPositionValues.NextTo },
                CrossingAxis = new CrossingAxis { Val = cnAxisId1 }
            };

            valueAxis.Append(new Crosses { Val = CrossesValues.AutoZero });
            valueAxis.Append(new CrossBetween { Val = CrossBetweenValues.Between });

            if (!string.IsNullOrWhiteSpace(yAxisLabel))
            {
                valueAxis.Append(ExcelChartHelper.GenerateAxisTitle(yAxisLabel, false));
            }

            chart.PlotArea.Append(valueAxis);

            chart.Legend = new Legend { LegendPosition = new LegendPosition { Val = LegendPositionValues.Right } };
            chart.Legend.Append(new Layout());
            chart.Legend.Append(new Overlay { Val = false });

            chart.PlotVisibleOnly = new PlotVisibleOnly { Val = true };

            chartPart.ChartSpace.Append(chart);
        }

        private int FindLastRowOfDataForCurrentColumn(int i, IEnumerable<IEnumerable<SpreadsheetCell>> data)
        {
            int lastRowOfData = 0;
            int removeCount = 0;
            if (data.Any())
            {
                lastRowOfData = data.Count() + 1;
                while (data.Skip(data.Count() - removeCount - 1).First().Skip(i).First().Data == null)
                {
                    lastRowOfData--;
                    removeCount++;
                }
            }
            return lastRowOfData;
        }

        // TODO: This, or a variation of it, should move to the ExcelChartHelper class
        private DrawingsPart GetOrCreateChartPart(string sheetName, string chartTitle)
        {
            string sheetChartKey = sheetName + "|" + chartTitle;

            if (!_worksheetDrawings.ContainsKey(sheetChartKey))
            {
                WorksheetPart part = _worksheetParts[sheetName];
                if (!_imageIds.ContainsKey(sheetChartKey))
                {
                    _imageIds.Add(sheetChartKey, 0);
                }

                _imageIds[sheetChartKey]++;
                int id = _imageIds[sheetChartKey];
                string rId = String.Format("rId{0}", id);

                var drawingsPart = part.AddNewPart<DrawingsPart>(rId);
                drawingsPart.WorksheetDrawing = new WorksheetDrawing();
                _worksheetDrawings.Add(sheetChartKey, drawingsPart);

                part.Worksheet.Append(new Drawing { Id = rId });
            }

            return _worksheetDrawings[sheetChartKey];
        }

        #region IDisposable Dispose pattern

        private bool _disposed;

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                Debug.Fail("Multiple calls made to Dispose", String.Format("Dispose method should only be called on {0} once per instance.", GetType().FullName));
                return;
            }

            ReleaseUnmanagedResources();
            if (disposing)
            {
                ReleaseManagedResources();
            }

            _disposed = true;
        }

        /// <summary>
        ///     Releases unmanaged resources and performs other cleanup operations before the
        ///     <see cref="ExcelExporter" /> is reclaimed by garbage collection.
        /// </summary>
        ~ExcelExporter()
        {
            Dispose(false);
        }

        /// <summary>
        ///     Releases unmanaged resources during Dispose or finalization.
        /// </summary>
        /// <remarks>
        ///     This method should be overriden in any derived class that creates its
        ///     own unmanaged resources. A call to the base method should always be included.
        /// </remarks>
        protected virtual void ReleaseUnmanagedResources()
        {
            // TODO: Release unmanaged resources
        }

        /// <summary>
        ///     Releases managed resources during Dispose.
        /// </summary>
        /// <remarks>
        ///     This method should be overriden in any derived class that creates its
        ///     own managed resources. A call to the base method should always be included.
        /// </remarks>
        protected virtual void ReleaseManagedResources()
        {
            EndDocument();

            // TODO: Release managed resources
        }

        #endregion IDisposable Dispose pattern

        /// <summary>
        /// Provides a scoping object that automatically Begins and Ends data collection when used in a using statement.
        /// </summary>
        public class CollectionScope : IDisposable
        {
            private readonly IExcelExporter _exporter;

            /// <summary>
            ///     Initializes a new instance of the <see cref="CollectionScope" /> class.
            /// </summary>
            /// <param name="exporter">The exporter.</param>
            /// <param name="sheetName">Name of the sheet.</param>
            public CollectionScope(IExcelExporter exporter, string sheetName)
            {
                SheetName = sheetName;
                _exporter = exporter;

                _exporter.BeginGridDataCollection(sheetName);
            }

            #region IDisposable Members

            /// <summary>
            ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                _exporter.EndGridDataCollection(SheetName);
            }

            #endregion IDisposable Members

            /// <summary>
            ///     Gets or sets the name of the sheet.
            /// </summary>
            /// <value>The name of the sheet.</value>
            public string SheetName { get; set; }
        }
    }

    /// <summary>
    /// Lists available image formats.
    /// </summary>
    public enum ImageFormat
    {
        /// <summary>
        /// </summary>
        Jpeg,

        /// <summary>
        /// </summary>
        Png,

        /// <summary>
        /// </summary>
        Gif
    }
}