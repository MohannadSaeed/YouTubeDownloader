namespace YouTubeDownloader.Forms
{
    public partial class MainForm
    {
        #region Title Bar Dragging Events

        private void TitleBar_MouseDown(object? sender, MouseEventArgs e)
        {
            isDragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = Location;
        }

        private void TitleBar_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!isDragging) return;
            Point difference = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
            Location = Point.Add(dragFormPoint, new Size(difference));
        }

        private void TitleBar_MouseUp(object? sender, MouseEventArgs e) => isDragging = false;

        #endregion
    }
}