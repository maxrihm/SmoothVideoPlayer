using SmoothVideoPlayer.Models;
using System;

namespace SmoothVideoPlayer.Services
{
    public interface ISubtitleStateService
    {
        SubtitleTrackView FirstSubtitleTrack { get; set; }
        SubtitleTrackView SecondSubtitleTrack { get; set; }
        TimeSpan CurrentTime { get; set; }
        string FirstSubtitleText { get; }
        string SecondSubtitleText { get; }
        void UpdateSubtitleText();
        void JumpToNext(SubtitleTrackView track);
        void JumpToPrevious(SubtitleTrackView track);
        string CurrentVideoFilePath { get; set; }
        int CurrentFirstSubtitleLineNumber { get; }
        int CurrentSecondSubtitleLineNumber { get; }
    }
}
