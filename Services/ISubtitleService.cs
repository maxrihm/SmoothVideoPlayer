using SmoothVideoPlayer.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmoothVideoPlayer.Services
{
    public interface ISubtitleService
    {
        Task<List<SubtitleTrackView>> ExtractAndParseSubtitleTracksAsync(string filePath, int totalSubtitleTracks);
        SubtitleTrackView GetSelectedSubtitleTrack();
        void SetSelectedSubtitleTrack(SubtitleTrackView track);
        string GetSubtitleForTime(TimeSpan currentTime);
    }
}