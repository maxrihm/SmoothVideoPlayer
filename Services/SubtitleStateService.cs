using SmoothVideoPlayer.Models;
using System;
using System.Linq;

namespace SmoothVideoPlayer.Services
{
    public class SubtitleStateService : ISubtitleStateService
    {
        static SubtitleStateService instance;
        public static SubtitleStateService Instance => instance ??= new SubtitleStateService();
        public SubtitleTrackView FirstSubtitleTrack { get; set; }
        public SubtitleTrackView SecondSubtitleTrack { get; set; }
        public TimeSpan CurrentTime { get; set; }
        public string FirstSubtitleText { get; private set; }
        public string SecondSubtitleText { get; private set; }
        SubtitleStateService() {}
        public void UpdateSubtitleText()
        {
            FirstSubtitleText = GetSubtitleForTime(CurrentTime, FirstSubtitleTrack);
            SecondSubtitleText = GetSubtitleForTime(CurrentTime, SecondSubtitleTrack);
        }
        public void JumpToNext(SubtitleTrackView track)
        {
            if (track == null || track.ParsedSubtitles == null || track.ParsedSubtitles.Count == 0) return;
            var idx = track.ParsedSubtitles.FindIndex(x => x.StartTime <= CurrentTime && x.EndTime >= CurrentTime);
            if (idx < 0)
            {
                idx = track.ParsedSubtitles.FindIndex(x => x.StartTime > CurrentTime);
                if (idx < 0) return;
                CurrentTime = track.ParsedSubtitles[idx].StartTime;
                return;
            }
            idx++;
            if (idx >= track.ParsedSubtitles.Count) return;
            CurrentTime = track.ParsedSubtitles[idx].StartTime;
        }
        public void JumpToPrevious(SubtitleTrackView track)
        {
            if (track == null || track.ParsedSubtitles == null || track.ParsedSubtitles.Count == 0) return;
            var idx = track.ParsedSubtitles.FindIndex(x => x.StartTime <= CurrentTime && x.EndTime >= CurrentTime);
            if (idx < 0)
            {
                idx = track.ParsedSubtitles.FindIndex(x => x.StartTime > CurrentTime);
                if (idx <= 0) return;
                idx--;
                CurrentTime = track.ParsedSubtitles[idx].StartTime;
                return;
            }
            idx--;
            if (idx < 0) return;
            CurrentTime = track.ParsedSubtitles[idx].StartTime;
        }
        string GetSubtitleForTime(TimeSpan time, SubtitleTrackView track)
        {
            if (track == null || track.ParsedSubtitles == null || track.ParsedSubtitles.Count == 0) return "";
            var idx = track.ParsedSubtitles.FindIndex(x => x.StartTime <= time && x.EndTime >= time);
            if (idx < 0) return "";
            return string.Join("\n", track.ParsedSubtitles[idx].Lines);
        }
    }
}
