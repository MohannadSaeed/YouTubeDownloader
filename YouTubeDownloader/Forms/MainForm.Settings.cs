using FontAwesome.Sharp;
using System;
using System.Drawing;
using System.Windows.Forms;
using YouTubeDownloader.Helpers;
using YouTubeDownloader.Models;

namespace YouTubeDownloader.Forms
{
    public partial class MainForm
    {
        #region Settings UI Construction

        /// <summary>
        /// Builds the settings panel container and adds it to the content panel.
        /// </summary>
        private void BuildSettingsPanel()
        {
            settingsPanel = new RoundedPanel
            {
                Width = ClientSize.Width - 50,
                Height = contentPanel.ClientSize.Height - footerPanel.Height - 50,
                BorderRadius = 25,
                BackColor = Color.FromArgb(18, 24, 38),
                Location = new Point(25, 25),
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            contentPanel.Controls.Add(settingsPanel);
            BuildSettingsControls();
        }

        /// <summary>
        /// Creates all the controls inside the settings panel: folder paths, performance options, preferences, and save button.
        /// </summary>
        private void BuildSettingsControls()
        {
            int left = 40;
            int top = 30;
            int spacing = 70;

            // Title label
            Label lblTitle = new()
            {
                Text = "Application Settings",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(left, top)
            };
            settingsPanel.Controls.Add(lblTitle);

            top += 70;

            // Download Folders section
            AddSectionTitle("Download Folders", left, top);
            top += 50;

            // Video folder textbox and browse button
            txtVideoFolder = CreateSettingsTextBox(left, top);
            settingsPanel.Controls.Add(txtVideoFolder);

            IconButton btnBrowseVideo = CreateIconButton("Browse", IconChar.FolderOpen, 150, 40);
            btnBrowseVideo.Location = new Point(left + 850, top);
            btnBrowseVideo.Click += BtnBrowseVideo_Click;
            settingsPanel.Controls.Add(btnBrowseVideo);

            top += spacing;

            // Audio folder textbox and browse button
            txtAudioFolder = CreateSettingsTextBox(left, top);
            settingsPanel.Controls.Add(txtAudioFolder);

            IconButton btnBrowseAudio = CreateIconButton("Browse", IconChar.FolderOpen, 150, 40);
            btnBrowseAudio.Location = new Point(left + 850, top);
            btnBrowseAudio.Click += BtnBrowseAudio_Click;
            settingsPanel.Controls.Add(btnBrowseAudio);

            top += 90;

            // Performance section
            AddSectionTitle("Performance", left, top);
            top += 50;

            // Maximum concurrent downloads spinner
            numConcurrentDownloads = new NumericUpDown
            {
                Width = 120,
                Height = 40,
                Minimum = 1,
                Maximum = 10,
                Value = 3,
                Font = new Font("Segoe UI", 11),
                Location = new Point(left, top)
            };
            settingsPanel.Controls.Add(numConcurrentDownloads);

            top += 90;

            // Preferences section
            AddSectionTitle("Preferences", left, top);
            top += 50;

            // Show notifications checkbox
            chkShowNotifications = CreateSettingsCheckbox("Show Notifications", left, top);
            settingsPanel.Controls.Add(chkShowNotifications);

            top += 45;

            // Auto-paste clipboard URL checkbox
            chkAutoPasteClipboard = CreateSettingsCheckbox("Auto Paste Clipboard URL", left, top);
            settingsPanel.Controls.Add(chkAutoPasteClipboard);

            top += 45;

            // Minimize to tray checkbox
            chkMinimizeTray = CreateSettingsCheckbox("Minimize To Tray", left, top);
            settingsPanel.Controls.Add(chkMinimizeTray);

            top += 90;

            // Save Settings button
            btnSaveSettings = CreateIconButton("Save Settings", IconChar.FloppyDisk, 220, 55);
            btnSaveSettings.Location = new Point(left, top - 20);
            btnSaveSettings.FlatAppearance.BorderSize = 0;
            btnSaveSettings.Click += BtnSaveSettings_Click;
            settingsPanel.Controls.Add(btnSaveSettings);
        }

        /// <summary>
        /// Adds a section title label to the settings panel.
        /// </summary>
        /// <param name="text">The title text.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        private void AddSectionTitle(string text, int x, int y)
        {
            Label lbl = new()
            {
                Text = text,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(x, y)
            };
            settingsPanel.Controls.Add(lbl);
        }

        /// <summary>
        /// Creates a styled text box for folder paths.
        /// </summary>
        private TextBox CreateSettingsTextBox(int x, int y)
        {
            return new TextBox
            {
                Width = 820,
                Height = 40,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(25, 35, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(x, y)
            };
        }

        /// <summary>
        /// Creates a "Browse" button used for folder selection.
        /// </summary>
        private Button CreateBrowseButton(int x, int y)
        {
            Button btn = new()
            {
                Text = "Browse",
                Width = 150,
                Height = 40,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 120, 255),
                FlatStyle = FlatStyle.Flat,
                Location = new Point(x, y)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        /// <summary>
        /// Creates a styled checkbox for settings options.
        /// </summary>
        private CheckBox CreateSettingsCheckbox(string text, int x, int y)
        {
            return new CheckBox
            {
                Text = text,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11),
                AutoSize = true,
                Location = new Point(x, y)
            };
        }

        #endregion

        #region Settings Logic

        /// <summary>
        /// Loads the saved application settings from the service into the UI controls.
        /// </summary>
        private void LoadSettingsToUI()
        {
            AppSettings settings = settingsService.GetSettings();
            txtVideoFolder.Text = settings.VideoFolder;
            txtAudioFolder.Text = settings.AudioFolder;
            numConcurrentDownloads.Value = settings.MaxParallelDownloads;
            chkShowNotifications.Checked = settings.ShowNotifications;
            chkAutoPasteClipboard.Checked = settings.AutoPasteClipboardUrl;
            chkMinimizeTray.Checked = settings.MinimizeToTray;
        }

        /// <summary>
        /// Opens a folder browser dialog to select the video download folder.
        /// </summary>
        private void BtnBrowseVideo_Click(object? sender, EventArgs e)
        {
            using FolderBrowserDialog dialog = new();
            if (dialog.ShowDialog() != DialogResult.OK)
                return;
            txtVideoFolder.Text = dialog.SelectedPath;
        }

        /// <summary>
        /// Opens a folder browser dialog to select the audio download folder.
        /// </summary>
        private void BtnBrowseAudio_Click(object? sender, EventArgs e)
        {
            using FolderBrowserDialog dialog = new();
            if (dialog.ShowDialog() != DialogResult.OK)
                return;
            txtAudioFolder.Text = dialog.SelectedPath;
        }

        /// <summary>
        /// Saves the current UI settings to persistent storage via the settings service.
        /// </summary>
        private void BtnSaveSettings_Click(object? sender, EventArgs e)
        {
            AppSettings settings = settingsService.GetSettings();
            settings.VideoFolder = txtVideoFolder.Text.Trim();
            settings.AudioFolder = txtAudioFolder.Text.Trim();
            settings.MaxParallelDownloads = (int)numConcurrentDownloads.Value;
            settings.ShowNotifications = chkShowNotifications.Checked;
            settings.AutoPasteClipboardUrl = chkAutoPasteClipboard.Checked;
            settings.MinimizeToTray = chkMinimizeTray.Checked;
            settingsService.Save(settings);

            MessageBox.Show("Settings saved successfully.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion
    }
}