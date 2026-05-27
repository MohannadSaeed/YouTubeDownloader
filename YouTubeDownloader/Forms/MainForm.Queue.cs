using YouTubeDownloader.Controls;
using YouTubeDownloader.Helpers;
using YouTubeDownloader.Models;

namespace YouTubeDownloader.Forms
{
    public partial class MainForm
    {
        #region Queue Panel

        /// <summary>
        /// Builds the download queue panel containing the list of items to download.
        /// </summary>
        private void BuildQueuePanel()
        {
            queuePanel = new RoundedPanel
            {
                Width = ClientSize.Width - 50,
                Height = 250,
                BorderRadius = 25,
                BackColor = Color.FromArgb(18, 24, 38),
                Location = new Point(25, 490),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            contentPanel.Controls.Add(queuePanel);
            queuePanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            queuePanel.Height = contentPanel.ClientSize.Height - queuePanel.Top - footerPanel.Height - 20;
            BuildQueueHeader();
            BuildQueueGrid();
        }

        /// <summary>
        /// Creates the header of the queue panel (title, count, start button).
        /// </summary>
        private void BuildQueueHeader()
        {
            Label lblTitle = new()
            {
                Text = "Download Queue",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            queuePanel.Controls.Add(lblTitle);

            lblQueueCount = new Label
            {
                Text = "0",
                Width = 35,
                Height = 25,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(0, 120, 255),
                ForeColor = Color.White,
                Location = new Point(190, 20)
            };
            queuePanel.Controls.Add(lblQueueCount);

            btnStartDownloads = new Button
            {
                Text = "Start Downloads",
                Width = 180,
                Height = 40,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(1120, 15)
            };
            btnStartDownloads.FlatAppearance.BorderSize = 0;
            btnStartDownloads.Click += BtnStartDownloads_Click;
            queuePanel.Controls.Add(btnStartDownloads);
        }

        /// <summary>
        /// Configures the DataGridView that displays the download queue with columns for title, format, progress, pause, cancel, etc.
        /// </summary>
        private void BuildQueueGrid()
        {
            dgvQueue = new DataGridView
            {
                Width = queuePanel.Width - 40,
                Height = queuePanel.ClientSize.Height - 80,
                Location = new Point(20, 65),
                BackgroundColor = Color.FromArgb(15, 20, 30),
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                ReadOnly = false,
                AutoGenerateColumns = false,
                EditMode = DataGridViewEditMode.EditProgrammatically,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(40, 50, 70),
                ScrollBars = ScrollBars.Both,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            dgvQueue.RowTemplate.Height = 38;
            dgvQueue.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvQueue.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvQueue.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(25, 35, 50);
            dgvQueue.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvQueue.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvQueue.ColumnHeadersHeight = 40;
            dgvQueue.DefaultCellStyle.BackColor = Color.FromArgb(18, 24, 38);
            dgvQueue.DefaultCellStyle.ForeColor = Color.White;
            dgvQueue.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 255);
            dgvQueue.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvQueue.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgvQueue.DefaultCellStyle.Padding = new Padding(2);
            DataGridViewHelper.EnableDoubleBuffering(dgvQueue);

            AddQueueColumns();

            // Make all non-button columns read-only
            foreach (DataGridViewColumn column in dgvQueue.Columns)
                if (column is not DataGridViewButtonColumn) column.ReadOnly = true;

            queueBindingSource.DataSource = queueItems;
            dgvQueue.DataSource = queueBindingSource;
            dgvQueue.CellContentClick += DgvQueue_CellContentClick;
            dgvQueue.CellClick += DgvQueue_CellContentClick;
            dgvQueue.CellFormatting += DgvQueue_CellFormatting;

            queuePanel.Controls.Add(dgvQueue);
        }

        /// <summary>
        /// Adds all columns to the queue DataGridView.
        /// </summary>
        private void AddQueueColumns()
        {
            dgvQueue.Columns.Clear();
            dgvQueue.Columns.Add("Id", "#");
            dgvQueue.Columns["Id"].DataPropertyName = "Id";
            dgvQueue.Columns.Add("Title", "Title");
            dgvQueue.Columns["Title"].DataPropertyName = "Title";
            dgvQueue.Columns.Add("Format", "Format");
            dgvQueue.Columns["Format"].DataPropertyName = "Format";
            dgvQueue.Columns.Add("Quality", "Quality");
            dgvQueue.Columns["Quality"].DataPropertyName = "Quality";
            dgvQueue.Columns.Add("Status", "Status");
            dgvQueue.Columns["Status"].DataPropertyName = "Status";

            DataGridViewProgressColumn progressColumn = new()
            {
                Name = "Progress",
                HeaderText = "Progress",
                DataPropertyName = "Progress",
                Width = 180
            };
            dgvQueue.Columns.Add(progressColumn);

            dgvQueue.Columns.Add("Speed", "Speed");
            dgvQueue.Columns["Speed"].DataPropertyName = "Speed";
            dgvQueue.Columns.Add("ETA", "ETA");
            dgvQueue.Columns["ETA"].DataPropertyName = "ETA";
            dgvQueue.Columns.Add("Size", "Size");
            dgvQueue.Columns["Size"].DataPropertyName = "Size";

            DataGridViewButtonColumn pauseButton = new()
            {
                Name = "PauseColumn",
                HeaderText = "Pause",
                Width = 70,
                UseColumnTextForButtonValue = false,
                FlatStyle = FlatStyle.Flat
            };
            dgvQueue.Columns.Add(pauseButton);

            DataGridViewButtonColumn cancelButton = new()
            {
                Name = "CancelColumn",
                HeaderText = "Cancel",
                Text = "Cancel",
                UseColumnTextForButtonValue = true,
                Width = 70,
                FlatStyle = FlatStyle.Flat
            };
            dgvQueue.Columns.Add(cancelButton);

            // Set column widths
            dgvQueue.Columns["Id"].Width = 50;
            dgvQueue.Columns["Title"].Width = 260;
            dgvQueue.Columns["Format"].Width = 120;
            dgvQueue.Columns["Quality"].Width = 130;
            dgvQueue.Columns["Status"].Width = 120;
            dgvQueue.Columns["Progress"].Width = 180;
            dgvQueue.Columns["Speed"].Width = 110;
            dgvQueue.Columns["ETA"].Width = 90;
            dgvQueue.Columns["Size"].Width = 90;
            dgvQueue.Columns["Title"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        /// <summary>
        /// Handles the Add To Queue button click: adds a single video or an entire playlist to the download queue.
        /// </summary>
        private async void BtnAddQueue_Click(object? sender, EventArgs e)
        {
            string url = txtUrl.Text.Trim();
            bool isPlaylist = url.Contains("list=");

            if (isPlaylist)
            {
                var videos = await youtubeService.GetPlaylistVideosAsync(url);
                foreach (var video in videos)
                {
                    DownloadItem item = new()
                    {
                        Id = queueItems.Count + 1,
                        Title = video.Title,
                        Url = video.Url,
                        Format = cmbFormat.Text,
                        Quality = cmbQuality.Text,
                        Status = "Queued",
                        Progress = 0
                    };
                    queueItems.Add(item);
                }
            }
            else
            {
                DownloadItem item = new()
                {
                    Id = queueItems.Count + 1,
                    Title = lblVideoTitle.Text,
                    Url = url,
                    Format = cmbFormat.Text,
                    Quality = cmbQuality.Text,
                    Status = "Queued",
                    Progress = 0
                };
                queueItems.Add(item);
            }

            lblQueueCount.Text = queueItems.Count.ToString();
            queueBindingSource.ResetBindings(false);
            StyleGridButtons();
            txtUrl.Clear();
            ClearPreview();
        }

        /// <summary>
        /// Styles all button cells in the queue grid with a consistent blue color.
        /// </summary>
        private void StyleGridButtons()
        {
            foreach (DataGridViewRow row in dgvQueue.Rows)
                foreach (DataGridViewCell cell in row.Cells)
                    if (cell is DataGridViewButtonCell buttonCell)
                    {
                        buttonCell.Style.BackColor = Color.FromArgb(0, 120, 255);
                        buttonCell.Style.ForeColor = Color.White;
                        buttonCell.Style.SelectionBackColor = Color.FromArgb(0, 100, 220);
                        buttonCell.Style.SelectionForeColor = Color.White;
                        buttonCell.Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    }
        }

        /// <summary>
        /// Handles cell formatting to change the Pause button text based on item state.
        /// </summary>
        private void DgvQueue_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvQueue.Columns[e.ColumnIndex].Name == "PauseColumn")
            {
                DownloadItem item = queueItems[e.RowIndex];
                e.Value = item.IsPaused ? "Resume" : "Pause";
            }
        }

        /// <summary>
        /// Handles clicks on the queue grid's Pause or Cancel buttons.
        /// </summary>
        private void DgvQueue_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DownloadItem item = queueItems[e.RowIndex];
            string columnName = dgvQueue.Columns[e.ColumnIndex].Name;

            if (columnName == "PauseColumn")
                TogglePause(item);
            else if (columnName == "CancelColumn")
                CancelDownload(item);
        }

        #endregion
    }
}