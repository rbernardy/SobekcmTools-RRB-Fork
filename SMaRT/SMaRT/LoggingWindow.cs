#region Using directives

using System;
using System.Drawing;
using System.Windows.Forms;

#endregion

namespace SobekCM.Management_Tool
{
    /// <summary> Logging window that displays real-time verbose logging during operations </summary>
    public class LoggingWindow : Form
    {
        private static LoggingWindow instance;
        private static readonly object lockObj = new object();
        private RichTextBox logTextBox;
        private Button copyAllButton;
        private Button clearButton;
        private Button closeButton;

        /// <summary> Gets or sets whether logging is enabled </summary>
        // Production version constant (updated to the current version)
        private static readonly string ProductionVersion = "3.52.10";

        // Backing field for the LoggingEnabled property. Default is false so logging is off until the user enables it.
        private static bool _loggingEnabled = false;

        /// <summary>Gets or sets whether logging is enabled. When turned on, the production version is logged as the first entry.</summary>
        public static bool LoggingEnabled
        {
            get => _loggingEnabled;
            set
            {
                // Only act when the value actually changes
                if (_loggingEnabled != value)
                {
                    _loggingEnabled = value;
                    // If logging is being enabled, emit a log entry with the production version.
                    if (value)
                    {
                        Log($"Production version: {ProductionVersion}");
                    }
                }
            }
        }

        /// <summary> Constructor for a new instance of the LoggingWindow class </summary>
        public LoggingWindow()
        {
            InitializeComponent();
            // Position the logging window near the right edge of the primary screen, 25px from the right border
            this.StartPosition = FormStartPosition.Manual;
            // Calculate the X coordinate: screen width minus window width minus 25 pixels
            int x = Screen.PrimaryScreen.WorkingArea.Width - this.Width - 25;
            // Position the window 100 pixels from the top of the screen
            int y = 100;
            this.Location = new Point(x, y);
        }

        private void InitializeComponent()
        {
            this.logTextBox = new RichTextBox();
            this.copyAllButton = new Button();
            this.clearButton = new Button();
            this.closeButton = new Button();
            this.SuspendLayout();

            // logTextBox
            this.logTextBox.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));
            this.logTextBox.BackColor = Color.Black;
            this.logTextBox.ForeColor = Color.LightGreen;
            this.logTextBox.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.logTextBox.Location = new Point(12, 12);
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ReadOnly = true;
            this.logTextBox.Size = new Size(560, 338);
            this.logTextBox.TabIndex = 0;
            this.logTextBox.Text = "";
            // Enable word wrap so log entries automatically break to fit the current width of the popup window
            this.logTextBox.WordWrap = true;
            this.logTextBox.HideSelection = false; // Keep selection visible when control loses focus

            // copyAllButton
            this.copyAllButton.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.copyAllButton.Location = new Point(335, 356);
            this.copyAllButton.Name = "copyAllButton";
            this.copyAllButton.Size = new Size(75, 23);
            this.copyAllButton.TabIndex = 1;
            this.copyAllButton.Text = "Copy All";
            this.copyAllButton.UseVisualStyleBackColor = true;
            this.copyAllButton.Click += new EventHandler(this.copyAllButton_Click);

            // clearButton
            this.clearButton.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.clearButton.Location = new Point(416, 356);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new Size(75, 23);
            this.clearButton.TabIndex = 2;
            this.clearButton.Text = "Clear";
            this.clearButton.UseVisualStyleBackColor = true;
            this.clearButton.Click += new EventHandler(this.clearButton_Click);

            // closeButton
            this.closeButton.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.closeButton.Location = new Point(497, 356);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new Size(75, 23);
            this.closeButton.TabIndex = 3;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new EventHandler(this.closeButton_Click);

            // LoggingWindow
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(584, 391);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.clearButton);
            this.Controls.Add(this.copyAllButton);
            this.Controls.Add(this.logTextBox);
            this.MinimumSize = new Size(400, 300);
            this.Name = "LoggingWindow";
            // Position the logging window near the right edge of the primary screen, 100px from the right border
            this.Text = "SMaRT Logging Window";
            this.FormClosing += new FormClosingEventHandler(this.LoggingWindow_FormClosing);
            this.ResumeLayout(false);
        }

        private void copyAllButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(logTextBox.Text))
            {
                Clipboard.SetText(logTextBox.Text);
                MessageBox.Show("Log content copied to clipboard.", "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            logTextBox.Clear();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void LoggingWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Hide instead of close to preserve log content
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        /// <summary> Gets the singleton instance of the logging window </summary>
        public static LoggingWindow Instance
        {
            get
            {
                lock (lockObj)
                {
                    if (instance == null || instance.IsDisposed)
                    {
                        instance = new LoggingWindow();
                    }
                    return instance;
                }
            }
        }

        /// <summary> Adds a log entry with timestamp </summary>
        public static void Log(string message)
        {
            if (!LoggingEnabled) return;

            try
            {
                var window = Instance;
                string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                string logEntry = $"[{timestamp}] {message}{Environment.NewLine}";

                if (window.InvokeRequired)
                {
                    window.BeginInvoke(new Action(() => AppendLog(window, logEntry)));
                }
                else
                {
                    AppendLog(window, logEntry);
                }
            }
            catch
            {
                // Silently ignore logging errors
            }
        }

        private static void AppendLog(LoggingWindow window, string logEntry)
        {
            window.logTextBox.AppendText(logEntry);
            window.logTextBox.ScrollToCaret();
        }

        /// <summary> Shows the logging window </summary>
        public static void ShowWindow()
        {
            var window = Instance;
            if (!window.Visible)
            {
                window.Show();
            }
            window.BringToFront();
        }

        /// <summary> Hides the logging window </summary>
        public static void HideWindow()
        {
            if (instance != null && !instance.IsDisposed && instance.Visible)
            {
                instance.Hide();
            }
        }
    }
}
