using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Diagnostics;
using YouTubeDownloader.Controls;
using YouTubeDownloader.Data;
using YouTubeDownloader.Helpers;
using YouTubeDownloader.Models;
using YouTubeDownloader.Services;
using System.Net.Http;
using System.IO;

namespace YouTubeDownloader
{
    public partial class MainForm : Form
    {
        #region Fields

        private Panel titleBar = null!;
        private Panel navigationBar = null!;
        private Panel contentPanel = null!;
        private Panel footerPanel = null!;

        private RoundedPanel urlSection = null!;
        private RoundedPanel previewCard = null!;
        private RoundedPanel queuePanel = null!;

        private TextBox txtUrl = null!;

        private ComboBox cmbFormat = null!;
        private ComboBox cmbQuality = null!;

        private Button btnAddQueue = null!;
        private Button btnStartDownloads = null!;

        private PictureBox picThumbnail = null!;

        private TextBox txtDownloadFolder = null!;

        private Button btnBrowseFolder = null!;

        private Label lblVideoTitle = null!;
        private Label lblAuthor = null!;
        private Label lblDuration = null!;
        private Label lblPublishDate = null!;
        private Label lblQueueCount = null!;

        private DataGridView dgvQueue = null!;

        private readonly BindingSource queueBindingSource = new();
        private readonly BindingList<DownloadItem> queueItems = new();

        private readonly YouTubeService youtubeService = new();
        private readonly DownloadService downloadService = new();

        private readonly SemaphoreSlim downloadSemaphore = new(3);

        private readonly Dictionary<int, CancellationTokenSource> cancellationTokens = new();

        private string downloadFolder = "";

        private bool isDragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        private Label lblLoading = null!;

        private ProgressBar previewLoader = null!;

        private CancellationTokenSource? previewCts;

        private readonly AppDbContext db = new();

        private Panel historyPanel = null!;

        private DataGridView dgvHistory = null!;

        private TextBox txtHistorySearch = null!;

        private ComboBox cmbHistoryFilter = null!;

        private Button btnHome = null!;

        private Button btnDownloads = null!;

        #endregion

        public MainForm()
        {
            InitializeComponent();

            db.Database.EnsureCreated();

            ConfigureForm();

            BuildLayout();
        }

        #region Layout

        private void ConfigureForm()
        {
            Text = "YouTube Downloader";

            FormBorderStyle = FormBorderStyle.None;

            StartPosition = FormStartPosition.CenterScreen;

            MinimumSize = new Size(1650, 950);

            Size = new Size(1650, 950);

            AutoScaleMode = AutoScaleMode.Dpi;



            BackColor = Color.FromArgb(10, 15, 25);
        }

        private void BuildLayout()
        {
            BuildFooterPanel();

            BuildContentPanel();

            BuildUrlSection();

            BuildPreviewCard();

            BuildQueuePanel();

            BuildHistoryPanel();

            BuildNavigationBar();

            BuildTitleBar();
        }

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

            titleBar.MouseDown += TitleBar_MouseDown;
            titleBar.MouseMove += TitleBar_MouseMove;
            titleBar.MouseUp += TitleBar_MouseUp;
        }


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

            int totalWidth =
                (buttonWidth * 2) + spacing;

            int startX =
                (navigationBar.Width - totalWidth) / 2;

            navigationBar.Resize += (s, e) =>
            {
                int x =
                    (navigationBar.Width - totalWidth) / 2;

                btnHome.Left = x;

                btnDownloads.Left =
                    x + buttonWidth + spacing;
            };

            btnHome = CreateNavButton("Queue");

            btnHome.Width = buttonWidth;

            btnHome.Height = 40;

            btnHome.Location = new Point(450, 15);

            btnHome.Click += (s, e) =>
            {
                queuePanel.Visible = true;

                previewCard.Visible = true;

                urlSection.Visible = true;

                historyPanel.Visible = false;
            };

            navigationBar.Controls.Add(btnHome);

            btnDownloads = CreateNavButton("Downloads");

            btnDownloads.Width = buttonWidth;

            btnDownloads.Height = 40;

            btnDownloads.Location = new Point(650, 15);

            btnDownloads.Click += BtnDownloads_Click;

            navigationBar.Controls.Add(btnDownloads);
        }

        private void BuildHistoryPanel()
        {
            historyPanel = new RoundedPanel
            {
                Width = ClientSize.Width - 50,
                Height =
    contentPanel.ClientSize.Height -
    footerPanel.Height -
    50,
                BorderRadius = 25,
                BackColor = Color.FromArgb(18, 24, 38),
                Location = new Point(25, 25),
                Visible = false,

                Anchor =
                    AnchorStyles.Top |
                    AnchorStyles.Bottom |
                    AnchorStyles.Left |
                    AnchorStyles.Right
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
                dgvHistory.Width =
                    historyPanel.ClientSize.Width - 40;

                dgvHistory.Height =
                    historyPanel.ClientSize.Height - 140;
            };
        }

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

            txtHistorySearch.TextChanged +=
                (s, e) => LoadHistory();

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

            cmbHistoryFilter.Items.AddRange(new[]
            {
        "All",
        "MP3",
        "MP4"
    });

            cmbHistoryFilter.SelectedIndex = 0;

            cmbHistoryFilter.SelectedIndexChanged +=
                (s, e) => LoadHistory();

            historyPanel.Controls.Add(cmbHistoryFilter);
        }

        private void BuildHistoryGrid()
        {
            dgvHistory = new DataGridView
            {
                Width = historyPanel.ClientSize.Width - 40,
                Height = historyPanel.ClientSize.Height - 140,
                Location = new Point(20, 120),

                BackgroundColor =
                    Color.FromArgb(15, 20, 30),

                BorderStyle = BorderStyle.None,

                AllowUserToAddRows = false,

                AllowUserToDeleteRows = false,

                RowHeadersVisible = false,

                AutoGenerateColumns = false,

                ReadOnly = true,
                Anchor =
    AnchorStyles.Top |
    AnchorStyles.Bottom |
    AnchorStyles.Left |
    AnchorStyles.Right,

                EnableHeadersVisualStyles = false
            };

            dgvHistory.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(25, 35, 50);

            dgvHistory.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;

            dgvHistory.DefaultCellStyle.BackColor =
                Color.FromArgb(18, 24, 38);

            dgvHistory.DefaultCellStyle.ForeColor =
                Color.White;

            dgvHistory.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(0, 120, 255);

            dgvHistory.RowTemplate.Height = 40;

            dgvHistory.Columns.Add("Title", "Title");

            dgvHistory.Columns["Title"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            dgvHistory.Columns.Add("Format", "Format");

            dgvHistory.Columns.Add("Date", "Downloaded");

            dgvHistory.Columns.Add("Path", "File Path");

            dgvHistory.Columns["Path"].Width = 450;

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

            dgvHistory.CellBorderStyle =
    DataGridViewCellBorderStyle.SingleHorizontal;

            dgvHistory.GridColor =
                Color.FromArgb(40, 50, 70);

            dgvHistory.ColumnHeadersBorderStyle =
                DataGridViewHeaderBorderStyle.None;

            dgvHistory.DefaultCellStyle.SelectionBackColor =
    Color.FromArgb(0, 120, 255);

            dgvHistory.DefaultCellStyle.SelectionForeColor =
                Color.White;

            dgvHistory.DefaultCellStyle.Padding =
                new Padding(2);

            dgvHistory.DefaultCellStyle.Font =
                new Font("Segoe UI", 10);

            dgvHistory.ColumnHeadersHeight = 40;

            dgvHistory.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 10, FontStyle.Bold);

            dgvHistory.CellClick += DgvHistory_CellClick;

            historyPanel.Controls.Add(dgvHistory);
        }

        private void LoadHistory()
        {
            dgvHistory.Rows.Clear();

            var query =
                db.DownloadHistories
                .OrderByDescending(x => x.DownloadedAt)
                .AsQueryable();

            string search =
                txtHistorySearch.Text
                .Trim()
                .ToLower();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.Title.ToLower().Contains(search));
            }

            string filter =
                cmbHistoryFilter.Text;

            if (filter == "MP3")
            {
                query = query.Where(x =>
                    x.Format.Contains("MP3"));
            }

            if (filter == "MP4")
            {
                query = query.Where(x =>
                    x.Format.Contains("MP4"));
            }

            foreach (var item in query.ToList())
            {
                dgvHistory.Rows.Add(
                    item.Title,
                    item.Format,
                    item.DownloadedAt
                        .ToString("yyyy-MM-dd HH:mm"),
                    item.FilePath
                );
            }

            StyleHistoryButtons();
        }

        private void StyleHistoryButtons()
        {
            foreach (DataGridViewRow row in dgvHistory.Rows)
            {
                StyleHistoryButtonCell(row, "OpenColumn");

                StyleHistoryButtonCell(row, "FolderColumn");
            }
        }

        private void StyleHistoryButtonCell(
            DataGridViewRow row,
            string columnName)
        {
            DataGridViewCell cell =
                row.Cells[columnName];

            cell.Style.BackColor =
                Color.FromArgb(0, 120, 255);

            cell.Style.ForeColor =
                Color.White;

            cell.Style.SelectionBackColor =
                Color.FromArgb(0, 100, 220);

            cell.Style.SelectionForeColor =
                Color.White;

            cell.Style.Font =
                new Font("Segoe UI", 9, FontStyle.Bold);

            cell.Style.Padding =
                new Padding(3);

            cell.Style.Alignment =
                DataGridViewContentAlignment.MiddleCenter;
        }

        private void BtnDownloads_Click(
    object? sender,
    EventArgs e)
        {
            if (historyPanel == null)
                return;

            queuePanel.Visible = false;

            previewCard.Visible = false;

            urlSection.Visible = false;

            historyPanel.Visible = true;

            LoadHistory();
        }

        private void DgvHistory_CellClick(
    object? sender,
    DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            string path =
                dgvHistory.Rows[e.RowIndex]
                .Cells["Path"]
                .Value?
                .ToString() ?? "";

            string column =
                dgvHistory.Columns[e.ColumnIndex]
                .Name;

            if (column == "OpenColumn")
            {
                if (File.Exists(path))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true
                    });
                }
            }

            if (column == "FolderColumn")
            {
                if (File.Exists(path))
                {
                    Process.Start("explorer.exe",
                        $"/select,\"{path}\"");
                }
            }
        }

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
                {
                    queuePanel.Height =
                        contentPanel.ClientSize.Height -
                        queuePanel.Top -
                        30;
                }
            };

            Controls.Add(contentPanel);
        }

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

        private Button CreateNavButton(string text)
        {
            return new Button
            {
                Text = text,
                Width = 150,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(25, 35, 55)
            };
        }

        #endregion

        #region Title bar dragging

        private void TitleBar_MouseDown(object? sender, MouseEventArgs e)
        {
            isDragging = true;

            dragCursorPoint = Cursor.Position;
            dragFormPoint = Location;
        }

        private void TitleBar_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!isDragging)
                return;

            Point difference = Point.Subtract(Cursor.Position,
                new Size(dragCursorPoint));

            Location = Point.Add(dragFormPoint,
                new Size(difference));
        }

        private void TitleBar_MouseUp(object? sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        #endregion

        #region URL Section

        private void BuildUrlSection()
        {
            urlSection = new RoundedPanel
            {
                Width = ClientSize.Width - 50,
                Height = 190,
                BorderRadius = 25,
                BackColor = Color.FromArgb(18, 24, 38),
                Location = new Point(25, 25),
                Anchor = AnchorStyles.Top |
         AnchorStyles.Left |
         AnchorStyles.Right
            };

            contentPanel.Controls.Add(urlSection);

            BuildUrlControls();
        }

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

            LoadSavedFolder();
        }

        private void BtnBrowseFolder_Click(
    object? sender,
    EventArgs e)
        {
            using FolderBrowserDialog dialog = new();

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            txtDownloadFolder.Text =
                dialog.SelectedPath;

            SaveFolderSettings(dialog.SelectedPath);
        }

        private void SaveFolderSettings(string path)
        {
            var settings =
                db.Settings.FirstOrDefault();

            if (settings == null)
            {
                settings = new AppSettings();

                db.Settings.Add(settings);
            }

            settings.VideoFolder =
                Path.Combine(path, "Video");

            settings.AudioFolder =
                Path.Combine(path, "Audio");

            db.SaveChanges();
        }

        private void LoadSavedFolder()
        {
            var settings =
                db.Settings.FirstOrDefault();

            if (settings == null)
            {
                string defaultPath =
                    Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.UserProfile),
                        "Downloads",
                        "YouTube Downloader");

                SaveFolderSettings(defaultPath);

                txtDownloadFolder.Text = defaultPath;

                return;
            }

            txtDownloadFolder.Text =
                Path.GetDirectoryName(
                    settings.VideoFolder) ?? "";
        }

        private string GetDownloadFolder(
    string format)
        {
            var settings =
                db.Settings.First();

            string folder =
                format.Contains("MP3")
                ? settings.AudioFolder
                : settings.VideoFolder;

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            return folder;
        }


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

            cmbFormat.Items.AddRange(new[]
            {
                "MP4 Video",
                "MP3 Audio"
            });

            cmbFormat.SelectedIndex = 0;

            urlSection.Controls.Add(cmbFormat);
        }

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

            cmbQuality.Items.AddRange(new[]
            {
                "Best Quality",
                "1080p",
                "720p",
                "480p"
            });

            cmbQuality.SelectedIndex = 0;

            urlSection.Controls.Add(cmbQuality);
        }

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

        #region Preview Card

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
                Anchor = AnchorStyles.Top |
         AnchorStyles.Left |
         AnchorStyles.Right
            };

            contentPanel.Controls.Add(previewCard);

            BuildPreviewControls();
        }

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

        #endregion

        #region Queue

        private void BuildQueuePanel()
        {
            queuePanel = new RoundedPanel
            {
                Width = ClientSize.Width - 50,
                Height = 250,
                BorderRadius = 25,
                BackColor = Color.FromArgb(18, 24, 38),
                Location = new Point(25, 490),
                Anchor = AnchorStyles.Top |
         AnchorStyles.Bottom |
         AnchorStyles.Left |
         AnchorStyles.Right
            };

            contentPanel.Controls.Add(queuePanel);

            queuePanel.Anchor =
    AnchorStyles.Top |
    AnchorStyles.Bottom |
    AnchorStyles.Left |
    AnchorStyles.Right;

            queuePanel.Height =
    contentPanel.ClientSize.Height -
    queuePanel.Top -
    footerPanel.Height -
    20;

            BuildQueueHeader();

            BuildQueueGrid();
        }

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
                Anchor = AnchorStyles.Top |
         AnchorStyles.Bottom |
         AnchorStyles.Left |
         AnchorStyles.Right
            };

            dgvQueue.RowTemplate.Height = 38;

            dgvQueue.CellBorderStyle =
                DataGridViewCellBorderStyle.SingleHorizontal;

            dgvQueue.ColumnHeadersBorderStyle =
                DataGridViewHeaderBorderStyle.None;

            dgvQueue.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(25, 35, 50);

            dgvQueue.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;

            dgvQueue.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 10, FontStyle.Bold);

            dgvQueue.ColumnHeadersHeight = 40;

            dgvQueue.DefaultCellStyle.BackColor =
                Color.FromArgb(18, 24, 38);

            dgvQueue.DefaultCellStyle.ForeColor =
                Color.White;

            dgvQueue.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(0, 120, 255);

            dgvQueue.DefaultCellStyle.SelectionForeColor =
                Color.White;

            dgvQueue.DefaultCellStyle.Font =
                new Font("Segoe UI", 10);

            dgvQueue.DefaultCellStyle.Padding =
                new Padding(2);

            DataGridViewHelper.EnableDoubleBuffering(dgvQueue);

            AddQueueColumns();

            foreach (DataGridViewColumn column in dgvQueue.Columns)
            {
                if (column is not DataGridViewButtonColumn)
                {
                    column.ReadOnly = true;
                }
            }

            queueBindingSource.DataSource = queueItems;

            dgvQueue.DataSource = queueBindingSource;

            dgvQueue.CellContentClick += DgvQueue_CellContentClick;

            dgvQueue.CellClick += DgvQueue_CellContentClick;

            dgvQueue.CellFormatting += DgvQueue_CellFormatting;

            queuePanel.Controls.Add(dgvQueue);
        }

        private void DgvQueue_CellFormatting(
    object? sender,
    DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            if (dgvQueue.Columns[e.ColumnIndex].Name == "PauseColumn")
            {
                DownloadItem item = queueItems[e.RowIndex];

                e.Value = item.IsPaused
                    ? "Resume"
                    : "Pause";
            }
        }

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

        #endregion

        #region Logic

        private string GetDefaultDownloadFolder(
    string format)
        {
            string downloads =
                Environment.GetFolderPath(
                    Environment.SpecialFolder.UserProfile);

            string baseFolder =
                Path.Combine(
                    downloads,
                    "Downloads",
                    "YouTube Downloader");

            string finalFolder =
                format.Contains("MP3")
                ? Path.Combine(baseFolder, "Audio")
                : Path.Combine(baseFolder, "Video");

            if (!Directory.Exists(finalFolder))
            {
                Directory.CreateDirectory(finalFolder);
            }

            return finalFolder;
        }

        private async void TxtUrl_TextChanged(
    object? sender,
    EventArgs e)
        {
            string url = txtUrl.Text.Trim();

            previewCts?.Cancel();

            previewCts = new CancellationTokenSource();

            CancellationToken token = previewCts.Token;

            try
            {
                await Task.Delay(700, token);

                if (token.IsCancellationRequested)
                    return;

                if (string.IsNullOrWhiteSpace(url))
                {
                    ClearPreview();
                    return;
                }

                if (!url.Contains("youtube.com") &&
    !url.Contains("youtu.be"))
                {
                    ShowPreviewError("Invalid YouTube URL");
                    return;
                }

                ShowPreviewLoading(true);

                await LoadVideoPreviewAsync(url);

                ShowPreviewLoading(false);
            }
            catch (TaskCanceledException)
            {
            }
            catch(Exception ex)
            {
                ShowPreviewLoading(false);

                MessageBox.Show(ex.Message);
            }
        }

        private void ShowPreviewLoading(bool loading)
        {
            lblLoading.Visible = loading;

            previewLoader.Visible = loading;

            txtUrl.Enabled = !loading;

            cmbFormat.Enabled = !loading;

            cmbQuality.Enabled = !loading;

            btnAddQueue.Enabled = !loading;

            Cursor = loading
                ? Cursors.WaitCursor
                : Cursors.Default;
        }

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

                bool isPlaylist =
                    url.Contains("list=");

                if (isPlaylist)
                {
                    var playlist =
                        await youtubeService
                        .GetPlaylistInfoAsync(url);

                    if (playlist == null)
                    {
                        ShowPreviewError(
                            "Unable to load playlist.");
                        return;
                    }

                    lblVideoTitle.Text =
                        playlist.Title;

                    lblAuthor.Text =
                        $"Channel: {playlist.Author}";

                    lblDuration.Text =
                        $"{playlist.VideoCount} videos";

                    lblPublishDate.Text =
                        "Playlist";

                    await LoadThumbnailAsync(playlist.ThumbnailUrl);

                    return;
                }

                var info =
                    await youtubeService
                    .GetVideoInfoAsync(url);

                if (info == null)
                {
                    ShowPreviewError(
                        "Unable to load video.");
                    return;
                }

                lblVideoTitle.Text =
                    info.Title;

                lblAuthor.Text =
                    $"Channel: {info.Author}";

                lblDuration.Text =
                    $"Duration: {info.Duration:mm\\:ss}";

                lblPublishDate.Text =
                    $"Published: {info.UploadDate:yyyy-MM-dd}";

                await LoadThumbnailAsync(info.ThumbnailUrl);
            }
            catch (Exception ex)
            {
                ShowPreviewError(ex.Message);
            }
        }

        private async Task LoadThumbnailAsync(string url)
        {
            try
            {
                using HttpClient client = new();

                byte[] bytes = await client.GetByteArrayAsync(url);

                using MemoryStream ms = new(bytes);

                Image img = Image.FromStream(ms);

                if (picThumbnail.Image != null)
                {
                    picThumbnail.Image.Dispose();
                }

                picThumbnail.Image = new Bitmap(img);
            }
            catch
            {
                picThumbnail.Image = null;
            }
        }

        private void ShowPreviewError(string message)
        {
            previewCard.Visible = true;

            lblVideoTitle.Text = message;

            lblAuthor.Text = "";

            lblDuration.Text = "";

            lblPublishDate.Text = "";

            picThumbnail.Image = null;
        }

        private async void BtnAddQueue_Click(
    object? sender,
    EventArgs e)
        {
            string url = txtUrl.Text.Trim();

            bool isPlaylist =
                url.Contains("list=");

            if (isPlaylist)
            {
                var videos =
                    await youtubeService
                    .GetPlaylistVideosAsync(url);

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

            lblQueueCount.Text =
                queueItems.Count.ToString();

            queueBindingSource.ResetBindings(false);

            StyleGridButtons();

            txtUrl.Clear();

            ClearPreview();
        }

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

        private async Task AddPlaylistToQueueAsync()
        {
            try
            {
                btnAddQueue.Enabled = false;

                btnAddQueue.Text = "Loading Playlist...";

                string playlistUrl = txtUrl.Text.Trim();

                var videos =
                    await youtubeService
                    .GetPlaylistVideosAsync(playlistUrl);
                Cursor = Cursors.WaitCursor;

                foreach (var video in videos)
                {
                    DownloadItem item = new()
                    {
                        Id = queueItems.Count + 1,

                        Title = video.Title,

                        Url = video.Url,

                        Format = cmbFormat.Text,

                        Quality = cmbQuality.Text,

                        Status = "Playlist Queued",

                        Progress = 0
                    };

                    queueItems.Add(item);

                    int rowIndex = dgvQueue.Rows.Count - 1;

                    dgvQueue.Rows[rowIndex]
                        .Cells["PauseColumn"].Value = "Pause";

                    item.Title = "[Playlist] " + video.Title;
                }

                lblQueueCount.Text =
                    queueItems.Count.ToString();

                MessageBox.Show(
                    $"{videos.Count} videos added to queue.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;

                btnAddQueue.Enabled = true;

                btnAddQueue.Text = "Add To Queue";

                txtUrl.Clear();

                previewCard.Visible = false;
            }
        }

        private async void BtnStartDownloads_Click(
    object? sender,
    EventArgs e)
        {
            List<Task> tasks = new();

            foreach (var item in queueItems)
            {
                if (item.Status == "Completed" ||
                    item.Status == "Downloading")
                {
                    continue;
                }

                item.DownloadFolder =
    GetDownloadFolder(item.Format);

                tasks.Add(QueueDownloadAsync(item));
            }

            await Task.WhenAll(tasks);

            MessageBox.Show(
                "Downloads completed.",
                "YouTube Downloader",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

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
        private void StyleGridButtons()
        {
            foreach (DataGridViewRow row in dgvQueue.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell is DataGridViewButtonCell buttonCell)
                    {
                        buttonCell.Style.BackColor =
                            Color.FromArgb(0, 120, 255);

                        buttonCell.Style.ForeColor =
                            Color.White;

                        buttonCell.Style.SelectionBackColor =
                            Color.FromArgb(0, 100, 220);

                        buttonCell.Style.SelectionForeColor =
                            Color.White;

                        buttonCell.Style.Font =
                            new Font("Segoe UI", 9, FontStyle.Bold);
                    }
                }
            }
        }
        private async Task StartDownloadAsync(DownloadItem item)
        {
            CancellationTokenSource cts = new();

            cancellationTokens[item.Id] = cts;

            try
            {
                item.Status = "Downloading";

                queueBindingSource.ResetBindings(false);
                StyleGridButtons();
                string extension =
                    item.Format.Contains("MP3")
                    ? ".mp3"
                    : ".mp4";

                string safeTitle = string.Join("_",
                    item.Title.Split(Path.GetInvalidFileNameChars()));

                string filePath =
                    Path.Combine(item.DownloadFolder,
                    $"{safeTitle}{extension}");

                Progress<double> progress = new(value =>
                {
                    if (item.IsCancelled)
                    {
                        cts.Cancel();
                        return;
                    }

                    if (item.IsPaused)
                    {
                        return;
                    }

                    int newProgress = (int)value;

                    if (newProgress < item.Progress)
                        return;

                    item.Progress = newProgress;

                    BeginInvoke(() =>
                    {
                        queueBindingSource.ResetBindings(false);
                    });
                });

                if (item.Format.Contains("MP3"))
                {
                    await downloadService.DownloadAudioAsync(
                        item.Url,
                        filePath,
                        progress,
                        cts.Token);
                }
                else
                {
                    await downloadService.DownloadVideoAsync(
                        item.Url,
                        filePath,
                        progress,
                        cts.Token);
                }

                if (!item.IsCancelled)
                {
                    item.Progress = 100;

                    db.DownloadHistories.Add(
    new DownloadHistory
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

                BeginInvoke(() =>
                {
                    queueBindingSource.ResetBindings(false);
                });
            }
            catch (OperationCanceledException)
            {
                if (item.IsCancelled)
                {
                    item.Status = "Cancelled";
                }

                BeginInvoke(() =>
                {
                    queueBindingSource.ResetBindings(false);
                });
            }
            catch (Exception ex)
            {
                item.Status = "Failed";

                MessageBox.Show(ex.Message);

                BeginInvoke(() =>
                {
                    queueBindingSource.ResetBindings(false);
                });
            }
        }

        private void TogglePause(DownloadItem item)
        {
            item.IsPaused = !item.IsPaused;

            item.Status =
                item.IsPaused
                ? "Paused"
                : "Downloading";

            queueBindingSource.ResetBindings(false);

            dgvQueue.Refresh();
        }

        private void CancelDownload(DownloadItem item)
        {
            item.IsCancelled = true;

            if (cancellationTokens.ContainsKey(item.Id))
            {
                cancellationTokens[item.Id].Cancel();
            }

            queueItems.Remove(item);

            for (int i = 0; i < queueItems.Count; i++)
            {
                queueItems[i].Id = i + 1;
            }

            lblQueueCount.Text = queueItems.Count.ToString();

            queueBindingSource.ResetBindings(false);

            RefreshQueueGrid();
        }

        private void RefreshQueueGrid()
        {
            if (InvokeRequired)
            {
                Invoke(() =>
                {
                    dgvQueue.Refresh();
                });
            }
            else
            {
                dgvQueue.Refresh();
            }
        }

        private void DgvQueue_CellContentClick(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            DownloadItem item = queueItems[e.RowIndex];

            string columnName =
                dgvQueue.Columns[e.ColumnIndex].Name;

            if (columnName == "PauseColumn")
            {
                TogglePause(item);
            }

            if (columnName == "CancelColumn")
            {
                CancelDownload(item);
            }
        }

        #endregion
    }
}