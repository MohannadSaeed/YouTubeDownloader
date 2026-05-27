using System;
using System.Collections.Generic;
using System.Text;

namespace YouTubeDownloader.Models
{
    public class PlaylistInfo
    {
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public int VideoCount { get; set; }
        public string ThumbnailUrl { get; set; } = "";
    }
}
