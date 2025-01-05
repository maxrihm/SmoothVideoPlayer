using System;

namespace SmoothVideoPlayer.Services.SubtitleOverlay
{
    public interface ISubtitleOverlayService
    {
        void Initialize();
        void UpdateSubtitles(string top, string bottom);
    }
} 