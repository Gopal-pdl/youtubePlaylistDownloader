using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.Streams;

namespace youtubePlaylistDownloader
{
    public partial class Form1 : Form
    {
        private TextBox txtPlaylistUrl;
        private Button btnDownload;
        private ProgressBar progressBar;
        private Label lblStatus;
        private Button btnSelectFolder;
        private TextBox txtDownloadFolder;
        private ListBox lstDownloads;
        private ListBox lstSimilarFiles;
        private Button btnStop;
        private NumericUpDown numDownloadCount;
        private Label lblDownloadCount;

        private string downloadFolder;
        private CancellationTokenSource cancellationTokenSource;

        public Form1()
        {
            InitializeComponent();
            InitializeCustomControls();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            downloadFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            txtDownloadFolder.Text = downloadFolder;
        }

        private void InitializeCustomControls()
        {
            // Modern color palette
            var backColor = System.Drawing.Color.FromArgb(34, 40, 49);      // Dark background
            var accentColor = System.Drawing.Color.FromArgb(0, 173, 181);   // Accent (buttons)
            var textColor = System.Drawing.Color.White;                     // Foreground
            var boxColor = System.Drawing.Color.FromArgb(57, 62, 70);       // Input boxes

            this.BackColor = backColor;

            // Playlist URL
            var lblUrl = new Label
            {
                Text = "Playlist URL:",
                Location = new System.Drawing.Point(10, 10),
                Width = 100,
                ForeColor = accentColor,
                Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold)
            };
            txtPlaylistUrl = new TextBox
            {
                Width = 400,
                Location = new System.Drawing.Point(120, 10),
                BackColor = boxColor,
                ForeColor = textColor,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new System.Drawing.Font("Segoe UI", 10)
            };

            // Download Folder
            var lblFolder = new Label
            {
                Text = "Download Folder:",
                Location = new System.Drawing.Point(10, 45),
                Width = 120,
                ForeColor = accentColor,
                Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold)
            };
            txtDownloadFolder = new TextBox
            {
                Width = 320,
                Location = new System.Drawing.Point(140, 45),
                ReadOnly = true,
                BackColor = boxColor,
                ForeColor = textColor,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new System.Drawing.Font("Segoe UI", 9)
            };
            btnSelectFolder = new Button
            {
                Text = "Browse...",
                Location = new System.Drawing.Point(470, 45),
                Width = 60,
                BackColor = accentColor,
                ForeColor = backColor,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold)
            };
            btnSelectFolder.FlatAppearance.BorderSize = 0;
            btnSelectFolder.Click += BtnSelectFolder_Click;

            // Download Count
            lblDownloadCount = new Label
            {
                Text = "Number of downloads:",
                Location = new System.Drawing.Point(10, 80),
                Width = 150,
                ForeColor = accentColor,
                Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold)
            };
            numDownloadCount = new NumericUpDown
            {
                Location = new System.Drawing.Point(170, 80),
                Width = 60,
                Minimum = 1,
                Maximum = 100,
                Value = 10,
                BackColor = boxColor,
                ForeColor = textColor,
                Font = new System.Drawing.Font("Segoe UI", 10)
            };

            // Download Button
            btnDownload = new Button
            {
                Text = "Download Playlist as MP3",
                Location = new System.Drawing.Point(240, 80),
                Width = 180,
                BackColor = accentColor,
                ForeColor = backColor,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold)
            };
            btnDownload.FlatAppearance.BorderSize = 0;
            btnDownload.Click += BtnDownload_Click;

            // Stop Button
            btnStop = new Button
            {
                Text = "Stop",
                Location = new System.Drawing.Point(430, 80),
                Width = 100,
                BackColor = System.Drawing.Color.FromArgb(238, 59, 59),
                ForeColor = backColor,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold),
                Enabled = false
            };
            btnStop.FlatAppearance.BorderSize = 0;
            btnStop.Click += BtnStop_Click;

            // Progress Bar
            progressBar = new ProgressBar
            {
                Width = 520,
                Location = new System.Drawing.Point(10, 120),
                BackColor = boxColor,
                ForeColor = accentColor
            };

            // Status Label
            lblStatus = new Label
            {
                Width = 520,
                Location = new System.Drawing.Point(10, 150),
                ForeColor = accentColor,
                Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold)
            };

            // Downloads List
            var lblDownloads = new Label
            {
                Text = "Downloads:",
                Location = new System.Drawing.Point(10, 175),
                Width = 100,
                ForeColor = accentColor,
                Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold)
            };
            lstDownloads = new ListBox
            {
                Width = 250,
                Height = 200,
                Location = new System.Drawing.Point(10, 200),
                BackColor = boxColor,
                ForeColor = textColor,
                Font = new System.Drawing.Font("Segoe UI", 10)
            };

            // Similar Files List
            var lblSimilar = new Label
            {
                Text = "Similar Files (Review):",
                Location = new System.Drawing.Point(280, 175),
                Width = 180,
                ForeColor = accentColor,
                Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold)
            };
            lstSimilarFiles = new ListBox
            {
                Width = 250,
                Height = 200,
                Location = new System.Drawing.Point(280, 200),
                BackColor = boxColor,
                ForeColor = textColor,
                Font = new System.Drawing.Font("Segoe UI", 10)
            };

            Controls.Add(lblUrl);
            Controls.Add(txtPlaylistUrl);
            Controls.Add(lblFolder);
            Controls.Add(txtDownloadFolder);
            Controls.Add(btnSelectFolder);
            Controls.Add(lblDownloadCount);
            Controls.Add(numDownloadCount);
            Controls.Add(btnDownload);
            Controls.Add(btnStop);
            Controls.Add(progressBar);
            Controls.Add(lblStatus);
            Controls.Add(lblDownloads);
            Controls.Add(lstDownloads);
            Controls.Add(lblSimilar);
            Controls.Add(lstSimilarFiles);

            this.Width = 560;
            this.Height = 470;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
        }


        private void BtnSelectFolder_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.SelectedPath = downloadFolder;
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    downloadFolder = fbd.SelectedPath;
                    txtDownloadFolder.Text = downloadFolder;
                }
            }
        }

        private async void BtnDownload_Click(object sender, EventArgs e)
        {
            btnDownload.Enabled = false;
            btnSelectFolder.Enabled = false;
            txtPlaylistUrl.Enabled = false;
            numDownloadCount.Enabled = false;
            btnStop.Enabled = true;
            lstDownloads.Items.Clear();
            lstSimilarFiles.Items.Clear();
            lblStatus.Text = "Fetching playlist...";
            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            var playlistUrl = txtPlaylistUrl.Text;
            var youtube = new YoutubeClient();

            try
            {
                var playlist = await youtube.Playlists.GetVideosAsync(playlistUrl);
                int totalToDownload = (int)numDownloadCount.Value;
                int total = Math.Min(totalToDownload, playlist.Count);
                int current = 0;
                progressBar.Maximum = total;
                progressBar.Value = 0;

                foreach (var video in playlist.Take(total))
                {
                    if (token.IsCancellationRequested)
                    {
                        lblStatus.Text = "Download stopped by user.";
                        break;
                    }

                    string fileName = $"{SanitizeFileName(video.Title)}.mp3";
                    string outputFile = Path.Combine(downloadFolder, fileName);

                    // Check for similar files
                    var similarFile = FindSimilarFile(fileName, downloadFolder);
                    if (similarFile != null)
                    {
                        lstSimilarFiles.Items.Add($"Skip: {fileName} (Similar: {Path.GetFileName(similarFile)})");
                        continue;
                    }

                    string tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.mp4");

                    lstDownloads.Items.Add($"Downloading: {video.Title}");
                    lblStatus.Text = $"Downloading: {video.Title}";

                    // Get the best audio stream
                    var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
                    var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

                    // Download audio stream
                    await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, tempFile);

                    // Convert to MP3 using FFmpeg
                    await ConvertToMp3Async(tempFile, outputFile);

                    File.Delete(tempFile);

                    lstDownloads.Items[lstDownloads.Items.Count - 1] = $"Downloaded: {video.Title}";
                    current++;
                    progressBar.Value = current;
                }

                lblStatus.Text = $"Download complete! Files saved to: {downloadFolder}";
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error: {ex.Message}";
            }
            finally
            {
                btnDownload.Enabled = true;
                btnSelectFolder.Enabled = true;
                txtPlaylistUrl.Enabled = true;
                numDownloadCount.Enabled = true;
                btnStop.Enabled = false;
            }
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            cancellationTokenSource?.Cancel();
            lblStatus.Text = "Stopping download...";
        }

        // Checks for similar files in the download folder (≥50% word match)
        private string FindSimilarFile(string fileName, string folder)
        {
            var files = Directory.GetFiles(folder, "*.mp3");
            var fileWords = GetWords(Path.GetFileNameWithoutExtension(fileName));

            foreach (var file in files)
            {
                var existingWords = GetWords(Path.GetFileNameWithoutExtension(file));
                double match = WordMatchPercentage(fileWords, existingWords);
                if (match >= 0.5)
                {
                    return file;
                }
            }
            return null;
        }

        // Splits a string into lowercase words
        private string[] GetWords(string name)
        {
            return name.ToLower().Split(new[] { ' ', '_', '-', '.', '[', ']', '(', ')', ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        // Returns the percentage of matching words between two arrays
        private double WordMatchPercentage(string[] words1, string[] words2)
        {
            if (words1.Length == 0 || words2.Length == 0) return 0;
            int matchCount = words1.Count(w => words2.Contains(w));
            return (double)matchCount / Math.Max(words1.Length, words2.Length);
        }

        private async Task ConvertToMp3Async(string inputPath, string outputPath)
        {
            string ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");
            if (!File.Exists(ffmpegPath))
                throw new FileNotFoundException("ffmpeg.exe not found.");

            var psi = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-y -i \"{inputPath}\" -vn -ar 44100 -ac 2 -b:a 192k \"{outputPath}\"",
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using (var process = Process.Start(psi))
            {
                if (process == null)
                    throw new InvalidOperationException("Failed to start ffmpeg process.");

                await Task.Run(() => process.WaitForExit());
            }
        }

        private string SanitizeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }
    }
}
