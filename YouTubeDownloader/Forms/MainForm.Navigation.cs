using FontAwesome.Sharp;

namespace YouTubeDownloader.Forms
{
    public partial class MainForm
    {
        #region Navigation

        /// <summary>
        /// Creates the navigation bar with buttons to switch between Queue and History views.
        /// </summary>
        private void BuildNavigationBar()
        {
            navigationBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(18, 24, 38)
            };
            Controls.Add(navigationBar);

            int buttonWidth = 180;
            int spacing = 20;
            int totalWidth = (buttonWidth * 3) + (spacing * 2);

            // Reposition buttons when navigation bar is resized
            navigationBar.Resize += (s, e) =>
            {
                int x = (navigationBar.Width - totalWidth) / 2;
                btnHome.Left = x;
                btnDownloads.Left = x + buttonWidth + spacing;
                btnSettings.Left = x + (buttonWidth * 2) + (spacing * 2);
            };

            btnHome = CreateNavButton("Queue", IconChar.List);
            btnHome.Width = buttonWidth;
            btnHome.Height = 40;
            btnHome.Location = new Point(450, 15);
            btnHome.Click += (s, e) =>
            {
                // Show queue-related panels
                queuePanel.Visible = true;
                previewCard.Visible = true;
                urlSection.Visible = true;
                historyPanel.Visible = false;
                settingsPanel.Visible = false;
            };
            navigationBar.Controls.Add(btnHome);

            btnDownloads = CreateNavButton("Downloads", IconChar.ClockRotateLeft);
            btnDownloads.Width = buttonWidth;
            btnDownloads.Height = 40;
            btnDownloads.Location = new Point(650, 15);
            btnDownloads.Click += BtnDownloads_Click;
            navigationBar.Controls.Add(btnDownloads);

            btnSettings = CreateNavButton("Settings", IconChar.Gears);
            btnSettings.Width = buttonWidth;
            btnSettings.Height = 40;
            btnSettings.Location = new Point(850, 15);
            btnSettings.Click += BtnSettings_Click;
            navigationBar.Controls.Add(btnSettings);
        }

        /// <summary>
        /// Factory method to create a styled navigation button.
        /// </summary>
        private IconButton CreateNavButton(string text, IconChar icon)
        {
            return new IconButton
            {
                Text = text,
                IconChar = icon,
                IconColor = Color.White,
                IconSize = 20,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Width = 170,
                Height = 45,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(25, 35, 55)
            };
        }

        /// <summary>
        /// Handles click on the Downloads navigation button: shows history panel and hides queue panels.
        /// </summary>
        private void BtnDownloads_Click(object? sender, EventArgs e)
        {
            if (historyPanel == null) return;
            queuePanel.Visible = false;
            previewCard.Visible = false;
            urlSection.Visible = false;
            historyPanel.Visible = true;
            settingsPanel.Visible = false;
            LoadHistory();
        }

        /// <summary>
        /// Handles click on the Settings navigation button: opens the settings form.
        /// </summary>
        private void BtnSettings_Click(object? sender, EventArgs e)
        {
            queuePanel.Visible = false;
            previewCard.Visible = false;
            urlSection.Visible = false;
            historyPanel.Visible = false;
            settingsPanel.Visible = true;
            LoadSettingsToUI();
        }

        #endregion
    }
}