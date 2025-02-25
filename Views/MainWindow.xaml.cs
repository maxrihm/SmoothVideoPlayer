using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using SmoothVideoPlayer.ViewModels;
using SmoothVideoPlayer.Services;
using SmoothVideoPlayer.Services.Translator;
using SmoothVideoPlayer.Services.AddWord;
using SmoothVideoPlayer.Data;
using SmoothVideoPlayer.Services.GotoTimeFeature;
using SmoothVideoPlayer.Services.GlobalHotkeys;
using SmoothVideoPlayer.Services.OverlayManager;

namespace SmoothVideoPlayer.Views
{
    public partial class MainWindow : Window
    {
        ITranslatorOverlayService translatorOverlayAggregator;
        IAddWordOverlayService addWordOverlayService;
        IGlobalHotkeyService globalHotkeyService;
        IOverlayManager overlayManager;
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
            overlayManager = new OverlayManager();
            var yandexService = new YandexTranslatorOverlayService(overlayManager);
            var googleService = new GoogleTranslatorOverlayService(overlayManager);
            translatorOverlayAggregator = new TranslatorOverlayAggregatorService(yandexService, googleService);
            var repository = new WordRepository(new WordTranslationDbContext());
            addWordOverlayService = new AddWordOverlayService(repository, mediaSrv, overlayManager);
            globalHotkeyService = new GlobalHotkeyService(mainVM, translatorOverlayAggregator, addWordOverlayService, overlayManager);
            globalHotkeyService.Initialize();
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
            // Force creation of the CoreWebView2 environment
            wv.EnsureCoreWebView2Async();

            // Hook the event to set the background to transparent
            wv.CoreWebView2InitializationCompleted += (sender, args) =>
            {
                if (args.IsSuccess && wv.CoreWebView2 != null)
                {
                    // This is crucial to ensure true transparency
                    wv.DefaultBackgroundColor = System.Drawing.Color.Transparent;
                }
            };

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
            if (DataContext is MainViewModel vm) vm.MediaService?.Stop();
            globalHotkeyService.Dispose();
            base.OnClosed(e);
        }

        void AddWordButton_Click(object sender, RoutedEventArgs e)
        {
            addWordOverlayService.ToggleOverlay();
        }

        void GotoTimeTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (DataContext is MainViewModel vm && vm.GotoTimeCommand.CanExecute(null))
                {
                    vm.GotoTimeCommand.Execute(null);
                }
                (sender as TextBox)?.Clear();
                System.Windows.Input.Keyboard.ClearFocus();
            }
        }
    }
}
