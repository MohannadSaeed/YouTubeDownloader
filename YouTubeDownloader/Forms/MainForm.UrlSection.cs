using YouTubeDownloader.Helpers;
using YouTubeDownloader.Models;

namespace YouTubeDownloader.Forms
{
    public partial class MainForm
    {
        #region URL Section

        /// <summary>
        /// Builds the URL input section where users enter YouTube links.
        /// </summary>
        private void BuildUrlSection()
        {
            urlSection = new RoundedPanel
            {
                Width = ClientSize.Width - 50,
                Height = 190,
                BorderRadius = 25,
                BackColor = Color.FromArgb(18, 24, 38),
                Location = new Point(25, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            contentPanel.Controls.Add(urlSection);
            BuildUrlControls();
        }

        /// <summary>
        /// Creates all controls inside the URL section: textbox, format/quality dropdowns, folder picker, and add button.
        /// </summary>
        private void BuildUrlControls()
        {
            Label lblUrl = new()
            {
                Text = "YouTube Link",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 30)
            };
            urlSection.Controls.Add(lblUrl);

            txtUrl = new TextBox
            {
                Width = 1360,
                Height = 45,
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(25, 35, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(180, 25)
            };
            txtUrl.TextChanged += TxtUrl_TextChanged;
            urlSection.Controls.Add(txtUrl);

            BuildFormatControls();
            BuildQualityControls();
            BuildFolderControls();
            BuildAddQueueButton();
        }

        /// <summary>
        /// Creates the format selection dropdown (MP4 Video / MP3 Audio).
        /// </summary>
        private void BuildFormatControls()
        {
            Label lblFormat = new()
            {
                Text = "Format",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 95)
            };
            urlSection.Controls.Add(lblFormat);

            cmbFormat = new ComboBox
            {
                Width = 220,
                Height = 40,
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(25, 35, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(180, 90),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFormat.Items.AddRange(new[] { "MP4 Video", "MP3 Audio" });
            cmbFormat.SelectedIndex = 0;
            urlSection.Controls.Add(cmbFormat);
        }

        /// <summary>
        /// Creates the quality selection dropdown.
        /// </summary>
        private void BuildQualityControls()
        {
            Label lblQuality = new()
            {
                Text = "Quality",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(450, 95)
            };
            urlSection.Controls.Add(lblQuality);

            cmbQuality = new ComboBox
            {
                Width = 220,
                Height = 40,
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(25, 35, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(560, 90),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbQuality.Items.AddRange(new[] { "Best Quality", "1080p", "720p", "480p" });
            cmbQuality.SelectedIndex = 0;
            urlSection.Controls.Add(cmbQuality);
        }

        /// <summary>
        /// Creates folder selection controls (textbox + browse button) and loads saved folder path.
        /// </summary>
        private void BuildFolderControls()
        {
            Label lblFolder = new()
            {
                Text = "Folder",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 140)
            };
            urlSection.Controls.Add(lblFolder);

            txtDownloadFolder = new TextBox
            {
                Width = 820,
                Height = 40,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(25, 35, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(180, 138)
            };
            urlSection.Controls.Add(txtDownloadFolder);

            btnBrowseFolder = new Button
            {
                Text = "Browse",
                Width = 150,
                Height = 40,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 120, 255),
                FlatStyle = FlatStyle.Flat,
                Location = new Point(1020, 136)
            };
            btnBrowseFolder.FlatAppearance.BorderSize = 0;
            btnBrowseFolder.Click += BtnBrowseFolder_Click;
            urlSection.Controls.Add(btnBrowseFolder);
            txtDownloadFolder.Text = Path.GetDirectoryName(appSettings.VideoFolder) ?? "";
        }

        /// <summary>
        /// Opens a folder browser dialog and saves the selected path.
        /// </summary>
        private void BtnBrowseFolder_Click(object? sender, EventArgs e)
        {
            using FolderBrowserDialog dialog = new();
            if (dialog.ShowDialog() != DialogResult.OK) return;
            txtDownloadFolder.Text = dialog.SelectedPath;
            appSettings.VideoFolder = Path.Combine(dialog.SelectedPath, "Video");
            appSettings.AudioFolder = Path.Combine(dialog.SelectedPath, "Audio");
            settingsService.Save(appSettings);
        }


        /// <summary>
        /// Returns the appropriate download folder (Video or Audio) based on the selected format.
        /// </summary>
        private string GetDownloadFolder(string format)
        {
            string folder = format.Contains("MP3") ? appSettings.AudioFolder : appSettings.VideoFolder;
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return folder;
        }

        /// <summary>
        /// Creates the "Add To Queue" button (click handler is in MainForm.Queue.cs).
        /// </summary>
        private void BuildAddQueueButton()
        {
            btnAddQueue = new Button
            {
                Text = "Add To Queue",
                Width = 260,
                Height = 60,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 120, 255),
                FlatStyle = FlatStyle.Flat,
                Location = new Point(1280, 82)
            };
            btnAddQueue.FlatAppearance.BorderSize = 0;
            btnAddQueue.Click += BtnAddQueue_Click;
            urlSection.Controls.Add(btnAddQueue);
        }

        #endregion
    }
}