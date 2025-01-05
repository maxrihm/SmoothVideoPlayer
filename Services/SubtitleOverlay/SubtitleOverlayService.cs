using SmoothVideoPlayer.Views.SubtitleOverlay;

namespace SmoothVideoPlayer.Services.SubtitleOverlay
{
    public class SubtitleOverlayService : ISubtitleOverlayService
    {
        SubtitleOverlayWindow window;
        public void Initialize()
        {
            if (window == null) window = new SubtitleOverlayWindow();
            window.Show();
        }
        public void UpdateSubtitles(string top, string bottom)
        {
            window.SetTopSubtitleText(top);
            window.SetBottomSubtitleText(bottom);
        }
    }
} 