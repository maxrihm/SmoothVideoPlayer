using SmoothVideoPlayer.Models;
using System;

namespace SmoothVideoPlayer.Services
{
    public class SubtitleStateService : ISubtitleStateService
    {
        static SubtitleStateService instance;
        public static SubtitleStateService Instance => instance ??= new SubtitleStateService();
        public SubtitleTrackView FirstSubtitleTrack { get; set; }
        public SubtitleTrackView SecondSubtitleTrack { get; set; }
        public TimeSpan CurrentTime { get; set; }
        public string CurrentVideoFilePath { get; set; }
        public string FirstSubtitleText { get; private set; }
        public string SecondSubtitleText { get; private set; }
        int lastFirstSubtitleIndex = -1;
        int lastSecondSubtitleIndex = -1;

        SubtitleStateService() {}

        public void UpdateSubtitleText()
        {
            var firstResult = GetIndexAndText(CurrentTime, FirstSubtitleTrack);
            lastFirstSubtitleIndex = firstResult.index;
            FirstSubtitleText = firstResult.text;
            var secondResult = GetIndexAndText(CurrentTime, SecondSubtitleTrack);
            lastSecondSubtitleIndex = secondResult.index;
            SecondSubtitleText = secondResult.text;
        }

        public int CurrentFirstSubtitleLineNumber
        {
            get
            {
                if (FirstSubtitleTrack == null || lastFirstSubtitleIndex < 0) return 0;
                if (lastFirstSubtitleIndex >= FirstSubtitleTrack.ParsedSubtitles.Count) return 0;
                return FirstSubtitleTrack.ParsedSubtitles[lastFirstSubtitleIndex].LineNumber;
            }
        }

        public int CurrentSecondSubtitleLineNumber
        {
            get
            {
                if (SecondSubtitleTrack == null || lastSecondSubtitleIndex < 0) return 0;
                if (lastSecondSubtitleIndex >= SecondSubtitleTrack.ParsedSubtitles.Count) return 0;
                return SecondSubtitleTrack.ParsedSubtitles[lastSecondSubtitleIndex].LineNumber;
            }
        }

        public void JumpToNext(SubtitleTrackView track)
        {
            if (track == null || track.ParsedSubtitles == null || track.ParsedSubtitles.Count == 0) return;
            var offset = TimeSpan.FromMilliseconds(10);
            var idx = track.ParsedSubtitles.FindIndex(x => x.StartTime <= CurrentTime && x.EndTime >= CurrentTime);
            if (idx < 0)
            {
                idx = track.ParsedSubtitles.FindIndex(x => x.StartTime > CurrentTime);
                if (idx < 0) return;
                CurrentTime = track.ParsedSubtitles[idx].StartTime + offset;
                return;
            }
            idx++;
            if (idx >= track.ParsedSubtitles.Count) return;
            CurrentTime = track.ParsedSubtitles[idx].StartTime + offset;
        }

        public void JumpToPrevious(SubtitleTrackView track)
        {
            if (track == null || track.ParsedSubtitles == null || track.ParsedSubtitles.Count == 0) return;
            var offset = TimeSpan.FromMilliseconds(10);
            var idx = track.ParsedSubtitles.FindIndex(x => x.StartTime <= CurrentTime && x.EndTime >= CurrentTime);
            if (idx < 0)
            {
                idx = track.ParsedSubtitles.FindIndex(x => x.StartTime > CurrentTime);
                if (idx <= 0) return;
                idx--;
                CurrentTime = track.ParsedSubtitles[idx].StartTime + offset;
                return;
            }
            idx--;
            if (idx < 0) return;
            CurrentTime = track.ParsedSubtitles[idx].StartTime + offset;
        }

        (int index, string text) GetIndexAndText(TimeSpan time, SubtitleTrackView track)
        {
            if (track == null || track.ParsedSubtitles == null || track.ParsedSubtitles.Count == 0) return (-1, "");
            var idx = track.ParsedSubtitles.FindIndex(x => x.StartTime <= time && x.EndTime >= time);
            if (idx < 0) return (-1, "");
            var lines = track.ParsedSubtitles[idx].Lines;
            return (idx, string.Join("\n", lines));
        }
    }
}
