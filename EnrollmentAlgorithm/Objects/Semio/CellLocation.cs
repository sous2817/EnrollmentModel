namespace Semio.ClientService.OpenXml.Excel
{
    /// <summary>
    ///     Represents a location of a spreadsheet cell.
    /// </summary>
    public struct CellLocation
    {
        /// <summary>
        ///     Gets or sets the row number.
        /// </summary>
        /// <value>The row.</value>
        public int Row { get; set; }

        /// <summary>
        ///     Gets or sets the column index.
        /// </summary>
        /// <value>The column.</value>
        public int Column { get; set; }

        /// <summary>
        ///     Indicates whether this instance and a specified CellLocation are equal.
        /// </summary>
        /// <param name="other">Another CellLocation to compare to.</param>
        /// <returns>
        ///     true if <paramref name="other" /> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        public bool Equals(CellLocation other)
        {
            return other.Row == Row && other.Column == Column;
        }

        /// <summary>
        ///     Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        ///     true if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(CellLocation)) return false;
            return Equals((CellLocation)obj);
        }

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Row * 397) ^ Column;
            }
        }

        /// <summary>
        ///     Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(CellLocation left, CellLocation right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(CellLocation left, CellLocation right)
        {
            return !left.Equals(right);
        }
    }
}