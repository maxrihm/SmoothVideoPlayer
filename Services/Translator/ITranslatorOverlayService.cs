namespace SmoothVideoPlayer.Services.Translator
{
    public interface ITranslatorOverlayService
    {
        bool IsOverlayOpen { get; }
        void ToggleOverlay();
    }
}
