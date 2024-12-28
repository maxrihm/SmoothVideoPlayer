using SmoothVideoPlayer.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmoothVideoPlayer.Services
{
    public interface ISubtitleService
    {
        List<SubtitleTrackView> AllSubtitleTracks { get; }
        Task<List<SubtitleTrackView>> ExtractAndParseSubtitleTracksAsync(string filePath, int totalSubtitleTracks);
        void AddSubtitleTrack(SubtitleTrackView track);
        string GetSubtitleForTime(TimeSpan currentTime, SubtitleTrackView track);
    }
}