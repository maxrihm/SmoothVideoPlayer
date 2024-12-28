using LibVLCSharp.Shared;
using SmoothVideoPlayer.Models;
using System;
using System.Collections.Generic;

namespace SmoothVideoPlayer.Services
{
    public class MediaService : IMediaService
    {
        LibVLC libVLC;
        MediaPlayer mediaPlayer;
        public event Action<TimeSpan, TimeSpan> OnTimeChanged;
        public event Action OnStopped;

        public void Initialize()
        {
            Core.Initialize();
            var libVlcOptions = new[]
            {
                "--file-caching=300",
                "--network-caching=300",
                "--live-caching=300",
                "--avcodec-hw=any",
                "--no-stats",
                "--no-plugins-cache"
            };
            libVLC = new LibVLC(libVlcOptions);
            mediaPlayer = new MediaPlayer(libVLC)
            {
                EnableHardwareDecoding = true
            };
            mediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
            mediaPlayer.EndReached += MediaPlayer_EndReached;
        }

        void MediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            var currentTime = TimeSpan.FromMilliseconds(e.Time);
            var totalTime = TimeSpan.FromMilliseconds(mediaPlayer.Length);
            OnTimeChanged?.Invoke(currentTime, totalTime);
        }

        void MediaPlayer_EndReached(object sender, EventArgs e)
        {
            Stop();
        }

        public void Play(string filePath)
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.Stop();
                var media = new Media(libVLC, new Uri(filePath));
                media.Parse(MediaParseOptions.ParseNetwork | MediaParseOptions.ParseLocal);
                while (media.ParsedStatus != MediaParsedStatus.Done)
                {
                    System.Threading.Thread.Sleep(10);
                }
                mediaPlayer.Media = media;
                mediaPlayer.Play();
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
            if (mediaPlayer != null && mediaPlayer.Media != null && mediaPlayer.Media.Tracks != null)
            {
                foreach (var track in mediaPlayer.Media.Tracks)
                {
                    if (track.TrackType == TrackType.Audio)
                    {
                        list.Add(new MediaTrackView(track));
                    }
                }
            }
            return list;
        }

        public void SetAudioTrack(int trackId)
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.SetAudioTrack(trackId);
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
    }
} 