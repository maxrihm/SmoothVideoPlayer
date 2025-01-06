using SmoothVideoPlayer.Views.Translator;
using SmoothVideoPlayer.Services;

namespace SmoothVideoPlayer.Services.Translator
{
    public class TranslatorOverlayService : ITranslatorOverlayService
    {
        TranslatorOverlayWindow window;
        bool isOverlayOpen;
        public bool IsOverlayOpen => isOverlayOpen;

        public void ToggleOverlay()
        {
            if (isOverlayOpen)
            {
                window.Hide();
                isOverlayOpen = false;
            }
            else
            {
                if (window == null) window = new TranslatorOverlayWindow();
                window.SetText(SubtitleStateService.Instance.FirstSubtitleText);
                window.OpenOverlay();
                isOverlayOpen = true;
            }
        }
    }
}
