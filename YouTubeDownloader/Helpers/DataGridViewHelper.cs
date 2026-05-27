using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace YouTubeDownloader.Helpers
{
    public static class DataGridViewHelper
    {
        public static void EnableDoubleBuffering(DataGridView dgv)
        {
            typeof(DataGridView).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic) ?.SetValue(dgv, true, null);
        }
    }
}
