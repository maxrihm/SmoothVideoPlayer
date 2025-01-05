using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SmoothVideoPlayer.ViewModels;
using SmoothVideoPlayer.Services;
using SmoothVideoPlayer.Services.Translator;
using SmoothVideoPlayer.Services.AddWord;
using SmoothVideoPlayer.Data;
using SmoothVideoPlayer.Services.GotoTimeFeature;

namespace SmoothVideoPlayer.Views
{
    public partial class MainWindow : Window
    {
        ITranslatorOverlayService translatorOverlayService;
        IAddWordOverlayService addWordOverlayService;

        public MainWindow()
        {
            InitializeComponent();
            var mediaSrv = new MediaService();
            var mainVM = new MainViewModel(mediaSrv, new SubtitleService(), new GotoTimeService());
            DataContext = mainVM;
            translatorOverlayService = new TranslatorOverlayService();
            var repository = new WordRepository(new WordTranslationDbContext());
            addWordOverlayService = new AddWordOverlayService(repository, mediaSrv);
        }

        void AudioTracksComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.AudioTrackChangedCommand.CanExecute(null))
            {
                vm.AudioTrackChangedCommand.Execute(null);
                if (vm.SelectedAudioTrack != null)
                {
                    var ms = vm.GetType()
                        .GetProperty("mediaService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        ?.GetValue(vm) as IMediaService;
                    if (ms != null) ms.SetAudioFfmpegIndex(vm.SelectedAudioTrack.FfmpegIndex);
                }
            }
        }

        void FirstSubtitleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.FirstSubtitleTrackChangedCommand.CanExecute(null))
            {
                vm.FirstSubtitleTrackChangedCommand.Execute(null);
            }
        }

        void SecondSubtitleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.SecondSubtitleTrackChangedCommand.CanExecute(null))
            {
                vm.SecondSubtitleTrackChangedCommand.Execute(null);
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
                if (e.Key == Key.Space || e.Key == Key.W)
                {
                    vm.TogglePlayPause();
                }
                if (e.Key == Key.A && vm.JumpToPreviousSubtitleCommand.CanExecute(null))
                {
                    vm.JumpToPreviousSubtitleCommand.Execute(null);
                }
                if (e.Key == Key.D && vm.JumpToNextSubtitleCommand.CanExecute(null))
                {
                    vm.JumpToNextSubtitleCommand.Execute(null);
                }
            }
            if (e.Key == Key.CapsLock) translatorOverlayService.ToggleOverlay();
        }

        void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            Activate();
        }

        void AddWordButton_Click(object sender, RoutedEventArgs e)
        {
            addWordOverlayService.ToggleOverlay();
        }

        void GotoTimeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DataContext is MainViewModel vm && vm.GotoTimeCommand.CanExecute(null))
                {
                    vm.GotoTimeCommand.Execute(null);
                }
                (sender as TextBox)?.Clear();
            }
        }
    }
}
