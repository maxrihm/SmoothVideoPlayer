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

        // Keep a collection of 'MediaTrackView' to display proper names in ComboBox
        private MediaTrackView[] _audioTrackViews = Array.Empty<MediaTrackView>();

        public MainWindow()
        {
            InitializeComponent();

            // Initialize LibVLC once
            Core.Initialize();

            // Recommended LibVLC options for smoother pause/unpause
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
        }

        // Note: 'async void' to allow parsing (await)
        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Video Files|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.flv;*.mpeg;*.mpg"
            };

            if (dlg.ShowDialog() == true)
            {
                _mediaPlayer.Stop();

                // Create new media with selected file
                var media = new Media(_libVLC, new Uri(dlg.FileName));

                // Parse to read track info
                await media.Parse(MediaParseOptions.ParseLocal);

                // Retrieve audio tracks
                var tracks = media.Tracks
                    .Where(t => t.TrackType == TrackType.Audio)
                    .ToArray();

                // Convert each MediaTrack to a MediaTrackView for display
                _audioTrackViews = tracks
                    .Select(t => new MediaTrackView(t))
                    .ToArray();

                // Bind to ComboBox
                AudioTracksComboBox.ItemsSource = _audioTrackViews;

                if (_audioTrackViews.Any())
                    AudioTracksComboBox.SelectedIndex = 0;

                // Assign and play
                _mediaPlayer.Media = media;
                _mediaPlayer.Play();
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
            _mediaPlayer?.Stop();
        }

        // When user changes audio track selection
        private void AudioTracksComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (AudioTracksComboBox.SelectedItem is MediaTrackView selectedTrackView)
            {
                // Switch audio using track.Id
                _mediaPlayer.SetAudioTrack(selectedTrackView.Track.Id);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _mediaPlayer?.Dispose();
            _libVLC?.Dispose();
            base.OnClosed(e);
        }
    }

    // Helper class to display friendly names for each audio track
    public class MediaTrackView
    {
        public MediaTrack Track { get; }
        public string DisplayName { get; }

        public MediaTrackView(MediaTrack track)
        {
            Track = track;

            // Show description if available,
            // else show language if available,
            // else fallback to "Track {ID}"
            DisplayName = !string.IsNullOrEmpty(track.Description)
                ? track.Description
                : !string.IsNullOrEmpty(track.Language)
                    ? track.Language
                    : $"Track {track.Id}";
        }
    }
}
