using System.Collections.Generic;

namespace SmoothVideoPlayer.Models
{
    public class SubtitleTrackView
    {
        public string Name { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public List<ParsedSubtitleItem> ParsedSubtitles { get; set; } = new List<ParsedSubtitleItem>();
    }
}