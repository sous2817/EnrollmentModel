using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Semio.ClientService.OpenXml.Excel
{
    /// <summary>
    ///     Contains utility functions related to spreadsheet cells.
    /// </summary>
    public static class CellUtilities
    {
        /// <summary>
        ///     Determines whether the specified column identifier is valid.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns>
        ///     <c>true</c> if the column identifier is valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsColumnIdentifierValid(string column)
        {
            if (String.IsNullOrEmpty(column))
                return false;

            string uppered = column.ToUpper();
            foreach (char character in uppered)
            {
                if (character < 'A' || character > 'Z')
                    return false;
            }
            return true;
        }

        /// <summary>
        ///     Singles the character column value.
        /// </summary>
        /// <param name="letter">The letter.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException" />
        public static int SingleCharacterColumnValue(char letter)
        {
            if (letter < 'A' || letter > 'Z')
                throw new ArgumentOutOfRangeException("letter", "Column characters must be in the range A-Z");
            return (letter - 'A') + 1;
        }

        /// <summary>
        ///     Converts the column id to int.
        /// </summary>
        /// <param name="columnId">The column id.</param>
        /// <returns>The appropriate column number or -1 if invalid.</returns>
        public static int ConvertColumnIdToInt(string columnId)
        {
            if (!IsColumnIdentifierValid(columnId))
                return -1;

            string uppered = columnId.ToUpper();
            if (uppered.Length == 1)
            {
                return SingleCharacterColumnValue(uppered[0]);
            }

            int result = 0;
            for (int i = 0; i < uppered.Length; i++)
            {
                var placeMultiplier = (int)Math.Pow(26, (uppered.Length - i - 1));
                result += SingleCharacterColumnValue(uppered[i]) * placeMultiplier;
            }
            return result;
        }

        /// <summary>
        ///     Converts the number to column id.
        /// </summary>
        /// <param name="columnNumber">The column number.</param>
        /// <returns></returns>
        public static string ConvertIntToColumnId(int columnNumber)
        {
            if (columnNumber < 1)
                return "A";

            string columnId = String.Empty;
            int remainingValue = columnNumber;
            while (remainingValue > 0)
            {
                int index = remainingValue - 1;
                var placeChar = (char)('A' + (index % 26));
                columnId = placeChar + columnId;
                string message = String.Format("Converting {0} to column value failed with {1}", columnNumber, columnId);

                //if (!IsColumnIdentifierValid(columnId))
                //    throw new InvalidOperationException(message);
                Debug.Assert(IsColumnIdentifierValid(columnId), message);
                remainingValue = index / 26;
            }
            return columnId;
        }

        public static IEnumerable<List<string>> ReadCellValuesFromExcelFile(string fileName)
        {
            using (SpreadsheetDocument myDoc = SpreadsheetDocument.Open(fileName, false))
            {
                WorkbookPart workbookPart = myDoc.WorkbookPart;
                WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
                OpenXmlReader reader = OpenXmlReader.Create(worksheetPart);
                Worksheet workSheet = worksheetPart.Worksheet;
                SheetData sheetData = workSheet.GetFirstChild<SheetData>();
                if (!sheetData.Elements<Row>().Any())
                    throw new Exception("Empty File!");
                Row[] rows = sheetData.Elements<Row>().ToArray();

                //string[] columnNames = rows.First()
                //              .Elements<Cell>()
                //              .Select(cell => GetCellValue(cell, myDoc))
                //              .ToArray();
                var notNullcellHeaders = rows.First().Elements<Cell>().Select(cell => cell.CellReference.Value);
                var firstNotNullHeader = GetHeaderLettersExcludeNumber(notNullcellHeaders.First());
                var lastNotNullHeader = GetHeaderLettersExcludeNumber(notNullcellHeaders.Last());
                var allHeaderLetters = GetAllHeaderLetters(200);
                var currentHeaderLetters = GetCurrentHeaders(allHeaderLetters, firstNotNullHeader, lastNotNullHeader);
                //if (columnNames.Count() != headerLetters.Count())
                //{
                //    throw new ArgumentException("HeaderLetters and Column names dont match");
                //}
                //This cell values collection is based on the cell index the column names can be retrieved.
                IEnumerable<List<string>> cellValues = GetCellValues(currentHeaderLetters.ToArray(), rows, myDoc);
                return cellValues;
            }
        }

        private static IEnumerable<List<string>> GetCellValues(string[] headers, IEnumerable<Row> rows, SpreadsheetDocument document)
        {
            var result = new List<List<string>>();
            int columnCount = headers.Count();
            foreach (var row in rows)
            {
                List<string> cellValues = new List<string>();
                var actualCells = row.Elements<Cell>().ToArray();

                int j = 0;
                for (int i = 0; i < columnCount; i++)
                {
                    if (actualCells.Count() <= j)
                    {
                        cellValues.Add(null);
                        continue;
                    }
                    string actualCellRef = actualCells[j].CellReference.ToString();
                    string actualCellHeader = GetHeaderLettersExcludeNumber(actualCellRef);
                    string header = headers[i];

                    //if the cell has a value but there is no header for it, move until the header and cell match.
                    while (!headers.Contains(actualCellHeader))
                    {
                        j++;
                        actualCellRef = actualCells[j].CellReference.ToString();
                        actualCellHeader = GetHeaderLettersExcludeNumber(actualCellRef);
                    }
                    //if the header is present but the cell doesnt have the value
                    if (actualCellHeader != header)
                    {
                        cellValues.Add(null);
                        continue;
                    }
                    //Read cell value.
                    cellValues.Add(GetCellValue(actualCells[j], document));
                    j++;
                }
                //If all the values of the columns are empty, dont read any further.
                if (cellValues.Any(c => !string.IsNullOrEmpty(c) && c.ToLower().Contains("edit") && c.ToLower().Contains("printable")))
                    continue;
                result.Add(cellValues);
            }
            return result;
        }

        private static string GetCellValue(Cell cell, SpreadsheetDocument document)
        {
            bool sstIndexedcell = GetCellType(cell);
            return sstIndexedcell
                ? GetSharedStringItemById(document.WorkbookPart, Convert.ToInt32(cell.InnerText))
                : cell.InnerText;
        }

        private static bool GetCellType(Cell cell)
        {
            return cell.DataType != null && cell.DataType == CellValues.SharedString;
        }

        private static string GetSharedStringItemById(WorkbookPart workbookPart, int id)
        {
            return
                workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>()
                    .ElementAt(id)
                    .InnerText;
        }

        public static string GetHeaderLettersExcludeNumber(string columnHeader)
        {
            var str = columnHeader.ToCharArray();
            string strOutput = str.Where(c1 => !isNumeric(c1.ToString()))
                    .Aggregate(string.Empty, (current, c1) => current + c1);
            return strOutput;
        }

        private static bool isNumeric(string val)
        {
            if (val == String.Empty)
                return false;
            int result;
            return int.TryParse(val, out result);
        }

        public static string[] GetAllHeaderLetters(uint max)
        {
            var result = new List<string>();
            int i = 0;
            var columnPrefix = new Queue<string>();
            string prefix = null;
            int prevRoundNo = 0;
            uint maxPrefix = max / 26;

            while (i < max)
            {
                int roundNo = i / 26;
                if (prevRoundNo < roundNo)
                {
                    prefix = columnPrefix.Dequeue();
                    prevRoundNo = roundNo;
                }
                string item = prefix + ((char)(65 + (i % 26))).ToString(CultureInfo.InvariantCulture);
                if (i <= maxPrefix)
                {
                    columnPrefix.Enqueue(item);
                }
                result.Add(item);
                i++;
            }
            return result.ToArray();
        }

        private static List<string> GetCurrentHeaders(IEnumerable<string> array, string first, string last)
        {
            List<string> output = new List<string>();
            bool startAdding = false;
            foreach (string s in array)
            {
                if (s == first)
                    startAdding = true;
                if (startAdding)
                    output.Add(s);
                if (s == last)
                    break;
            }
            return output;
        }
    }
}