using SmoothVideoPlayer.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmoothVideoPlayer.Services
{
    public interface IMediaService
    {
        event Action<TimeSpan, TimeSpan> OnTimeChanged;
        event Action OnStopped;
        void Initialize();
        void Play(string filePath);
        void Play();
        void Pause();
        void Stop();
        bool IsPlaying();
        IList<MediaTrackView> GetAudioTracks();
        void SetAudioTrack(int trackId);
        TimeSpan GetLength();
        void Seek(TimeSpan position);
    }
}