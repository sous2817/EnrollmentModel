namespace Semio.ClientService.OpenXml.Excel
{
    /// <summary>
    ///     Template for CellFormula object
    /// </summary>
    public class SpreadsheetCellFormula
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="text"></param>
        public SpreadsheetCellFormula(string text)
        {
            Text = text;
        }

        /// <summary>
        ///     String formula
        /// </summary>
        public string Text { get; set; }
    }
}