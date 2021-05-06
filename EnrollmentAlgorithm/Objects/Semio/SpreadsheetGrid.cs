using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Semio.ClientService.OpenXml.Excel
{
    /// <summary>
    ///     Represents a two-dimensional grid of cells in a spreadsheet.
    /// </summary>
    public class SpreadsheetGrid : IEnumerable<IEnumerable<SpreadsheetCell>>
    {
        private readonly Dictionary<CellLocation, SpreadsheetCell> _cells = new Dictionary<CellLocation, SpreadsheetCell>();
        private int _maxColumn;
        private int _maxRow;

        /// <summary>
        ///     Gets the collection of <see cref="SpreadsheetCell" /> for the specified row number.
        /// </summary>
        /// <value></value>
        public IEnumerable<SpreadsheetCell> this[int row]
        {
            get
            {
                return from col in Enumerable.Range(1, _maxColumn)
                       let location = new CellLocation { Row = row, Column = col }
                       select !_cells.ContainsKey(location) ? SpreadsheetCell.Empty : _cells[location];
            }
        }

        /// <summary>
        ///     Gets the <see cref="SpreadsheetCell" /> for the specified row and column.
        /// </summary>
        /// <value></value>
        public SpreadsheetCell this[int row, string column]
        {
            get
            {
                var location = new CellLocation { Row = row, Column = CellUtilities.ConvertColumnIdToInt(column) };
                return !_cells.ContainsKey(location) ? SpreadsheetCell.Empty : _cells[location];
            }
        }

        /// <summary>
        ///     Adds the cell to the grid.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <returns>
        ///     <c>true</c> if the added cell is unique, <c>false</c> if it replaces an existing cell.
        /// </returns>
        public bool AddCell(SpreadsheetCell cell)
        {
            if (!cell.SpecifiesCell)
                throw new ArgumentException("Cell must specify a row and column location to be added.");

            return AddCell(cell, cell.RowId, cell.ColumnId);
        }

        /// <summary>
        ///     Adds the cell to the grid.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <param name="row">The row number.</param>
        /// <param name="column">The column identifier.</param>
        /// <returns>
        ///     <c>true</c> if the added cell is unique, <c>false</c> if it replaces an existing cell.
        /// </returns>
        public bool AddCell(SpreadsheetCell cell, int row, string column)
        {
            var location = new CellLocation { Row = row, Column = CellUtilities.ConvertColumnIdToInt(column) };
            if (location.Column < 0)
                throw new ArgumentOutOfRangeException("column", "Specified column is invalid");

            bool result = true;
            if (_cells.ContainsKey(location))
            {
                result = false;
                _cells[location] = cell;
            }
            else
            {
                _cells.Add(location, cell);
            }

            if (location.Row > _maxRow)
                _maxRow = location.Row;
            if (location.Column > _maxColumn)
                _maxColumn = location.Column;

            return result;
        }

        #region IEnumerable<IEnumerable<SpreadsheetCell>> Members

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<IEnumerable<SpreadsheetCell>> GetEnumerator()
        {
            for (int row = 1; row <= _maxRow; row++)
            {
                IEnumerable<SpreadsheetCell> list = from col in Enumerable.Range(1, _maxColumn)
                                                    let location = new CellLocation { Row = row, Column = col }
                                                    select !_cells.ContainsKey(location) ? SpreadsheetCell.Empty : _cells[location];
                yield return list;
            }
        }

        #endregion IEnumerable<IEnumerable<SpreadsheetCell>> Members

        #region IEnumerable Members

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerable Members
    }
}