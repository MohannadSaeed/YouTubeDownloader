using FontAwesome.Sharp;

namespace YouTubeDownloader.Forms
{
    public partial class MainForm
    {
        #region Helpers

        /// <summary>
        /// Returns the default download folder path based on format (Video or Audio).
        /// </summary>
        private string GetDefaultDownloadFolder(string format)
        {
            string downloads = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string baseFolder = Path.Combine(downloads, "Downloads", "YouTube Downloader");
            string finalFolder = format.Contains("MP3") ? Path.Combine(baseFolder, "Audio") : Path.Combine(baseFolder, "Video");
            if (!Directory.Exists(finalFolder)) Directory.CreateDirectory(finalFolder);
            return finalFolder;
        }


        private IconButton CreateIconButton(string text, IconChar icon, int width, int height)
        {
            IconButton button = new()
            {
                Text = text,
                IconChar = icon,
                IconColor = Color.White,
                IconSize = 22,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(10, 0, 10, 0),
                Width = width,
                Height = height,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 120, 255),
                FlatStyle = FlatStyle.Flat
            };
            button.FlatAppearance.BorderSize = 0;
            button.MouseEnter += (s, e) =>
            {
                button.BackColor = Color.FromArgb(0, 100, 220);
            };
            button.MouseLeave += (s, e) =>
            {
                button.BackColor = Color.FromArgb(0, 120, 255);
            };
            return button;
        }

        // NOTE: Add any other general-purpose helper methods here.

        #endregion
    }
}