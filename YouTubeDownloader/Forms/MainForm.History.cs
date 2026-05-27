using System.Diagnostics;
using YouTubeDownloader.Helpers;

namespace YouTubeDownloader.Forms
{
    public partial class MainForm
    {
        #region History Panel

        /// <summary>
        /// Builds the history panel showing previously downloaded items.
        /// </summary>
        private void BuildHistoryPanel()
        {
            historyPanel = new RoundedPanel
            {
                Width = ClientSize.Width - 50,
                Height = contentPanel.ClientSize.Height - footerPanel.Height - 50,
                BorderRadius = 25,
                BackColor = Color.FromArgb(18, 24, 38),
                Location = new Point(25, 25),
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            contentPanel.Controls.Add(historyPanel);

            Label lblTitle = new()
            {
                Text = "Download History",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            historyPanel.Controls.Add(lblTitle);

            BuildHistoryFilters();
            BuildHistoryGrid();

            historyPanel.Resize += (s, e) =>
            {
                dgvHistory.Width = historyPanel.ClientSize.Width - 40;
                dgvHistory.Height = historyPanel.ClientSize.Height - 140;
            };
        }

        /// <summary>
        /// Creates search and filter controls for the history panel.
        /// </summary>
        private void BuildHistoryFilters()
        {
            txtHistorySearch = new TextBox
            {
                Width = 350,
                Height = 40,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(25, 35, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(20, 60)
            };
            txtHistorySearch.TextChanged += (s, e) => LoadHistory();
            historyPanel.Controls.Add(txtHistorySearch);

            cmbHistoryFilter = new ComboBox
            {
                Width = 150,
                Height = 40,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(25, 35, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(390, 60),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbHistoryFilter.Items.AddRange(new[] { "All", "MP3", "MP4" });
            cmbHistoryFilter.SelectedIndex = 0;
            cmbHistoryFilter.SelectedIndexChanged += (s, e) => LoadHistory();
            historyPanel.Controls.Add(cmbHistoryFilter);
        }

        /// <summary>
        /// Configures the DataGridView that displays download history.
        /// </summary>
        private void BuildHistoryGrid()
        {
            dgvHistory = new DataGridView
            {
                Width = historyPanel.ClientSize.Width - 40,
                Height = historyPanel.ClientSize.Height - 140,
                Location = new Point(20, 120),
                BackgroundColor = Color.FromArgb(15, 20, 30),
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoGenerateColumns = false,
                ReadOnly = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                EnableHeadersVisualStyles = false
            };
            // Column header styling
            dgvHistory.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(25, 35, 50);
            dgvHistory.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvHistory.DefaultCellStyle.BackColor = Color.FromArgb(18, 24, 38);
            dgvHistory.DefaultCellStyle.ForeColor = Color.White;
            dgvHistory.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 255);
            dgvHistory.RowTemplate.Height = 40;

            // Add columns
            dgvHistory.Columns.Add("Title", "Title");
            dgvHistory.Columns["Title"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvHistory.Columns.Add("Format", "Format");
            dgvHistory.Columns.Add("Date", "Downloaded");
            dgvHistory.Columns.Add("Path", "File Path");
            dgvHistory.Columns["Path"].Width = 450;

            // Action buttons: Open and Folder
            DataGridViewButtonColumn openButton = new()
            {
                Name = "OpenColumn",
                HeaderText = "Open",
                Text = "Open",
                UseColumnTextForButtonValue = true,
                Width = 90,
                FlatStyle = FlatStyle.Flat
            };
            dgvHistory.Columns.Add(openButton);

            DataGridViewButtonColumn folderButton = new()
            {
                Name = "FolderColumn",
                HeaderText = "Folder",
                Text = "Folder",
                UseColumnTextForButtonValue = true,
                Width = 90,
                FlatStyle = FlatStyle.Flat
            };
            dgvHistory.Columns.Add(folderButton);

            dgvHistory.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvHistory.GridColor = Color.FromArgb(40, 50, 70);
            dgvHistory.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvHistory.DefaultCellStyle.Padding = new Padding(2);
            dgvHistory.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgvHistory.ColumnHeadersHeight = 40;
            dgvHistory.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvHistory.CellClick += DgvHistory_CellClick;

            historyPanel.Controls.Add(dgvHistory);
        }

        /// <summary>
        /// Loads download history from the database and filters based on search text and format filter.
        /// </summary>
        private void LoadHistory()
        {
            dgvHistory.Rows.Clear();
            var query = db.DownloadHistories.OrderByDescending(x => x.DownloadedAt).AsQueryable();

            // Apply search filter
            string search = txtHistorySearch.Text.Trim().ToLower();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(x => x.Title.ToLower().Contains(search));

            // Apply format filter
            string filter = cmbHistoryFilter.Text;
            if (filter == "MP3")
                query = query.Where(x => x.Format.Contains("MP3"));
            if (filter == "MP4")
                query = query.Where(x => x.Format.Contains("MP4"));

            foreach (var item in query.ToList())
            {
                dgvHistory.Rows.Add(item.Title, item.Format, item.DownloadedAt.ToString("yyyy-MM-dd HH:mm"), item.FilePath);
            }
            StyleHistoryButtons();
        }

        /// <summary>
        /// Applies consistent styling to the action buttons in the history grid.
        /// </summary>
        private void StyleHistoryButtons()
        {
            foreach (DataGridViewRow row in dgvHistory.Rows)
            {
                StyleHistoryButtonCell(row, "OpenColumn");
                StyleHistoryButtonCell(row, "FolderColumn");
            }
        }

        /// <summary>
        /// Styles a specific button cell in the history grid.
        /// </summary>
        /// <param name="row">The row containing the cell.</param>
        /// <param name="columnName">The name of the button column.</param>
        private void StyleHistoryButtonCell(DataGridViewRow row, string columnName)
        {
            DataGridViewCell cell = row.Cells[columnName];
            cell.Style.BackColor = Color.FromArgb(0, 120, 255);
            cell.Style.ForeColor = Color.White;
            cell.Style.SelectionBackColor = Color.FromArgb(0, 100, 220);
            cell.Style.SelectionForeColor = Color.White;
            cell.Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            cell.Style.Padding = new Padding(3);
            cell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        /// <summary>
        /// Handles clicks on history grid buttons (Open or Folder).
        /// </summary>
        private void DgvHistory_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            string path = dgvHistory.Rows[e.RowIndex].Cells["Path"].Value?.ToString() ?? "";
            string column = dgvHistory.Columns[e.ColumnIndex].Name;

            if (column == "OpenColumn" && File.Exists(path))
            {
                Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
            }
            else if (column == "FolderColumn" && File.Exists(path))
            {
                Process.Start("explorer.exe", $"/select,\"{path}\"");
            }
        }

        #endregion
    }
}