using SmoothVideoPlayer.Views.Translator;
using SmoothVideoPlayer.Services;

namespace SmoothVideoPlayer.Services.Translator
{
    public class TranslatorOverlayService : ITranslatorOverlayService
    {
        TranslatorOverlayWindow window;
        bool isOpen;
        public void ToggleOverlay()
        {
            if (isOpen)
            {
                window.Hide();
                isOpen = false;
            }
            else
            {
                if (window == null) window = new TranslatorOverlayWindow();
                window.SetText(SubtitleStateService.Instance.FirstSubtitleText);
                window.OpenOverlay();
                isOpen = true;
            }
        }
    }
}
