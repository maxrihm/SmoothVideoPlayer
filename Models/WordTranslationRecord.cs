using System;

namespace SmoothVideoPlayer.Models
{
    public class WordTranslationRecord
    {
        public int Id { get; set; }
        public string EngWord { get; set; }
        public string WordRu { get; set; }
        public string SubtitleEngContext { get; set; }
        public string SubtitleRuContext { get; set; }
        public string MoviePath { get; set; }
        public string ScreenshotPath { get; set; }
        public string SubtitlesEngPath { get; set; }
        public string SubtitlesRuPath { get; set; }
        public string AudioPath { get; set; }
        public string SubRuKey { get; set; }
        public string SubEngKey { get; set; }
        public DateTime? DateAddedToAnki { get; set; }
        public DateTime? DateAdded { get; set; }
        public DateTime? DateUpdated { get; set; }
    }
} 