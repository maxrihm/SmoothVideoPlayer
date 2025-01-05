using LibVLCSharp.Shared;

namespace SmoothVideoPlayer.Models
{
    public class MediaTrackView
    {
        public MediaTrack Track { get; }
        public string DisplayName { get; }
        public int FfmpegIndex { get; }

        public MediaTrackView(MediaTrack track, int ffmpegIndex)
        {
            Track = track;
            FfmpegIndex = ffmpegIndex;
            DisplayName = !string.IsNullOrEmpty(track.Description)
                ? track.Description
                : !string.IsNullOrEmpty(track.Language)
                    ? track.Language
                    : $"Track {track.Id}";
        }
    }
} 