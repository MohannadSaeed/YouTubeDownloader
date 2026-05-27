using System;
using System.Collections.Generic;
using System.Text;

namespace YouTubeDownloader.Models
{
    public class VideoInfoModel
    {
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string ThumbnailUrl { get; set; } = "";
        public TimeSpan Duration { get; set; }
        public DateTime? UploadDate { get; set; }
    }
}
