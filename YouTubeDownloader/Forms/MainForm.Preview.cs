using YouTubeDownloader.Helpers;

namespace YouTubeDownloader.Forms
{
    public partial class MainForm
    {
        #region Preview Card

        /// <summary>
        /// Builds the video/playlist preview card that shows thumbnail and metadata.
        /// </summary>
        private void BuildPreviewCard()
        {
            previewCard = new RoundedPanel
            {
                Width = ClientSize.Width - 50,
                Height = 250,
                BorderRadius = 25,
                BackColor = Color.FromArgb(18, 24, 38),
                Location = new Point(25, 230),
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            contentPanel.Controls.Add(previewCard);
            BuildPreviewControls();
        }

        /// <summary>
        /// Creates all controls inside the preview card: thumbnail image, title, author, duration, etc.
        /// </summary>
        private void BuildPreviewControls()
        {
            picThumbnail = new PictureBox
            {
                Width = 360,
                Height = 200,
                Location = new Point(20, 25),
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            previewCard.Controls.Add(picThumbnail);

            lblVideoTitle = new Label
            {
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Width = 1100,
                Height = 60,
                Location = new Point(410, 30)
            };
            previewCard.Controls.Add(lblVideoTitle);

            lblAuthor = new Label
            {
                ForeColor = Color.Silver,
                Font = new Font("Segoe UI", 11),
                AutoSize = true,
                Location = new Point(410, 105)
            };
            previewCard.Controls.Add(lblAuthor);

            lblDuration = new Label
            {
                ForeColor = Color.Silver,
                Font = new Font("Segoe UI", 11),
                AutoSize = true,
                Location = new Point(410, 140)
            };
            previewCard.Controls.Add(lblDuration);

            lblPublishDate = new Label
            {
                ForeColor = Color.Silver,
                Font = new Font("Segoe UI", 11),
                AutoSize = true,
                Location = new Point(520, 140)
            };
            previewCard.Controls.Add(lblPublishDate);

            lblLoading = new Label
            {
                Text = "Loading video information...",
                ForeColor = Color.DeepSkyBlue,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(410, 180),
                Visible = false
            };
            previewCard.Controls.Add(lblLoading);

            previewLoader = new ProgressBar
            {
                Width = 300,
                Height = 10,
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Location = new Point(410, 205),
                Visible = false
            };
            previewCard.Controls.Add(previewLoader);
        }

        /// <summary>
        /// Handles the TextChanged event of the URL textbox: fetches video/playlist preview after a short delay.
        /// Implements debouncing to avoid excessive API calls.
        /// </summary>
        private async void TxtUrl_TextChanged(object? sender, EventArgs e)
        {
            string url = txtUrl.Text.Trim();
            previewCts?.Cancel();
            previewCts = new CancellationTokenSource();
            CancellationToken token = previewCts.Token;

            try
            {
                await Task.Delay(700, token); // Wait for user to stop typing
                if (token.IsCancellationRequested) return;

                if (string.IsNullOrWhiteSpace(url))
                {
                    ClearPreview();
                    return;
                }
                if (!url.Contains("youtube.com") && !url.Contains("youtu.be"))
                {
                    ShowPreviewError("Invalid YouTube URL");
                    return;
                }

                ShowPreviewLoading(true);
                await LoadVideoPreviewAsync(url);
                ShowPreviewLoading(false);
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                ShowPreviewLoading(false);
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Shows or hides the loading spinner and disables controls during preview loading.
        /// </summary>
        private void ShowPreviewLoading(bool loading)
        {
            lblLoading.Visible = loading;
            previewLoader.Visible = loading;
            txtUrl.Enabled = !loading;
            cmbFormat.Enabled = !loading;
            cmbQuality.Enabled = !loading;
            btnAddQueue.Enabled = !loading;
            Cursor = loading ? Cursors.WaitCursor : Cursors.Default;
        }

        /// <summary>
        /// Fetches video or playlist information from YouTubeService and updates the preview UI.
        /// </summary>
        private async Task LoadVideoPreviewAsync(string url)
        {
            try
            {
                previewCard.Visible = true;
                lblVideoTitle.Text = "Loading...";
                lblAuthor.Text = "";
                lblDuration.Text = "";
                lblPublishDate.Text = "";
                picThumbnail.Image = null;

                bool isPlaylist = url.Contains("list=");
                if (isPlaylist)
                {
                    var playlist = await youtubeService.GetPlaylistInfoAsync(url);
                    if (playlist == null)
                    {
                        ShowPreviewError("Unable to load playlist.");
                        return;
                    }
                    lblVideoTitle.Text = playlist.Title;
                    lblAuthor.Text = $"Channel: {playlist.Author}";
                    lblDuration.Text = $"{playlist.VideoCount} videos";
                    lblPublishDate.Text = "Playlist";
                    await LoadThumbnailAsync(playlist.ThumbnailUrl);
                    return;
                }

                var info = await youtubeService.GetVideoInfoAsync(url);
                if (info == null)
                {
                    ShowPreviewError("Unable to load video.");
                    return;
                }
                lblVideoTitle.Text = info.Title;
                lblAuthor.Text = $"Channel: {info.Author}";
                lblDuration.Text = $"Duration: {info.Duration:mm\\:ss}";
                lblPublishDate.Text = $"Published: {info.UploadDate:yyyy-MM-dd}";
                await LoadThumbnailAsync(info.ThumbnailUrl);
            }
            catch (Exception ex)
            {
                ShowPreviewError(ex.Message);
            }
        }

        /// <summary>
        /// Loads a thumbnail image from a URL and displays it in the PictureBox.
        /// </summary>
        private async Task LoadThumbnailAsync(string url)
        {
            try
            {
                using HttpClient client = new();
                byte[] bytes = await client.GetByteArrayAsync(url);
                using MemoryStream ms = new(bytes);
                Image img = Image.FromStream(ms);
                if (picThumbnail.Image != null) picThumbnail.Image.Dispose();
                picThumbnail.Image = new Bitmap(img);
            }
            catch
            {
                picThumbnail.Image = null;
            }
        }

        /// <summary>
        /// Displays an error message in the preview card instead of metadata.
        /// </summary>
        private void ShowPreviewError(string message)
        {
            previewCard.Visible = true;
            lblVideoTitle.Text = message;
            lblAuthor.Text = "";
            lblDuration.Text = "";
            lblPublishDate.Text = "";
            picThumbnail.Image = null;
        }

        /// <summary>
        /// Clears the preview card and hides it.
        /// </summary>
        private void ClearPreview()
        {
            previewCard.Visible = false;
            if (picThumbnail.Image != null)
            {
                picThumbnail.Image.Dispose();
                picThumbnail.Image = null;
            }
            lblVideoTitle.Text = "";
            lblAuthor.Text = "";
            lblDuration.Text = "";
            lblPublishDate.Text = "";
            lblLoading.Visible = false;
            previewLoader.Visible = false;
        }

        #endregion
    }
}