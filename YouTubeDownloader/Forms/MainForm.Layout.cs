namespace YouTubeDownloader.Forms
{
    public partial class MainForm
    {
        #region Layout Construction

        /// <summary>
        /// Builds the entire UI layout by calling individual panel builders.
        /// </summary>
        private void BuildLayout()
        {
            BuildFooterPanel();
            BuildContentPanel();
            BuildUrlSection();
            BuildPreviewCard();
            BuildQueuePanel();
            BuildHistoryPanel();
            BuildSettingsPanel();
            BuildNavigationBar();
            BuildTitleBar();
            InitializeClipboardMonitor();
        }

        /// <summary>
        /// Creates the custom title bar with close button and drag functionality.
        /// </summary>
        private void BuildTitleBar()
        {
            titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = Color.FromArgb(15, 20, 30)
            };
            Controls.Add(titleBar);

            Label lblTitle = new()
            {
                Text = " YouTube Downloader",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(15, 12)
            };
            titleBar.Controls.Add(lblTitle);

            Button btnClose = new()
            {
                Text = "✕",
                Width = 45,
                Height = 45,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Dock = DockStyle.Right
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Close();
            titleBar.Controls.Add(btnClose);

            // Enable dragging the form from the title bar
            titleBar.MouseDown += TitleBar_MouseDown;
            titleBar.MouseMove += TitleBar_MouseMove;
            titleBar.MouseUp += TitleBar_MouseUp;
        }

        /// <summary>
        /// Creates the main content panel that holds all dynamic sections.
        /// </summary>
        private void BuildContentPanel()
        {
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(10, 15, 25),
                AutoScroll = true
            };
            Padding = new Padding(0, 0, 0, 20);
            contentPanel.Resize += (s, e) =>
            {
                if (queuePanel != null)
                    queuePanel.Height = contentPanel.ClientSize.Height - queuePanel.Top - 30;
            };
            Controls.Add(contentPanel);
        }

        /// <summary>
        /// Creates the footer panel with a status label.
        /// </summary>
        private void BuildFooterPanel()
        {
            footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.FromArgb(15, 20, 30)
            };
            Controls.Add(footerPanel);

            Label lblStatus = new()
            {
                Text = "Ready",
                ForeColor = Color.LightGreen,
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(20, 10)
            };
            footerPanel.Controls.Add(lblStatus);
        }

        #endregion
    }
}