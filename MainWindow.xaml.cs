using LibVLCSharp.Shared;
using Microsoft.Win32;
using System;
using System.Linq;
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

                // Parse the media so we can retrieve track information
                await media.Parse(MediaParseOptions.ParseLocal);

                // Grab audio tracks
                var tracks = media.Tracks
                    .Where(t => t.TrackType == TrackType.Audio)
                    .ToArray();

                _audioTrackViews = tracks
                    .Select(t => new MediaTrackView(t))
                    .ToArray();

                AudioTracksComboBox.ItemsSource = _audioTrackViews;

                if (_audioTrackViews.Any())
                    AudioTracksComboBox.SelectedIndex = 0;

                // Assign the parsed media to the player and start playing
                _mediaPlayer.Media = media;
                _mediaPlayer.Play();

                // ------------------------------------------
                // NEW: Retrieve subtitle tracks and list them
                // ------------------------------------------
                var subtitles = _mediaPlayer.SpuDescription;
                if (subtitles != null && subtitles.Any())
                {
                    SubtitlesComboBox.ItemsSource = subtitles;
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

        private void MediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            // Update time display using Dispatcher to interact with the UI thread
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
    }

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
}
