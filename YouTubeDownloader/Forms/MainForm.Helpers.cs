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

        // NOTE: Add any other general-purpose helper methods here.

        #endregion
    }
}