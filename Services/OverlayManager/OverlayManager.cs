namespace SmoothVideoPlayer.Services.OverlayManager
{
    public class OverlayManager : IOverlayManager
    {
        int overlayCount;
        public int OverlayCount => overlayCount;
        public bool AreHotkeysEnabled => overlayCount == 0;
        public void RegisterOverlay()
        {
            overlayCount++;
        }
        public void UnregisterOverlay()
        {
            if (overlayCount > 0) overlayCount--;
        }
    }
} 