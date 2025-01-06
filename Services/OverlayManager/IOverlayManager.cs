namespace SmoothVideoPlayer.Services.OverlayManager
{
    public interface IOverlayManager
    {
        int OverlayCount { get; }
        bool AreHotkeysEnabled { get; }
        void RegisterOverlay();
        void UnregisterOverlay();
    }
} 