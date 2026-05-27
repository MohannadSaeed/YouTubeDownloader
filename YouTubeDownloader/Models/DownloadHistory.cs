namespace YouTubeDownloader.Models
{
    public class DownloadHistory
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Url { get; set; } = "";
        public string Format { get; set; } = "";
        public string FilePath { get; set; } = "";
        public DateTime DownloadedAt { get; set; }
    }
}
