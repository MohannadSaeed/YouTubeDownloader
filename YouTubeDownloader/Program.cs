using FFMpegCore;
using Microsoft.EntityFrameworkCore;
using YouTubeDownloader.Data;

namespace YouTubeDownloader
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            //C:\Users\za3tr\AppData\Local\Microsoft\WinGet\Packages\Gyan.FFmpeg_Microsoft.Winget.Source_8wekyb3d8bbwe\ffmpeg-8.1.1-full_build\bin
            GlobalFFOptions.Configure(options =>
            {
                options.BinaryFolder = Path.Combine(AppContext.BaseDirectory, "Tools");
            });
            using AppDbContext db = new();
            db.Database.Migrate();
            Application.Run(new Forms.MainForm());
        }
    }
}