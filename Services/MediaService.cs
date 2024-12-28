using LibVLCSharp.Shared;
using SmoothVideoPlayer.Models;
using System;
using System.Collections.Generic;
using System.Threading;

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
            InitializeLibVLC();
            InitializeMediaPlayer();
        }

        void InitializeLibVLC()
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
        }

        void InitializeMediaPlayer()
        {
            mediaPlayer = new MediaPlayer(libVLC)
            {
                EnableHardwareDecoding = true
            };
            mediaPlayer.TimeChanged += OnMediaPlayerTimeChanged;
            mediaPlayer.EndReached += OnMediaPlayerEndReached;
        }

        void OnMediaPlayerTimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            var current = TimeSpan.FromMilliseconds(e.Time);
            var total = TimeSpan.FromMilliseconds(mediaPlayer.Length);
            OnTimeChanged?.Invoke(current, total);
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
                media.Parse(MediaParseOptions.ParseNetwork | MediaParseOptions.ParseLocal);
                while (media.ParsedStatus != MediaParsedStatus.Done)
                {
                    Thread.Sleep(10);
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