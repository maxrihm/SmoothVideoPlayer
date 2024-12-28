using LibVLCSharp.Shared;
using Microsoft.Win32;
using SubtitlesParser.Classes.Parsers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmoothVideoPlayer
{
    public partial class MainWindow : Window
    {
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;

        private MediaTrackView[] _audioTrackViews = Array.Empty<MediaTrackView>();

        public MainWindow()
        {
            InitializeComponent();

            Core.Initialize();

            var libVlcOptions = new[]
            {
                "--file-caching=300",
                "--network-caching=300",
                "--live-caching=300",
                "--avcodec-hw=any",
                "--no-stats",
                "--no-plugins-cache"
            };

            _libVLC = new LibVLC(libVlcOptions);
            _mediaPlayer = new MediaPlayer(_libVLC)
            {
                EnableHardwareDecoding = true
            };

            videoView.MediaPlayer = _mediaPlayer;

            // Subscribe to LibVLC's TimeChanged event
            _mediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
        }

        /// <summary>
        /// Handles the "Open..." button click: lets user pick a file,
        /// plays it, then extracts *all* embedded subtitle tracks and
        /// populates the SubtitlesComboBox with them.
        /// </summary>
        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Video Files|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.flv;*.mpeg;*.mpg"
            };

            if (dlg.ShowDialog() == true)
            {
                _mediaPlayer.Stop();

                var media = new Media(_libVLC, new Uri(dlg.FileName));
                await media.Parse(MediaParseOptions.ParseLocal);

                // ------------------------------------------------
                // 1) Grab audio tracks
                // ------------------------------------------------
                var audioTracks = media.Tracks
                    .Where(t => t.TrackType == TrackType.Audio)
                    .ToArray();

                _audioTrackViews = audioTracks
                    .Select(t => new MediaTrackView(t))
                    .ToArray();

                AudioTracksComboBox.ItemsSource = _audioTrackViews;
                if (_audioTrackViews.Any())
                    AudioTracksComboBox.SelectedIndex = 0;

                // ------------------------------------------------
                // 2) Assign media to the player and play
                // ------------------------------------------------
                _mediaPlayer.Media = media;
                _mediaPlayer.Play();

                // ------------------------------------------------
                // 3) Grab *all* subtitle tracks (TrackType.Text)
                // ------------------------------------------------
                var subtitleTracks = media.Tracks
                    .Where(t => t.TrackType == TrackType.Text)
                    .ToArray();

                // We'll store a list of "SubtitleTrackView"
                var extractedTracks = new List<SubtitleTrackView>();

                // ------------------------------------------------
                // 4) Extract each subtitle track asynchronously
                // ------------------------------------------------
                for (int i = 0; i < subtitleTracks.Length; i++)
                {
                    var track = subtitleTracks[i];
                    // We'll attempt to extract track index = i
                    // If your video has more complicated track indexing,
                    // adapt as needed. Typically it's 0,1,2,... for subtitles.

                    try
                    {
                        string extractedPath = await ExtractSubtitlesForTrackAsync(dlg.FileName, i);
                        if (!string.IsNullOrEmpty(extractedPath) && File.Exists(extractedPath))
                        {
                            // ------------------------------------------------
                            // 5) Parse this .srt
                            // ------------------------------------------------
                            var parsedSubtitles = ParseSubtitles(extractedPath);

                            // ------------------------------------------------
                            // 6) Create a track view
                            // ------------------------------------------------
                            var trackName = $"Subtitle Track {i}";

                            // If there's a language or description, use that
                            if (!string.IsNullOrEmpty(track.Language))
                                trackName += $" ({track.Language})";

                            if (!string.IsNullOrEmpty(track.Description))
                                trackName += $" - {track.Description}";

                            extractedTracks.Add(new SubtitleTrackView
                            {
                                Name = trackName,
                                FilePath = extractedPath,
                                ParsedSubtitles = parsedSubtitles
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to extract track {i}: {ex.Message}");
                    }
                }

                // ------------------------------------------------
                // 7) Populate the ComboBox with all extracted tracks
                // ------------------------------------------------
                if (extractedTracks.Any())
                {
                    SubtitlesComboBox.ItemsSource = extractedTracks;
                    SubtitlesComboBox.SelectedIndex = 0;
                }
                else
                {
                    SubtitlesComboBox.ItemsSource = null;
                }
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (_mediaPlayer != null && !_mediaPlayer.IsPlaying)
            {
                _mediaPlayer.Play();
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Pause();
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Stop();

                // Reset time displays to zero
                CurrentTimeTextBlock.Text = "00:00:00";
                TotalTimeTextBlock.Text = "00:00:00";
            }
        }

        private void AudioTracksComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (AudioTracksComboBox.SelectedItem is MediaTrackView selectedTrackView)
            {
                _mediaPlayer.SetAudioTrack(selectedTrackView.Track.Id);
            }
        }

        /// <summary>
        /// Subtitles combo selection changed
        /// Here you can choose which subtitles to display or handle differently.
        /// Right now, it's just a placeholder example.
        /// </summary>
        private void SubtitlesComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (SubtitlesComboBox.SelectedItem is SubtitleTrackView selectedSub)
            {
                // Example: You might want to show the parsed lines in a debug
                // window or overlay them manually, etc.
                Debug.WriteLine($"Selected {selectedSub.Name} with {selectedSub.ParsedSubtitles.Count} subtitle blocks.");
            }
        }

        private void MediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            // Update time display on the UI thread
            Dispatcher.Invoke(() =>
            {
                var currentTime = TimeSpan.FromMilliseconds(e.Time);
                var totalTime = TimeSpan.FromMilliseconds(_mediaPlayer.Length);

                CurrentTimeTextBlock.Text = currentTime.ToString(@"hh\:mm\:ss");
                TotalTimeTextBlock.Text = totalTime.ToString(@"hh\:mm\:ss");
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            _mediaPlayer?.Dispose();
            _libVLC?.Dispose();
            base.OnClosed(e);
        }

        // ---------------------------------------------------------------------
        // EXTRACT AND PARSE SUBTITLES
        // ---------------------------------------------------------------------

        /// <summary>
        /// Extract a specific subtitle track (by index) from the input file
        /// using FFmpeg, to an .srt file. Runs on a background thread
        /// so the UI won't freeze.
        /// </summary>
        private async Task<string> ExtractSubtitlesForTrackAsync(string inputPath, int trackIndex)
        {
            var outputFolder = Path.GetDirectoryName(inputPath) ?? Environment.CurrentDirectory;
            var outputSrtPath = Path.Combine(outputFolder, $"extracted_track_{trackIndex}_{Guid.NewGuid():N}.srt");

            // Adjust if FFmpeg is not on your PATH
            const string ffmpegExecutable = "ffmpeg";

            // NOTE: We map the specific track index  (0:s:<trackIndex>)
            // E.g., if trackIndex=1, then we do -map 0:s:1
            var arguments = $"-i \"{inputPath}\" -map 0:s:{trackIndex} -c:s srt -y \"{outputSrtPath}\"";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = ffmpegExecutable,
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            await Task.Run(() =>
            {
                using (var process = new Process { StartInfo = processStartInfo })
                {
                    // Read standard output/error
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                            Debug.WriteLine($"[FFmpeg STDOUT] {e.Data}");
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                            Debug.WriteLine($"[FFmpeg STDERR] {e.Data}");
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                }
            });

            return File.Exists(outputSrtPath) ? outputSrtPath : null;
        }

        /// <summary>
        /// Parse an .srt file using SubtitlesParser and return
        /// a list of ParsedSubtitleItem.
        /// </summary>
        private List<ParsedSubtitleItem> ParseSubtitles(string srtPath)
        {
            var result = new List<ParsedSubtitleItem>();
            var parser = new SrtParser();

            using (var fs = File.OpenRead(srtPath))
            {
                var items = parser.ParseStream(fs, Encoding.UTF8);
                foreach (var item in items)
                {
                    result.Add(new ParsedSubtitleItem
                    {
                        StartTime = TimeSpan.FromMilliseconds(item.StartTime),
                        EndTime = TimeSpan.FromMilliseconds(item.EndTime),
                        Lines = item.Lines
                    });
                }
            }

            return result;
        }
    }

    // ---------------------------------------------------------------------
    // VIEW MODELS
    // ---------------------------------------------------------------------

    /// <summary>
    /// Holds the audio track info for the audio ComboBox.
    /// </summary>
    public class MediaTrackView
    {
        public MediaTrack Track { get; }
        public string DisplayName { get; }

        public MediaTrackView(MediaTrack track)
        {
            Track = track;
            DisplayName = !string.IsNullOrEmpty(track.Description)
                ? track.Description
                : !string.IsNullOrEmpty(track.Language)
                    ? track.Language
                    : $"Track {track.Id}";
        }
    }

    /// <summary>
    /// Represents one extracted subtitle track in the SubtitlesComboBox.
    /// </summary>
    public class SubtitleTrackView
    {
        public string Name { get; set; } = string.Empty;       // e.g. "Subtitle Track 0 (eng)"
        public string FilePath { get; set; } = string.Empty;   // path to the .srt file
        public List<ParsedSubtitleItem> ParsedSubtitles { get; set; } = new List<ParsedSubtitleItem>();
    }

    /// <summary>
    /// Represents one block of parsed subtitles (start time, end time, lines).
    /// </summary>
    public class ParsedSubtitleItem
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public IList<string> Lines { get; set; } = Array.Empty<string>();
    }
}
