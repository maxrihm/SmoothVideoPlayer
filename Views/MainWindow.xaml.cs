using System.Windows;
using SmoothVideoPlayer.ViewModels;
using SmoothVideoPlayer.Services;

namespace SmoothVideoPlayer.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(new MediaService(), new SubtitleService());
        }

        void AudioTracksComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.AudioTrackChangedCommand.CanExecute(null))
            {
                vm.AudioTrackChangedCommand.Execute(null);
            }
        }

        void SubtitlesComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.SubtitleTrackChangedCommand.CanExecute(null))
            {
                vm.SubtitleTrackChangedCommand.Execute(null);
            }
        }

        protected override void OnClosed(System.EventArgs e)
        {
            if (DataContext is MainViewModel)
            {
                var mediaService = (DataContext as MainViewModel).GetType().GetProperty("mediaService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(DataContext) as IMediaService;
                mediaService?.Stop();
            }
            base.OnClosed(e);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                var mediaServiceProp = vm.GetType().GetField("mediaService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (mediaServiceProp != null)
                {
                    var svc = mediaServiceProp.GetValue(vm) as MediaService;
                    if (svc != null)
                    {
                        videoView.MediaPlayer = null;
                        videoView.MediaPlayer = svc.GetType().GetField("mediaPlayer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(svc) as LibVLCSharp.Shared.MediaPlayer;
                    }
                }
            }
        }
    }
}