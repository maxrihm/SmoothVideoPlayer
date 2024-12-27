using LibVLCSharp.Shared;
using Microsoft.Win32;
using System;
using System.Windows;

namespace SmoothVideoPlayer
{
    public partial class MainWindow : Window
    {
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;

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

                // You can comment out advanced options if they're causing side effects:
                // "--clock-jitter=0",
                // Keep hardware decoding, but you can test disabling it if needed:
                "--avcodec-hw=any",

                "--no-stats",
                "--no-plugins-cache"

                // Remove or comment out the lines below that prevent dropping frames:
                // "--no-drop-late-frames",
                // "--no-skip-frames"
            };

            _libVLC = new LibVLC(libVlcOptions);

            _mediaPlayer = new MediaPlayer(_libVLC)
            {
                // You can disable hardware decoding to test if it helps:
                EnableHardwareDecoding = true
            };

            // Assign MediaPlayer to our VideoView
            videoView.MediaPlayer = _mediaPlayer;
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Video Files|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.flv;*.mpeg;*.mpg"
            };

            if (dlg.ShowDialog() == true)
            {
                // Stop any currently playing media
                _mediaPlayer.Stop();

                // Create new media with selected file
                var media = new Media(_libVLC, new Uri(dlg.FileName));

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
                // If playing, pause. If already paused, unpause.
                _mediaPlayer.Pause();
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _mediaPlayer?.Stop();
        }

        protected override void OnClosed(EventArgs e)
        {
            _mediaPlayer?.Dispose();
            _libVLC?.Dispose();
            base.OnClosed(e);
        }
    }
}
