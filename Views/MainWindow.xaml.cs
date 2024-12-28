using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        void AudioTracksComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.AudioTrackChangedCommand.CanExecute(null))
            {
                vm.AudioTrackChangedCommand.Execute(null);
            }
        }

        void SubtitlesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.SubtitleTrackChangedCommand.CanExecute(null))
            {
                vm.SubtitleTrackChangedCommand.Execute(null);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is MainViewModel)
            {
                var svc = (DataContext as MainViewModel)
                    .GetType()
                    .GetProperty("mediaService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(DataContext) as IMediaService;
                svc?.Stop();
            }
            base.OnClosed(e);
        }

        void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                if (e.Key == Key.Space)
                {
                    vm.TogglePlayPause();
                }
            }
        }

        void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            Activate();
        }
    }
}
