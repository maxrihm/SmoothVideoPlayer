using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using SmoothVideoPlayer.ViewModels;

namespace SmoothVideoPlayer.Views.Translator
{
    public partial class TranslatorOverlayWindow : Window
    {
        bool isInitialized;
        string lastInjectedText;

        public TranslatorOverlayWindow()
        {
            InitializeComponent();
            DataContext = new TranslatorOverlayViewModel();
            Loaded += OnLoaded;
        }

        public void SetText(string text)
        {
            if (string.IsNullOrEmpty(text)) text = "";
            text = Regex.Replace(text, @"\r?\n", "");
            lastInjectedText = text;
        }

        public void OpenOverlay()
        {
            Show();
            if (isInitialized) InjectText();
        }

        async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await webView.EnsureCoreWebView2Async();
            isInitialized = true;
            InjectText();
        }

        async void InjectText()
        {
            if (webView.CoreWebView2 == null || string.IsNullOrEmpty(lastInjectedText)) return;
            var script =
$@"
textarea=document.querySelector('#textarea');
if(textarea.hasAttribute('disabled'))textarea.removeAttribute('disabled');
textarea.value='{lastInjectedText}';
textarea.dispatchEvent(new Event('input',{{bubbles:true,cancelable:true}}));
textarea.dispatchEvent(new Event('change',{{bubbles:true,cancelable:true}}));
";
            await webView.CoreWebView2.ExecuteScriptAsync(script);
        }
    }
}
