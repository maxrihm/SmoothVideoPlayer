using System;
using System.Collections.Generic;

namespace SmoothVideoPlayer.Models
{
    public class ParsedSubtitleItem
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public IList<string> Lines { get; set; } = Array.Empty<string>();
    }
}