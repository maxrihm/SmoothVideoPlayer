using LibVLCSharp.Shared;
using SmoothVideoPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace SmoothVideoPlayer.Services
{
    public class MediaService : IMediaService
    {
        LibVLC libVLC;
        MediaPlayer mediaPlayer;
        public event Action<TimeSpan, TimeSpan> OnTimeChanged;
        public event Action OnStopped;
        Dictionary<int, int> vlcIdToFfmpeg = new Dictionary<int, int>();
        int? currentFfmpegIndex;

        public void Initialize()
        {
            Core.Initialize();
            var options = new[]
            {
                "--file-caching=300",
                "--network-caching=300",
                "--live-caching=300",
                "--avcodec-hw=any",
                "--no-stats",
                "--no-plugins-cache"
            };
            libVLC = new LibVLC(options);
            mediaPlayer = new MediaPlayer(libVLC)
            {
                EnableHardwareDecoding = true
            };
            mediaPlayer.TimeChanged += OnMediaPlayerTimeChanged;
            mediaPlayer.EndReached += OnMediaPlayerEndReached;
        }

        void OnMediaPlayerTimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            var c = TimeSpan.FromMilliseconds(e.Time);
            var t = TimeSpan.FromMilliseconds(mediaPlayer.Length);
            OnTimeChanged?.Invoke(c, t);
        }

        void OnMediaPlayerEndReached(object sender, EventArgs e)
        {
            Stop();
        }

        public void Play(string filePath)
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.Stop();
                var media = new Media(libVLC, new Uri(filePath));
                media.AddOption(":no-embedded-video");
                media.AddOption(":video-on-top");
                media.AddOption(":fullscreen");
                media.Parse(MediaParseOptions.ParseLocal);
                while (media.ParsedStatus != MediaParsedStatus.Done)
                {
                    Thread.Sleep(10);
                }
                mediaPlayer.Media = media;
                mediaPlayer.Play();
                mediaPlayer.Fullscreen = true;
            }
        }

        public void Play()
        {
            if (mediaPlayer != null && !mediaPlayer.IsPlaying)
            {
                mediaPlayer.Play();
            }
        }

        public void Pause()
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.Pause();
            }
        }

        public void Stop()
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.Stop();
                OnStopped?.Invoke();
            }
        }

        public bool IsPlaying()
        {
            return mediaPlayer != null && mediaPlayer.IsPlaying;
        }

        public IList<MediaTrackView> GetAudioTracks()
        {
            var list = new List<MediaTrackView>();
            vlcIdToFfmpeg.Clear();
            if (mediaPlayer != null && mediaPlayer.Media != null && mediaPlayer.Media.Tracks != null)
            {
                var ffIndex = 0;
                foreach (var track in mediaPlayer.Media.Tracks)
                {
                    if (track.TrackType == TrackType.Audio)
                    {
                        var id = track.Id;
                        vlcIdToFfmpeg[id] = ffIndex;
                        list.Add(new MediaTrackView(track, id, ffIndex));
                        ffIndex++;
                    }
                }
            }
            return list;
        }

        public void SetAudioTrack(int vlcTrackId)
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.SetAudioTrack(vlcTrackId);
                if (vlcIdToFfmpeg.ContainsKey(vlcTrackId))
                {
                    currentFfmpegIndex = vlcIdToFfmpeg[vlcTrackId];
                }
            }
        }

        public TimeSpan GetLength()
        {
            if (mediaPlayer != null)
            {
                return TimeSpan.FromMilliseconds(mediaPlayer.Length);
            }
            return TimeSpan.Zero;
        }

        public void Seek(TimeSpan position)
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.Time = (long)position.TotalMilliseconds;
            }
        }

        public string TakeSnapshot(string folderPath)
        {
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            var fn = Guid.NewGuid().ToString() + ".png";
            var fp = Path.Combine(folderPath, fn);
            if (mediaPlayer != null)
            {
                mediaPlayer.TakeSnapshot(0, fp, 0, 0);
            }
            return fp;
        }

        public int? SelectedAudioFfmpegIndex => currentFfmpegIndex;

        public void SetAudioFfmpegIndex(int ffIndex)
        {
            currentFfmpegIndex = ffIndex;
        }
    }
}
