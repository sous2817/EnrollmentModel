using DocumentFormat.OpenXml.Spreadsheet;
using System;

namespace Semio.ClientService.OpenXml.Excel
{
    /// <summary>
    ///     Provides a container for data and settings of a cell in a spreadsheet
    /// </summary>
    public class SpreadsheetCell
    {
        /// <summary>
        ///     Style code for a cell with borders on all sides.
        /// </summary>
        public const uint AllBorderStyle = 1U;

        /// <summary>
        ///     Style code for a cell with bold font.
        /// </summary>
        public const uint BoldStyle = 3U;

        /// <summary>
        ///     Style code for a cell with borders on all sides and centered content.
        /// </summary>
        public const uint CenteredBorderStyle = 2U;

        /// <summary>
        ///     Style code for a cell with borders on all sides and centered content with a bold font.
        /// </summary>
        public const uint CenteredBorderBoldStyle = 4U;

        /// <summary>
        ///     Style code for a cell with a border on top, left, right.
        /// </summary>
        public const uint TopLeftRightBorderStyle = 5U;

        /// <summary>
        ///     Style code for a cell with a border on top, left, right with bold font.
        /// </summary>
        public const uint TopLeftRightBorderBoldStyle = 6U;

        /// <summary>
        ///     Style code for a cell with a border on left, right.
        /// </summary>
        public const uint LeftRightBorderStyle = 7U;

        /// <summary>
        ///     Style code for a cell with a border on left, right with bold font.
        /// </summary>
        public const uint LeftRightBorderBoldStyle = 8U;

        /// <summary>
        ///     Style code for a cell with a border on left, right, bottom.
        /// </summary>
        public const uint LeftRightBottomBorderStyle = 9U;

        /// <summary>
        ///     Style code for a cell with a border on left, right, bottom with bold font.
        /// </summary>
        public const uint LeftRightBottomBorderBoldStyle = 10U;

        /// <summary>
        ///     Style code for a cell with a border on left, right, bottom with bold font.
        /// </summary>
        public const uint CenteredWrapStyle = 11U;

        /// <summary>
        ///     Style code for a cell with a border on left, right, bottom with bold font.
        /// </summary>
        public const uint LabelForCenteredWrapStyle = 12U;

        public const uint WrapStyle = 12U;

        /// <summary>
        ///     An empty cell with no content or borders.
        /// </summary>
        public static readonly SpreadsheetCell Empty = new SpreadsheetCell(CellValues.Error, null);

        /// <summary>
        ///     A cell with no content and all borders set on.
        /// </summary>
        public static readonly SpreadsheetCell BorderOnly = new SpreadsheetCell(CellValues.String, String.Empty) { StyleId = AllBorderStyle };

        private string _columnId = String.Empty;
        private int _rowId = -1;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SpreadsheetCell" /> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="data">The data.</param>
        public SpreadsheetCell(CellValues type, string data)
        {
            Type = type;
            Data = data;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SpreadsheetCell" /> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="data">The data.</param>
        /// <param name="rowId">The row id.</param>
        /// <param name="columnId">The column id.</param>
        public SpreadsheetCell(CellValues type, string data, int rowId, string columnId)
            : this(type, data)
        {
            SetRow(rowId);
            SetColumn(columnId);
        }

        /// <summary>
        ///     Formula for cell (optional)
        /// </summary>
        public SpreadsheetCellFormula CellFormula { get; set; }

        /// <summary>
        ///     Gets or sets the type of data in the cell.
        /// </summary>
        /// <value>The type.</value>
        public CellValues Type { get; set; }

        /// <summary>
        ///     Gets or sets the string representation of the cell data.
        /// </summary>
        /// <value>The data.</value>
        public string Data { get; set; }

        /// <summary>
        ///     Gets or sets the style id.
        /// </summary>
        /// <value>The style id.</value>
        public uint? StyleId { get; set; }

        /// <summary>
        ///     Gets the row id.
        /// </summary>
        /// <value>The row id.</value>
        public int RowId
        {
            get { return _rowId; }
        }

        /// <summary>
        ///     Gets the column id.
        /// </summary>
        /// <value>The column id.</value>
        public string ColumnId
        {
            get { return _columnId; }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance specifies a cell location.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance specifies a cell; otherwise, <c>false</c>.
        /// </value>
        public bool SpecifiesCell
        {
            get { return RowId > 0 && ColumnId.Length > 0; }
        }

        /// <summary>
        ///     Reference to cell in standard relative Excel format (ex: "B6")
        /// </summary>
        /// <remarks>
        ///     This is a relative reference notation, allowing Excel to change
        ///     the cell reference of copied cells to refer to new cells relative
        ///     to itself.
        /// </remarks>
        public string CellReference
        {
            get { return String.Format("{0}{1}", ColumnId, RowId); }
        }

        /// <summary>
        ///     Reference to cell in standard fixed location Excel format (ex: "$B$6")
        /// </summary>
        /// <remarks>
        ///     This notation prevents excel from ever changing this reference in
        ///     an equation if the referring cell is moved.
        /// </remarks>
        public string CellReferenceFixed
        {
            get { return String.Format("${0}${1}", ColumnId, RowId); }
        }

        /// <summary>
        ///     Gets the delimited cell identifier. (ex: "B:6")
        /// </summary>
        /// <value>The cell.</value>
        public string Cell
        {
            get { return String.Format("{0}:{1}", ColumnId, RowId); }
        }

		/// <summary>
		///		Indicates that any leading/training spacing should be preserved
		/// </summary>
		public bool PreserveSpacing { get; set; }

		/// <summary>
		///     Sets the delimited cell identifier. (ex: "B:6")
		/// </summary>
		/// <param name="cell">The cell.</param>
		/// <exception cref="ArgumentOutOfRangeException" />
		public void SetCell(string cell)
        {
            string[] split = cell.Split(':');
            if (split.Length != 2)
                throw new ArgumentOutOfRangeException("cell", "Invalid cell format. Cell location should be specified in the form A:1");

            int parsedRow;
            if (!Int32.TryParse(split[1], out parsedRow))
                throw new ArgumentOutOfRangeException("cell", "Invalid cell format. Cell location should be specified in the form A:1");

            SetRow(parsedRow);
            SetColumn(split[0]);
        }

        /// <summary>
        ///     Sets the row. (ex: 6)
        /// </summary>
        /// <param name="rowNumber">The row number.</param>
        /// <exception cref="ArgumentOutOfRangeException" />
        public void SetRow(int rowNumber)
        {
            if (rowNumber < 1)
                throw new ArgumentOutOfRangeException("rowNumber", "Row number must be greater than or equal to 1.");

            _rowId = rowNumber;
        }

        /// <summary>
        ///     Sets the column. (ex: "B")
        /// </summary>
        /// <param name="column">The column's letter designation.</param>
        /// <exception cref="ArgumentOutOfRangeException" />
        public void SetColumn(string column)
        {
            if (!CellUtilities.IsColumnIdentifierValid(column))
                throw new ArgumentOutOfRangeException("column", "Columns must be specified as a letter or multiple letters: A or AH.");

            _columnId = column.ToUpper();
        }

        /// <summary>
        ///     Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </returns>
        public override string ToString()
        {
            return String.Format("{0} {1}", CellReference, Data);
        }
    }
}