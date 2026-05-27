using System;
using System.Collections.Generic;
using System.Text;

namespace YouTubeDownloader.Models
{
    public class AppSettings
    {
        public int Id { get; set; }
        public string VideoFolder { get; set; } = "";
        public string AudioFolder { get; set; } = "";
        public int MaxParallelDownloads { get; set; } = 3;
        public bool AutoStartDownloads { get; set; }
        public bool AutoPasteClipboardUrl { get; set; } = true;
        public bool ShowNotifications { get; set; } = true;
        public bool DeleteFailedDownloads { get; set; }
        public bool MinimizeToTray { get; set; }
        public string Theme { get; set; } = "Dark";
    }
}
