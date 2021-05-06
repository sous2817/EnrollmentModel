using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;

namespace Semio.ClientService.OpenXml.Excel
{
    public interface IExcelExporter : IDisposable
    {
        /// <summary>
        ///     Provides a scope for collecting grid data.
        /// </summary>
        /// <param name="sheetName">Name of the sheet to collect data for.</param>
        /// <returns></returns>
        IDisposable DataCollectionScope(string sheetName);

        /// <summary>
        ///     Begins the document and initiates writing it to the specified path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        void BeginDocument(string filePath);

        /// <summary>
        ///     Begins the document and initiates writing it to the specified path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="defaultWidth">The default column width.</param>
        void BeginDocument(string filePath, double defaultWidth);

        /// <summary>
        ///     Ends the document and completes saving it to disk.
        /// </summary>
        void EndDocument();

        /// <summary>
        ///     Begins the grid data collection.
        /// </summary>
        /// <param name="sheetName">Name of the sheet to collect data for.</param>
        void BeginGridDataCollection(string sheetName);

        /// <summary>
        ///     Adds the cell to the grid data.
        /// </summary>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <param name="cell">The cell to add.</param>
        void AddCell(string sheetName, SpreadsheetCell cell);

        /// <summary>
        ///     Ends the grid data collection.
        /// </summary>
        /// <param name="sheetName">Name of the sheet to collect data for.</param>
        void EndGridDataCollection(string sheetName);

        /// <summary>
        ///     Adds a new sheet to the workbook.
        /// </summary>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <returns></returns>
        bool AddSheet(string sheetName);

        /// <summary>
        ///     Adds a new sheet to the workbook.
        /// </summary>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <param name="columns">The columns.</param>
        /// <returns></returns>
        bool AddSheet(string sheetName, Columns columns);

        /// <summary>
        ///     Adds a new sheet to the workbook.
        /// </summary>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="usesConditionalFormatting">
        ///     if set to <c>true</c> uses conditional formatting.
        /// </param>
        /// <returns></returns>
        bool AddSheet(string sheetName, Columns columns, bool usesConditionalFormatting);

        /// <summary>
        ///     Add a ConditionalFormattingRule to the worksheet
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="cfRule"></param>
        /// <param name="start"></param>
        void AddConditionalFormatting(string sheetName, ConditionalFormattingRule cfRule, SpreadsheetCell start);

        /// <summary>
        ///     Add a ConditionalFormattingRule to the worksheet
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="cfRule"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        void AddConditionalFormatting(string sheetName, ConditionalFormattingRule cfRule, SpreadsheetCell start, SpreadsheetCell end);

        /// <summary>
        ///     Adds the image to the specified sheet.
        /// </summary>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <param name="image">The image.</param>
        /// <param name="xOffset">The x offset.</param>
        /// <param name="yOffset">The y offset.</param>
        void AddImage(string sheetName, ExportImage image, int xOffset, int yOffset);

        /// <summary>
        ///     Outputs a line chart based on the provided data, one series per column (minus the
        ///     first column, which is the X-Axis). The chart will output at the top of the sheet.
        /// </summary>
        /// <param name="sheetName">The sheet to which the data and chart will output.</param>
        /// <param name="allDataWithHeader">The data used to generate the chart. The first row is the header. The first column is the X-Axis.</param>
        /// <param name="chartTitle">Title of the chart.</param>
        int AddChartAndData(string sheetName, IEnumerable<IEnumerable<SpreadsheetCell>> allDataWithHeader, string chartTitle);

        /// <summary>
        ///     Outputs a line chart based on the provided data, one series per column (minus the
        ///     first column, which is the X-Axis). The chart will output at the top of the sheet.
        /// </summary>
        /// <param name="sheetName">The sheet to which the data and chart will output.</param>
        /// <param name="allDataWithHeader">The data used to generate the chart. The first row is the header. The first column is the X-Axis.</param>
        /// <param name="chartTitle">Title of the chart.</param>
        /// <param name="startingRow">The Row to place the Chart on</param>
        int AddChartAndData(string sheetName, IEnumerable<IEnumerable<SpreadsheetCell>> allDataWithHeader, string chartTitle, int startingRow);

        /// <summary>
        ///     Outputs a line chart based on the provided data, one series per column (minus the
        ///     first column, which is the X-Axis). The chart will output at the top of the sheet.
        /// </summary>
        /// <param name="sheetName">The sheet to which the data and chart will output.</param>
        /// <param name="allDataWithHeader">The data used to generate the chart. The first row is the header. The first column is the X-Axis.</param>
        /// <param name="chartTitle">Title of the chart.</param>
        /// <param name="startingRow">The Row to place the Chart on</param>
        /// <param name="chartWidthInColumns">How wide the chart should be</param>
        /// <param name="xAxisLabel">Text to be displayed below the chart's X-Axis</param>
        /// <param name="yAxisLabel">Text to be displayed beside the chart's Y-Axis</param>
        int AddChartAndData(string sheetName, IEnumerable<IEnumerable<SpreadsheetCell>> allDataWithHeader, string chartTitle, int startingRow, int chartWidthInColumns, string xAxisLabel, string yAxisLabel);

        int AddTierScatterPlotChart(string sheetName, IEnumerable<SpreadsheetCell> facilityCells, IEnumerable<SpreadsheetCell> performanceCells, IEnumerable<IEnumerable<SpreadsheetCell>> tierCells, string chartTitle, int chartStartingRow, int chartHeightInRows, int chartWidthInColumns, int dataStartingRow, int dataEndingRow, int facilityDataColumn, int performanceDataColumn);

        /// <summary>
        ///     Skips the rows corresponding to the specified height in pixels.
        /// </summary>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <param name="pixelHeight">Height in pixels.</param>
        void SkipRows(string sheetName, int pixelHeight);

        /// <summary>
        ///     Adds a new row of data to the sheet.
        /// </summary>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <param name="items">The items to add as cells.</param>
        void AddRow(string sheetName, IEnumerable<SpreadsheetCell> items);

        /// <summary>
        ///     Adds the row dynamically assigning CellValue based on item
        /// </summary>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <param name="items">The items.</param>
        void AddRowDynamic(string sheetName, IEnumerable<string> items);

        /// <summary>
        ///     Adds the row.
        /// </summary>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <param name="items">The items.</param>
        void AddRow(string sheetName, IEnumerable<string> items);

        /// <summary>
        /// Checks for presence of a worksheet by name
        /// </summary>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <returns></returns>
        bool HasWorksheet(string sheetName);

        uint GetIndentLevelStyle(int indentLevel);
    }
}