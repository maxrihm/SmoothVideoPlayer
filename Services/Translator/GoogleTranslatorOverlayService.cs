using SmoothVideoPlayer.Views.Translator;
using SmoothVideoPlayer.Services;
using SmoothVideoPlayer.Services.OverlayManager;

namespace SmoothVideoPlayer.Services.Translator
{
    public class GoogleTranslatorOverlayService : ITranslatorOverlayService
    {
        readonly IOverlayManager overlayManager;
        GoogleTranslatorOverlayWindow window;
        bool isOverlayOpen;
        public bool IsOverlayOpen => isOverlayOpen;

        public GoogleTranslatorOverlayService(IOverlayManager overlayManager)
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
                if (window == null) window = new GoogleTranslatorOverlayWindow();
                window.SetText(SubtitleStateService.Instance.FirstSubtitleText);
                window.OpenOverlay();
                isOverlayOpen = true;
                overlayManager.RegisterOverlay();
            }
        }
    }
} 