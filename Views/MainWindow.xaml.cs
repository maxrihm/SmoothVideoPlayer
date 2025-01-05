using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core;
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
        bool initializedTop;
        bool initializedBottom;
        string topText = "";
        string bottomText = "";
        public MainWindow()
        {
            InitializeComponent();
            var mediaSrv = new MediaService();
            var subtitleSrv = new SubtitleService();
            var gotoTimeSrv = new GotoTimeService();
            var mainVM = new MainViewModel(mediaSrv, subtitleSrv, gotoTimeSrv);
            DataContext = mainVM;
            translatorOverlayService = new TranslatorOverlayService();
            var repository = new WordRepository(new WordTranslationDbContext());
            addWordOverlayService = new AddWordOverlayService(repository, mediaSrv);
            Loaded += OnMainWindowLoaded;
        }
        void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            SetupWebView(webViewBottom, "SubtitleOverlayBottom.html", false);
            SetupWebView(webViewTop, "SubtitleOverlayTop.html", true);
            var vm = DataContext as MainViewModel;
            var subtitleState = SubtitleStateService.Instance;
            vm.MediaService.OnTimeChanged += (current, total) =>
            {
                subtitleState.CurrentTime = current;
                subtitleState.UpdateSubtitleText();
                topText = subtitleState.FirstSubtitleText?.Replace("\r", "").Replace("\n", " ") ?? "";
                bottomText = subtitleState.SecondSubtitleText?.Replace("\r", "").Replace("\n", " ") ?? "";
                Dispatcher.InvokeAsync(() =>
                {
                    if (initializedTop) UpdateTopText();
                    if (initializedBottom) UpdateBottomText();
                });
            };
            vm.MediaService.OnStopped += () =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    topText = "";
                    bottomText = "";
                    if (initializedTop) UpdateTopText();
                    if (initializedBottom) UpdateBottomText();
                });
            };
        }
        void SetupWebView(Microsoft.Web.WebView2.Wpf.WebView2 wv, string htmlFileName, bool isTop)
        {
            wv.EnsureCoreWebView2Async();
            var htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "SubtitleOverlay", htmlFileName);
            wv.NavigationCompleted += (s, e) =>
            {
                if (isTop) initializedTop = true;
                else initializedBottom = true;
                Dispatcher.InvokeAsync(() =>
                {
                    if (isTop) UpdateTopText();
                    else UpdateBottomText();
                });
            };
            wv.Source = new Uri(htmlPath);
        }
        async void UpdateTopText()
        {
            if (webViewTop.CoreWebView2 != null)
            {
                var safeText = topText ?? "";
                var script = "setSubtitle('" + safeText.Replace("'", "\\'") + "')";
                await webViewTop.CoreWebView2.ExecuteScriptAsync(script);
            }
        }
        async void UpdateBottomText()
        {
            if (webViewBottom.CoreWebView2 != null)
            {
                var safeText = bottomText ?? "";
                var script = "setSubtitle('" + safeText.Replace("'", "\\'") + "')";
                await webViewBottom.CoreWebView2.ExecuteScriptAsync(script);
            }
        }
        void AudioTracksComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.AudioTrackChangedCommand.CanExecute(null))
            {
                vm.AudioTrackChangedCommand.Execute(null);
                if (vm.SelectedAudioTrack != null)
                {
                    if (vm.MediaService != null) vm.MediaService.SetAudioFfmpegIndex(vm.SelectedAudioTrack.FfmpegIndex);
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
            if (DataContext is MainViewModel vm)
            {
                vm.MediaService?.Stop();
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
