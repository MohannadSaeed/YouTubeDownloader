using YouTubeDownloader.Models;

namespace YouTubeDownloader.Forms
{
    public partial class MainForm
    {
        #region Download Engine

        /// <summary>
        /// Handles the Start Downloads button click: starts all queued downloads in parallel (up to 3 at a time).
        /// </summary>
        private async void BtnStartDownloads_Click(object? sender, EventArgs e)
        {
            List<Task> tasks = new();
            foreach (var item in queueItems)
            {
                if (item.Status == "Completed" || item.Status == "Downloading") continue;
                item.DownloadFolder = GetDownloadFolder(item.Format);
                tasks.Add(QueueDownloadAsync(item));
            }
            await Task.WhenAll(tasks);
            MessageBox.Show("Downloads completed.", "YouTube Downloader", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Queues a download respecting the concurrency limit (3 simultaneous downloads).
        /// </summary>
        private async Task QueueDownloadAsync(DownloadItem item)
        {
            await downloadSemaphore.WaitAsync();
            try
            {
                await StartDownloadAsync(item);
            }
            finally
            {
                downloadSemaphore.Release();
            }
        }

        /// <summary>
        /// Starts an individual download, reports progress, and saves to history on completion.
        /// </summary>
        private async Task StartDownloadAsync(DownloadItem item)
        {
            CancellationTokenSource cts = new();
            cancellationTokens[item.Id] = cts;

            try
            {
                item.Status = "Downloading";
                queueBindingSource.ResetBindings(false);
                StyleGridButtons();

                string extension = item.Format.Contains("MP3") ? ".mp3" : ".mp4";
                string safeTitle = string.Join("_", item.Title.Split(Path.GetInvalidFileNameChars()));
                string filePath = Path.Combine(item.DownloadFolder, $"{safeTitle}{extension}");

                Progress<double> progress = new(value =>
                {
                    if (item.IsCancelled) { cts.Cancel(); return; }
                    if (item.IsPaused) return;
                    int newProgress = (int)value;
                    if (newProgress < item.Progress) return;
                    item.Progress = newProgress;
                    BeginInvoke(() => queueBindingSource.ResetBindings(false));
                });

                if (item.Format.Contains("MP3"))
                    await downloadService.DownloadAudioAsync(item.Url, filePath, progress, cts.Token);
                else
                    await downloadService.DownloadVideoAsync(item.Url, filePath, progress, cts.Token);

                if (!item.IsCancelled)
                {
                    item.Progress = 100;
                    db.DownloadHistories.Add(new DownloadHistory
                    {
                        Title = item.Title,
                        Url = item.Url,
                        Format = item.Format,
                        FilePath = filePath,
                        DownloadedAt = DateTime.Now
                    });
                    db.SaveChanges();
                    item.Status = "Completed";
                }

                BeginInvoke(() => queueBindingSource.ResetBindings(false));
            }
            catch (OperationCanceledException)
            {
                if (item.IsCancelled) item.Status = "Cancelled";
                BeginInvoke(() => queueBindingSource.ResetBindings(false));
            }
            catch (Exception ex)
            {
                item.Status = "Failed";
                MessageBox.Show(ex.Message);
                BeginInvoke(() => queueBindingSource.ResetBindings(false));
            }
        }

        /// <summary>
        /// Toggles the pause/resume state of a download item.
        /// </summary>
        private void TogglePause(DownloadItem item)
        {
            item.IsPaused = !item.IsPaused;
            item.Status = item.IsPaused ? "Paused" : "Downloading";
            queueBindingSource.ResetBindings(false);
            dgvQueue.Refresh();
        }

        /// <summary>
        /// Cancels a download and removes it from the queue.
        /// </summary>
        private void CancelDownload(DownloadItem item)
        {
            item.IsCancelled = true;
            if (cancellationTokens.ContainsKey(item.Id))
                cancellationTokens[item.Id].Cancel();

            queueItems.Remove(item);
            // Re-index remaining items
            for (int i = 0; i < queueItems.Count; i++)
                queueItems[i].Id = i + 1;

            lblQueueCount.Text = queueItems.Count.ToString();
            queueBindingSource.ResetBindings(false);
            RefreshQueueGrid();
        }

        /// <summary>
        /// Refreshes the queue DataGridView, ensuring thread safety via Invoke if needed.
        /// </summary>
        private void RefreshQueueGrid()
        {
            if (InvokeRequired)
                Invoke(() => dgvQueue.Refresh());
            else
                dgvQueue.Refresh();
        }

        #endregion
    }
}