using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace YouTubeDownloader.Controls
{
    /// <summary>
    /// A custom DataGridView column that displays a progress bar and percentage text.
    /// </summary>
    public class DataGridViewProgressColumn : DataGridViewImageColumn
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridViewProgressColumn"/> class.
        /// Sets the cell template to <see cref="DataGridViewProgressCell"/>.
        /// </summary>
        public DataGridViewProgressColumn()
        {
            CellTemplate = new DataGridViewProgressCell();
        }
    }

    /// <summary>
    /// A custom DataGridView cell that renders a progress bar based on an integer value (0-100).
    /// </summary>
    public class DataGridViewProgressCell : DataGridViewImageCell
    {
        // A single-pixel transparent image used to prevent the base class from drawing anything.
        private static readonly Image emptyImage = new Bitmap(1, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridViewProgressCell"/> class.
        /// Sets the expected value type to <see cref="int"/>.
        /// </summary>
        public DataGridViewProgressCell()
        {
            ValueType = typeof(int);
        }

        /// <summary>
        /// Returns an empty image to suppress the default image drawing of the base class.
        /// </summary>
        protected override object GetFormattedValue(
            object value,
            int rowIndex,
            ref DataGridViewCellStyle cellStyle,
            TypeConverter valueTypeConverter,
            TypeConverter formattedValueTypeConverter,
            DataGridViewDataErrorContexts context)
        {
            return emptyImage;
        }

        /// <summary>
        /// Paints the progress bar and percentage text inside the cell.
        /// </summary>
        /// <param name="graphics">The Graphics used to paint the cell.</param>
        /// <param name="clipBounds">The clipping rectangle of the cell.</param>
        /// <param name="cellBounds">The bounds of the cell being painted.</param>
        /// <param name="rowIndex">The row index of the cell.</param>
        /// <param name="cellState">The state of the cell.</param>
        /// <param name="value">The cell's data value.</param>
        /// <param name="formattedValue">The formatted value (ignored).</param>
        /// <param name="errorText">Any error text associated with the cell.</param>
        /// <param name="cellStyle">The style of the cell.</param>
        /// <param name="advancedBorderStyle">The border style of the cell.</param>
        /// <param name="paintParts">Which parts of the cell to paint.</param>
        protected override void Paint(
            Graphics graphics,
            Rectangle clipBounds,
            Rectangle cellBounds,
            int rowIndex,
            DataGridViewElementStates cellState,
            object value,
            object formattedValue,
            string errorText,
            DataGridViewCellStyle cellStyle,
            DataGridViewAdvancedBorderStyle advancedBorderStyle,
            DataGridViewPaintParts paintParts)
        {
            // Draw only the border and background using the base method.
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle,
                DataGridViewPaintParts.Border | DataGridViewPaintParts.Background);

            // Get the progress value (default to 0 if null).
            int progressVal = value != null ? Convert.ToInt32(value) : 0;
            float percentage = progressVal / 100f;

            // Calculate the rectangle for the progress bar (with 5px margins).
            Rectangle progressRect = new Rectangle(
                cellBounds.X + 5,
                cellBounds.Y + 6,
                (int)((cellBounds.Width - 10) * percentage),
                cellBounds.Height - 12);

            // Draw the filled portion of the progress bar.
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(0, 120, 255)))
            {
                graphics.FillRectangle(brush, progressRect);
            }

            // Draw the percentage text centered in the cell.
            string text = $"{progressVal}%";
            TextRenderer.DrawText(graphics, text, new Font("Segoe UI", 9, FontStyle.Bold), cellBounds, Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}