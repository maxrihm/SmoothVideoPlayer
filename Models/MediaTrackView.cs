using LibVLCSharp.Shared;

namespace SmoothVideoPlayer.Models
{
    public class MediaTrackView
    {
        public MediaTrack Track { get; }
        public int VlcId { get; }
        public int FfmpegIndex { get; }
        public string DisplayName { get; }

        public MediaTrackView(MediaTrack track, int vlcId, int ffmpegIndex)
        {
            Track = track;
            VlcId = vlcId;
            FfmpegIndex = ffmpegIndex;
            DisplayName = !string.IsNullOrEmpty(track.Description)
                ? track.Description
                : !string.IsNullOrEmpty(track.Language)
                    ? track.Language
                    : $"Track {vlcId}";
        }
    }
} 