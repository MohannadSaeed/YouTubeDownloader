using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using YouTubeDownloader.Models;

namespace YouTubeDownloader.Services
{
    /// <summary>
    /// Provides YouTube data retrieval services including video info, playlist info, and playlist videos.
    /// </summary>
    public class YouTubeService
    {
        private readonly YoutubeClient youtube = new YoutubeClient();

        /// <summary>
        /// Retrieves detailed information about a single YouTube video.
        /// </summary>
        /// <param name="url">The YouTube video URL.</param>
        /// <returns>A <see cref="VideoInfoModel"/> if successful; otherwise, null.</returns>
        public async Task<VideoInfoModel?> GetVideoInfoAsync(string url)
        {
            try
            {
                var video = await youtube.Videos.GetAsync(url);
                return new VideoInfoModel
                {
                    Title = video.Title,
                    Author = video.Author.ChannelTitle,
                    Duration = video.Duration ?? TimeSpan.Zero,
                    UploadDate = video.UploadDate.DateTime,
                    ThumbnailUrl = $"https://img.youtube.com/vi/{video.Id}/hqdefault.jpg"
                };
            }
            catch
            {
                // Return null if the video is unavailable or invalid
                return null;
            }
        }

        /// <summary>
        /// Retrieves information about a YouTube playlist (title, author, video count, and first video thumbnail).
        /// </summary>
        /// <param name="url">The YouTube playlist URL.</param>
        /// <returns>A <see cref="PlaylistInfo"/> model if successful; otherwise, null.</returns>
        public async Task<PlaylistInfo?> GetPlaylistInfoAsync(string url)
        {
            try
            {
                var playlist = await youtube.Playlists.GetAsync(url);
                int count = 0;
                string thumbnail = string.Empty;

                // Iterate through all videos in the playlist to count them and capture the first thumbnail
                await foreach (var video in youtube.Playlists.GetVideosAsync(url))
                {
                    count++;
                    if (string.IsNullOrWhiteSpace(thumbnail))
                    {
                        thumbnail = $"https://img.youtube.com/vi/{video.Id}/hqdefault.jpg";
                    }
                }

                return new PlaylistInfo
                {
                    Title = playlist.Title,
                    Author = playlist.Author.ChannelTitle,
                    VideoCount = count,
                    ThumbnailUrl = thumbnail
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieves all video titles and URLs from a YouTube playlist.
        /// </summary>
        /// <param name="url">The YouTube playlist URL.</param>
        /// <returns>A list of tuples containing each video's title and full watch URL.</returns>
        public async Task<List<(string Title, string Url)>> GetPlaylistVideosAsync(string url)
        {
            var result = new List<(string, string)>();

            await foreach (var video in youtube.Playlists.GetVideosAsync(url))
            {
                result.Add((video.Title, $"https://www.youtube.com/watch?v={video.Id}"));
            }

            return result;
        }
    }
}