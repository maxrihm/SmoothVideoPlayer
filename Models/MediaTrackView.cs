using LibVLCSharp.Shared;

namespace SmoothVideoPlayer.Models
{
    public class MediaTrackView
    {
        public MediaTrack Track { get; }
        public string DisplayName { get; }

        public MediaTrackView(MediaTrack track)
        {
            Track = track;
            DisplayName = !string.IsNullOrEmpty(track.Description)
                ? track.Description
                : !string.IsNullOrEmpty(track.Language)
                    ? track.Language
                    : $"Track {track.Id}";
        }
    }
} 