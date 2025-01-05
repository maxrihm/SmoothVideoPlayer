using SmoothVideoPlayer.Models;
using System;
using System.Collections.Generic;

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
        void SetAudioTrack(int vlcTrackId);
        TimeSpan GetLength();
        void Seek(TimeSpan position);
        string TakeSnapshot(string folderPath);
        int? SelectedAudioFfmpegIndex { get; }
        void SetAudioFfmpegIndex(int index);
    }
}