using System;
using System.IO;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace SmoothVideoPlayer.Views.SubtitleOverlay
{
    public partial class SubtitleOverlayWindow : Window
    {
        bool initializedTop;
        bool initializedBottom;
        string topText;
        string bottomText;
        public SubtitleOverlayWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }
        void OnLoaded(object sender, RoutedEventArgs e)
        {
            SetupWebView(webViewTop, "SubtitleOverlayTop.html", true);
            SetupWebView(webViewBottom, "SubtitleOverlayBottom.html", false);
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            Left = 0;
            Top = 0;
            Width = screenWidth;
            Height = screenHeight;
        }
        void SetupWebView(Microsoft.Web.WebView2.Wpf.WebView2 wv, string htmlFileName, bool isTop)
        {
            wv.EnsureCoreWebView2Async();
            var htmlPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources",
                "SubtitleOverlay",
                htmlFileName
            );
            MessageBox.Show($"Attempting to load: {htmlPath}");
            wv.NavigationCompleted += async (s, e) =>
            {
                if (isTop)
                {
                    initializedTop = true;
                    UpdateTopText();
                }
                else
                {
                    initializedBottom = true;
                    UpdateBottomText();
                }
            };
            wv.Source = new System.Uri(htmlPath);
        }
        public void SetTopSubtitleText(string text)
        {
            topText = text ?? string.Empty;
            if (initializedTop) UpdateTopText();
        }
        public void SetBottomSubtitleText(string text)
        {
            bottomText = text ?? string.Empty;
            if (initializedBottom) UpdateBottomText();
        }
        async void UpdateTopText()
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                if (webViewTop.CoreWebView2 != null)
                {
                    var safeText = (topText ?? string.Empty).Replace("\r", "").Replace("\n", " ");
                    var script = "setSubtitle('" + safeText.Replace("'", "\\'") + "')";
                    await webViewTop.CoreWebView2.ExecuteScriptAsync(script);
                }
            });
        }
        async void UpdateBottomText()
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                if (webViewBottom.CoreWebView2 != null)
                {
                    var safeText = (bottomText ?? string.Empty).Replace("\r", "").Replace("\n", " ");
                    var script = "setSubtitle('" + safeText.Replace("'", "\\'") + "')";
                    await webViewBottom.CoreWebView2.ExecuteScriptAsync(script);
                }
            });
        }
    }
} 