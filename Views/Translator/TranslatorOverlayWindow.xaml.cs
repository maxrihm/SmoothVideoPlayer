using System.Text.RegularExpressions;
using System.Windows;
using CefSharp;
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

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            webBrowser.FrameLoadEnd += (s, args) =>
            {
                if (!args.Frame.IsMain) return;
                
                Dispatcher.Invoke(() =>
                {
                    isInitialized = true;
                    InjectText();
                });
            };
        }

        private async void InjectText()
        {
            if (!isInitialized || string.IsNullOrEmpty(lastInjectedText)) return;

            var script =
            $@"
            textarea=document.querySelector('#textarea');
            if(textarea.hasAttribute('disabled'))textarea.removeAttribute('disabled');
            textarea.value='{lastInjectedText}';
            textarea.dispatchEvent(new Event('input',{{bubbles:true,cancelable:true}}));
            textarea.dispatchEvent(new Event('change',{{bubbles:true,cancelable:true}}));
            ";
            
            await webBrowser.EvaluateScriptAsync(script);
        }
    }
}
