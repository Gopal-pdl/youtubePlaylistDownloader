using System;
using System.Diagnostics;
using System.IO;
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

        private string downloadFolder;

        public Form1()
        {
            InitializeComponent();
            InitializeCustomControls();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Set default download folder to My Music
            downloadFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            txtDownloadFolder.Text = downloadFolder;
        }

        private void InitializeCustomControls()
        {
            // Playlist URL
            var lblUrl = new Label { Text = "Playlist URL:", Location = new System.Drawing.Point(10, 10), Width = 80 };
            txtPlaylistUrl = new TextBox { Width = 400, Location = new System.Drawing.Point(100, 10) };

            // Download Folder
            var lblFolder = new Label { Text = "Download Folder:", Location = new System.Drawing.Point(10, 40), Width = 100 };
            txtDownloadFolder = new TextBox { Width = 320, Location = new System.Drawing.Point(120, 40), ReadOnly = true };
            btnSelectFolder = new Button { Text = "Browse...", Location = new System.Drawing.Point(450, 40), Width = 80 };
            btnSelectFolder.Click += BtnSelectFolder_Click;

            // Download Button
            btnDownload = new Button { Text = "Download Playlist as MP3", Location = new System.Drawing.Point(10, 75), Width = 200 };
            btnDownload.Click += BtnDownload_Click;

            // Progress Bar
            progressBar = new ProgressBar { Width = 520, Location = new System.Drawing.Point(10, 110) };

            // Status Label
            lblStatus = new Label { Width = 520, Location = new System.Drawing.Point(10, 140) };

            // Downloads List
            lstDownloads = new ListBox { Width = 520, Height = 200, Location = new System.Drawing.Point(10, 170) };

            Controls.Add(lblUrl);
            Controls.Add(txtPlaylistUrl);
            Controls.Add(lblFolder);
            Controls.Add(txtDownloadFolder);
            Controls.Add(btnSelectFolder);
            Controls.Add(btnDownload);
            Controls.Add(progressBar);
            Controls.Add(lblStatus);
            Controls.Add(lstDownloads);

            this.Width = 560;
            this.Height = 430;
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
            lstDownloads.Items.Clear();
            lblStatus.Text = "Fetching playlist...";
            var playlistUrl = txtPlaylistUrl.Text;
            var youtube = new YoutubeClient();

            try
            {
                var playlist = await youtube.Playlists.GetVideosAsync(playlistUrl);
                int total = playlist.Count;
                int current = 0;
                progressBar.Maximum = total;
                progressBar.Value = 0;

                foreach (var video in playlist)
                {
                    string fileName = $"{SanitizeFileName(video.Title)}.mp3";
                    string tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.mp4");
                    string outputFile = Path.Combine(downloadFolder, fileName);

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
            }
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
