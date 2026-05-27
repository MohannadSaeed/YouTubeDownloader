using System;
using System.Windows.Forms;
using YouTubeDownloader.Models;

namespace YouTubeDownloader.Forms
{
    public partial class MainForm
    {
        /// <summary>
        /// Initializes the timer that periodically checks the clipboard for YouTube URLs.
        /// </summary>
        private void InitializeClipboardMonitor()
        {
            clipboardTimer = new System.Windows.Forms.Timer
            {
                Interval = 1500 // Check every 1.5 seconds
            };
            clipboardTimer.Tick += ClipboardTimer_Tick;
            clipboardTimer.Start();
        }

        /// <summary>
        /// Handles the timer tick event: reads the clipboard and auto‑fills the URL textbox
        /// if a YouTube link is detected and auto‑paste is enabled in settings.
        /// </summary>
        private void ClipboardTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                // Get current settings
                AppSettings settings = settingsService.GetSettings();
                if (!settings.AutoPasteClipboardUrl)
                    return;

                // Exit if clipboard doesn't contain text
                if (!Clipboard.ContainsText())
                    return;

                string text = Clipboard.GetText().Trim();
                if (string.IsNullOrWhiteSpace(text))
                    return;

                // Avoid processing the same URL repeatedly
                if (text == lastClipboardText)
                    return;

                // Check if the clipboard text is a YouTube URL
                bool isYouTube = text.Contains("youtube.com") || text.Contains("youtu.be");
                if (!isYouTube)
                    return;

                // Remember this URL to prevent duplicate processing
                lastClipboardText = text;

                // Only auto‑paste if the URL textbox has focus
                if (!txtUrl.Focused)
                    return;

                // Update the textbox only if the content is different
                if (!txtUrl.Text.Equals(text))
                {
                    txtUrl.Text = text;
                }
            }
            catch
            {
                // Silently ignore clipboard access errors (e.g., another application has clipboard open)
            }
        }
    }
}