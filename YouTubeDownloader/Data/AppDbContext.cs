using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using YouTubeDownloader.Models;

namespace YouTubeDownloader.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<AppSettings> Settings { get; set; }
        public DbSet<DownloadHistory> DownloadHistories { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=ytdownloader.db");
        }
    }
}
