using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FFMpegCore;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace YouTubeDownloader.Services
{
    /// <summary>
    /// Provides download functionality for YouTube videos and audio streams.
    /// </summary>
    public class DownloadService
    {
        private readonly YoutubeClient _youtube;

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadService"/> class.
        /// </summary>
        public DownloadService()
        {
            _youtube = new YoutubeClient();
        }

        /// <summary>
        /// Downloads a video (muxed stream) from YouTube and saves it to the specified path.
        /// </summary>
        /// <param name="url">The YouTube video URL.</param>
        /// <param name="outputPath">The full path where the video will be saved.</param>
        /// <param name="progress">Progress reporter (0-100).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <param name="totalBytesCallback">Optional callback with the total file size in bytes.</param>
        /// <exception cref="Exception">Thrown when no suitable muxed stream is found.</exception>
        public async Task DownloadVideoAsync(string url, string outputPath, IProgress<double> progress, CancellationToken cancellationToken, Action<long>? totalBytesCallback = null)
        {
            // Get stream manifest for the video
            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(url);

            // Select the best quality muxed stream (video + audio)
            var streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();
            if (streamInfo == null)
                throw new Exception("No suitable stream found.");

            // Report total bytes if callback is provided
            totalBytesCallback?.Invoke(streamInfo.Size.Bytes);

            // Download the stream with progress reporting (scale to 0-100)
            await _youtube.Videos.Streams.DownloadAsync(streamInfo, outputPath,
                cancellationToken: cancellationToken,
                progress: new Progress<double>(p => progress.Report(p * 100)));
        }

        /// <summary>
        /// Downloads audio only from YouTube, converts it to MP3 using FFmpeg, and saves it to the specified path.
        /// </summary>
        /// <param name="url">The YouTube video URL.</param>
        /// <param name="outputPath">The full path where the MP3 file will be saved.</param>
        /// <param name="progress">Progress reporter (0-100).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <param name="totalBytesCallback">Optional callback with the total audio stream size in bytes.</param>
        /// <exception cref="Exception">Thrown when no audio stream is found or conversion fails.</exception>
        public async Task DownloadAudioAsync(string url, string outputPath, IProgress<double> progress, CancellationToken cancellationToken, Action<long>? totalBytesCallback = null)
        {
            // Get stream manifest
            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(url);

            // Select the highest bitrate audio-only stream
            var audioStreamInfo = streamManifest.GetAudioOnlyStreams()
                .OrderByDescending(s => s.Bitrate)
                .FirstOrDefault();
            if (audioStreamInfo == null)
                throw new Exception("No audio stream found.");

            // Report total bytes if callback is provided
            totalBytesCallback?.Invoke(audioStreamInfo.Size.Bytes);

            // Download audio to a temporary file (using original container format, e.g., webm)
            string tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.{audioStreamInfo.Container.Name}");
            await _youtube.Videos.Streams.DownloadAsync(audioStreamInfo, tempFile,
                cancellationToken: cancellationToken,
                progress: new Progress<double>(p => progress.Report(p * 80))); // 0-80% of total progress

            progress.Report(85); // Download complete, starting conversion

            if (!File.Exists(tempFile))
                throw new Exception("Temp audio file not found.");

            // Convert the temporary audio file to MP3 using FFmpeg
            if (File.Exists(outputPath))
                File.Delete(outputPath);

            await FFMpegArguments.FromFileInput(tempFile)
                .OutputToFile(outputPath, true, options => options
                    .WithAudioCodec("libmp3lame")
                    .WithAudioBitrate(320)
                    .ForceFormat("mp3"))
                .ProcessAsynchronously();

            progress.Report(100); // Conversion complete

            if (!File.Exists(outputPath))
                throw new Exception("MP3 conversion failed.");

            // Clean up temporary file
            File.Delete(tempFile);
        }
    }
}