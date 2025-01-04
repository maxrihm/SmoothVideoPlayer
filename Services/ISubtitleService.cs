using SmoothVideoPlayer.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmoothVideoPlayer.Services
{
    public interface ISubtitleService
    {
        List<SubtitleTrackView> AllSubtitleTracks { get; }
        Task<SubtitleTrackView> ExtractAndParseSubtitleTrackAsync(string filePath, int trackIndex, string language, string description, string outputFolder);
        void AddSubtitleTrack(SubtitleTrackView track);
        string GetSubtitleForTime(TimeSpan currentTime, SubtitleTrackView track);
        List<SubtitleTrackView> ParseSubtitlesInFolder(string folderPath);
        string GetSubtitleFolderPath(string videoFilePath);
    }
}
