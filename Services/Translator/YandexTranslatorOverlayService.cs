using SmoothVideoPlayer.Views.Translator;
using SmoothVideoPlayer.Services;
using SmoothVideoPlayer.Services.OverlayManager;

namespace SmoothVideoPlayer.Services.Translator
{
    public class YandexTranslatorOverlayService : ITranslatorOverlayService
    {
        readonly IOverlayManager overlayManager;
        YandexTranslatorOverlayWindow window;
        bool isOverlayOpen;
        public bool IsOverlayOpen => isOverlayOpen;

        public YandexTranslatorOverlayService(IOverlayManager overlayManager)
        {
            this.overlayManager = overlayManager;
        }

        public void ToggleOverlay()
        {
            if (isOverlayOpen)
            {
                window.Hide();
                isOverlayOpen = false;
                overlayManager.UnregisterOverlay();
            }
            else
            {
                if (window == null) window = new YandexTranslatorOverlayWindow();
                window.SetText(SubtitleStateService.Instance.FirstSubtitleText);
                window.OpenOverlay();
                isOverlayOpen = true;
                overlayManager.RegisterOverlay();
            }
        }
    }
}
