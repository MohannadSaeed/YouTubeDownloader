using System.ComponentModel;
using YouTubeDownloader.Data;
using YouTubeDownloader.Helpers;
using YouTubeDownloader.Models;
using YouTubeDownloader.Services;
using YouTubeDownloader.Forms;

namespace YouTubeDownloader.Forms
{
    /// <summary>
    /// Main form of the YouTube Downloader application.
    /// Handles UI layout, user interactions, and orchestrates download operations.
    /// </summary>
    public partial class MainForm : Form
    {
        #region Fields

        // UI Panels
        private Panel titleBar = null!;
        private Panel navigationBar = null!;
        private Panel contentPanel = null!;
        private Panel footerPanel = null!;

        // Rounded container panels
        private RoundedPanel urlSection = null!;
        private RoundedPanel previewCard = null!;
        private RoundedPanel queuePanel = null!;

        // Input controls
        private TextBox txtUrl = null!;
        private ComboBox cmbFormat = null!;
        private ComboBox cmbQuality = null!;
        private Button btnAddQueue = null!;
        private Button btnStartDownloads = null!;
        private PictureBox picThumbnail = null!;
        private TextBox txtDownloadFolder = null!;
        private Button btnBrowseFolder = null!;

        // Labels for video preview
        private Label lblVideoTitle = null!;
        private Label lblAuthor = null!;
        private Label lblDuration = null!;
        private Label lblPublishDate = null!;
        private Label lblQueueCount = null!;

        // Queue grid
        private DataGridView dgvQueue = null!;

        // Data binding
        private readonly BindingSource queueBindingSource = new();
        private readonly BindingList<DownloadItem> queueItems = new();

        // Services
        private readonly YouTubeService youtubeService = new();
        private readonly DownloadService downloadService = new();

        // Concurrency control (max 3 concurrent downloads)
        private readonly SemaphoreSlim downloadSemaphore = new(3);

        // Track cancellation tokens per download item
        private readonly Dictionary<int, CancellationTokenSource> cancellationTokens = new();

        // Database context for settings and history
        private readonly AppDbContext db = new();

        // Download folder path
        private string downloadFolder = "";

        // Title bar dragging
        private bool isDragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        // Preview loading indicators
        private Label lblLoading = null!;
        private ProgressBar previewLoader = null!;
        private CancellationTokenSource? previewCts;

        // History panel components
        private Panel historyPanel = null!;
        private DataGridView dgvHistory = null!;
        private TextBox txtHistorySearch = null!;
        private ComboBox cmbHistoryFilter = null!;
        private Button btnHome = null!;
        private Button btnDownloads = null!;


        // Settings service instance
        private readonly SettingsService settingsService = new();
        private AppSettings appSettings = null!;

        // Settings Panel components
        private RoundedPanel settingsPanel = null!;
        private TextBox txtVideoFolder = null!;
        private TextBox txtAudioFolder = null!;
        private NumericUpDown numConcurrentDownloads = null!;
        private CheckBox chkShowNotifications = null!;
        private CheckBox chkAutoPasteClipboard = null!;
        private CheckBox chkMinimizeTray = null!;
        private Button btnSaveSettings = null!;
        private Button btnSettings = null!;

        // Auto Paste Clipboard Timer
        private System.Windows.Forms.Timer clipboardTimer = null!;
        private string lastClipboardText = "";

        #endregion

        /// <summary>
        /// Initializes a new instance of the MainForm.
        /// Sets up the database, configures the form, and builds the UI layout.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            //db.Database.EnsureCreated(); // Create database if not exists
            appSettings = settingsService.GetSettings();
            ConfigureForm();
            BuildLayout();
        }

        /// <summary>
        /// Configures basic form properties like size, border style, and colors.
        /// </summary>
        private void ConfigureForm()
        {
            Text = "YouTube Downloader";
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1650, 950);
            Size = new Size(1650, 950);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(10, 15, 25); // Dark background
        }
    }
}