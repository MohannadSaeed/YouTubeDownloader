using System;
using System.Collections.Generic;
using System.Text;
using YouTubeDownloader.Data;
using YouTubeDownloader.Models;

namespace YouTubeDownloader.Services
{
    public class SettingsService
    {
        private readonly AppDbContext _db;

        public SettingsService()
        {
            _db = new AppDbContext();
        }

        public AppSettings GetSettings()
        {
            AppSettings? settings =
                _db.Settings.FirstOrDefault();

            if (settings != null)
            {
                return settings;
            }

            settings = CreateDefaultSettings();

            _db.Settings.Add(settings);

            _db.SaveChanges();

            return settings;
        }

        public void Save(AppSettings settings)
        {
            _db.Settings.Update(settings);

            _db.SaveChanges();
        }

        private AppSettings CreateDefaultSettings()
        {
            string baseFolder =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.UserProfile),
                    "Downloads",
                    "YouTube Downloader");

            return new AppSettings
            {
                VideoFolder =
                    Path.Combine(baseFolder, "Video"),

                AudioFolder =
                    Path.Combine(baseFolder, "Audio")
            };
        }
    }
}
