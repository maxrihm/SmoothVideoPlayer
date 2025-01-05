using System;
using System.Threading.Tasks;

namespace SmoothVideoPlayer.Services.AudioExtraction
{
    public interface IAudioExtractionService
    {
        Task<string> ExtractAudioAsync(string inputFilePath, TimeSpan start, TimeSpan duration, int audioTrackIndex);
    }
} 