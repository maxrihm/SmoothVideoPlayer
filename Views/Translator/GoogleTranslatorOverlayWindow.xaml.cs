using System.Text.RegularExpressions;
using System.Windows;
using CefSharp;
using System.Threading.Tasks;

namespace SmoothVideoPlayer.Views.Translator
{
    public partial class GoogleTranslatorOverlayWindow : Window
    {
        private bool isInitialized;
        private string lastInjectedText;
        private const int MAX_INJECTION_ATTEMPTS = 5;
        private const int RETRY_DELAY_MS = 300;

        public GoogleTranslatorOverlayWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        public void SetText(string text)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            text = Regex.Replace(text, @"\r?\n", "");
            lastInjectedText = text;

            // If the browser is already loaded, try injecting right away
            if (isInitialized)
                InjectTextIfNeeded();
        }

        public void OpenOverlay()
        {
            Show();
            if (isInitialized)
                InjectTextIfNeeded();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            webBrowser.LoadingStateChanged += (s, args) =>
            {
                if (!args.IsLoading) // Main frame and all sub-frames are done loading
                {
                    Dispatcher.Invoke(() =>
                    {
                        isInitialized = true;
                        InjectTextIfNeeded();
                    });
                }
            };
        }

        private async void InjectTextIfNeeded()
        {
            if (!isInitialized || string.IsNullOrEmpty(lastInjectedText))
                return;

            for (int attempt = 1; attempt <= MAX_INJECTION_ATTEMPTS; attempt++)
            {
                bool success = await InjectTextIntoTextAreaAsync(lastInjectedText);
                if (success)
                    break;

                // Wait a bit before retrying, in case the translator's script is still setting up
                await Task.Delay(RETRY_DELAY_MS);
            }
        }

        private async Task<bool> InjectTextIntoTextAreaAsync(string text)
        {
            // Properly escape text for JS
            string safeText = text
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\"", "\\\"");

            string script = $@"
                (function() {{
                    var textarea = document.querySelector('textarea');
                    if (!textarea) return false;

                    textarea.value = '{safeText}';
                    // Fire input/change events to trigger translator's internal handlers
                    textarea.dispatchEvent(new Event('input', {{ bubbles: true, cancelable: true }}));
                    textarea.dispatchEvent(new Event('change', {{ bubbles: true, cancelable: true }}));

                    return true;
                }})();
            ";

            try
            {
                var response = await webBrowser.GetMainFrame().EvaluateScriptAsync(script);
                return response?.Success == true && response.Result is bool result && result;
            }
            catch
            {
                return false;
            }
        }
    }
}
