using System;
using System.Collections.Generic;
using System.Text;

namespace YouTubeDownloader.Models
{
    public class DownloadItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Url { get; set; } = "";
        public string Format { get; set; } = "";
        public string Quality { get; set; } = "";
        public string Status { get; set; } = "Queued";
        public int Progress { get; set; }
        public string Speed { get; set; } = "-";
        public string ETA { get; set; } = "-";
        public string Size { get; set; } = "-";
        public bool IsPaused { get; set; }
        public bool IsCancelled { get; set; }
        public string DownloadFolder { get; set; } = "";
    }
}
