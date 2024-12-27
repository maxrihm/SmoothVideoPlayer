using LibVLCSharp.Shared;
using Microsoft.Win32;   // for OpenFileDialog
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

            // LibVLC needs to be initialized once
            Core.Initialize();

            // LibVLC recommended options
            var libVlcOptions = new[]
            {
                "--file-caching=3000",
                "--network-caching=3000",
                "--clock-jitter=0",
                "--avcodec-hw=any",
                "--no-stats",
                "--no-plugins-cache",
                "--no-drop-late-frames",
                "--no-skip-frames"
            };

            _libVLC = new LibVLC(libVlcOptions);

            _mediaPlayer = new MediaPlayer(_libVLC)
            {
                EnableHardwareDecoding = true
            };

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
                _mediaPlayer.Stop();
                var media = new Media(_libVLC, new Uri(dlg.FileName));
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
            if (_mediaPlayer != null && _mediaPlayer.IsPlaying)
            {
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
