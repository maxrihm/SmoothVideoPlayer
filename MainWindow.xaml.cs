using LibVLCSharp.Shared;
using Microsoft.Win32;
using System;
using System.Linq;        // Needed for .Where() and .ToList()
using System.Windows;

namespace SmoothVideoPlayer
{
    public partial class MainWindow : Window
    {
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;

        // New list to keep track of audio track info
        private MediaTrack[] _audioTracks = Array.Empty<MediaTrack>();

        public MainWindow()
        {
            InitializeComponent();

            // Initialize LibVLC once
            Core.Initialize();

            // Recommended LibVLC options for smoother pause/unpause
            // Lower caching from 3000ms to ~300ms for local playback
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

        // Modified to be 'async void' because we parse the media asynchronously
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

                // IMPORTANT: Parse the media so we can read track data
                await media.Parse(MediaParseOptions.ParseLocal);

                // Extract audio tracks
                var tracks = media.Tracks
                    .Where(t => t.TrackType == TrackType.Audio)
                    .ToArray();

                // Keep them in a local field if needed
                _audioTracks = tracks;

                // Populate the ComboBox with audio track info
                AudioTracksComboBox.ItemsSource = tracks;
                if (tracks.Any())
                {
                    // Optionally select the first track by default
                    AudioTracksComboBox.SelectedIndex = 0;
                }

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

        // New event handler to switch audio tracks
        private void AudioTracksComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // If a track is selected
            if (AudioTracksComboBox.SelectedItem is MediaTrack selectedTrack)
            {
                // LibVLC uses track.Id for switching
                _mediaPlayer.SetAudioTrack(selectedTrack.Id);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _mediaPlayer?.Dispose();
            _libVLC?.Dispose();
            base.OnClosed(e);
        }
    }
}
