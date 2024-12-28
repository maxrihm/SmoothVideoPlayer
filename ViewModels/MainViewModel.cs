using Microsoft.Win32;
using SmoothVideoPlayer.Models;
using SmoothVideoPlayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SmoothVideoPlayer.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        readonly IMediaService mediaService;
        readonly ISubtitleService subtitleService;
        string currentTime;
        string totalTime;
        string subtitleText;
        List<MediaTrackView> audioTracks;
        MediaTrackView selectedAudioTrack;
        List<SubtitleTrackView> subtitleTracks;
        SubtitleTrackView selectedSubtitleTrack;

        public MainViewModel(IMediaService mediaService, ISubtitleService subtitleService)
        {
            this.mediaService = mediaService;
            this.subtitleService = subtitleService;
            mediaService.Initialize();
            mediaService.OnTimeChanged += MediaService_OnTimeChanged;
            mediaService.OnStopped += MediaService_OnStopped;
            OpenCommand = new RelayCommand(async _ => await Open());
            TogglePlayPauseCommand = new RelayCommand(_ => TogglePlayPause());
            StopCommand = new RelayCommand(_ => Stop());
            AudioTrackChangedCommand = new RelayCommand(_ => AudioTrackChanged());
            SubtitleTrackChangedCommand = new RelayCommand(_ => SubtitleTrackChanged());
            CurrentTime = "00:00:00";
            TotalTime = "00:00:00";
        }

        public ICommand OpenCommand { get; }
        public ICommand TogglePlayPauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand AudioTrackChangedCommand { get; }
        public ICommand SubtitleTrackChangedCommand { get; }

        public string CurrentTime
        {
            get => currentTime;
            set
            {
                currentTime = value;
                OnPropertyChanged();
            }
        }

        public string TotalTime
        {
            get => totalTime;
            set
            {
                totalTime = value;
                OnPropertyChanged();
            }
        }

        public string SubtitleText
        {
            get => subtitleText;
            set
            {
                subtitleText = value;
                OnPropertyChanged();
            }
        }

        public List<MediaTrackView> AudioTracks
        {
            get => audioTracks;
            set
            {
                audioTracks = value;
                OnPropertyChanged();
            }
        }

        public MediaTrackView SelectedAudioTrack
        {
            get => selectedAudioTrack;
            set
            {
                selectedAudioTrack = value;
                OnPropertyChanged();
            }
        }

        public List<SubtitleTrackView> SubtitleTracks
        {
            get => subtitleTracks;
            set
            {
                subtitleTracks = value;
                OnPropertyChanged();
            }
        }

        public SubtitleTrackView SelectedSubtitleTrack
        {
            get => selectedSubtitleTrack;
            set
            {
                selectedSubtitleTrack = value;
                OnPropertyChanged();
            }
        }

        void MediaService_OnTimeChanged(TimeSpan current, TimeSpan total)
        {
            CurrentTime = current.ToString(@"hh\:mm\:ss");
            TotalTime = total.ToString(@"hh\:mm\:ss");
            if (SelectedSubtitleTrack != null)
            {
                SubtitleText = subtitleService.GetSubtitleForTime(current);
            }
            else
            {
                SubtitleText = "";
            }
        }

        void MediaService_OnStopped()
        {
            CurrentTime = "00:00:00";
            TotalTime = "00:00:00";
            SubtitleText = "";
        }

        async Task Open()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Video Files|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.flv;*.mpeg;*.mpg"
            };
            if (dlg.ShowDialog() == true)
            {
                mediaService.Play(dlg.FileName);
                var allTracks = mediaService.GetAudioTracks();
                AudioTracks = allTracks.ToList();
                if (AudioTracks.Any())
                {
                    SelectedAudioTrack = AudioTracks.First();
                    mediaService.SetAudioTrack(SelectedAudioTrack.Track.Id);
                }
                var textTracks = await GetSubtitleTracks(dlg.FileName);
                SubtitleTracks = textTracks;
                if (SubtitleTracks.Any())
                {
                    SelectedSubtitleTrack = SubtitleTracks.First();
                    subtitleService.SetSelectedSubtitleTrack(SelectedSubtitleTrack);
                }
            }
        }

        async Task<List<SubtitleTrackView>> GetSubtitleTracks(string filePath)
        {
            var media = new LibVLCSharp.Shared.Media(new LibVLCSharp.Shared.LibVLC(), new Uri(filePath));
            await media.Parse(LibVLCSharp.Shared.MediaParseOptions.ParseLocal);
            var subs = media.Tracks.Where(t => t.TrackType == LibVLCSharp.Shared.TrackType.Text).ToArray();
            var extracted = await subtitleService.ExtractAndParseSubtitleTracksAsync(filePath, subs.Length);
            return extracted;
        }

        public void TogglePlayPause()
        {
            if (mediaService.IsPlaying())
            {
                mediaService.Pause();
            }
            else
            {
                mediaService.Play();
            }
        }

        void Stop()
        {
            mediaService.Stop();
        }

        void AudioTrackChanged()
        {
            if (SelectedAudioTrack != null)
            {
                mediaService.SetAudioTrack(SelectedAudioTrack.Track.Id);
            }
        }

        void SubtitleTrackChanged()
        {
            if (SelectedSubtitleTrack != null)
            {
                subtitleService.SetSelectedSubtitleTrack(SelectedSubtitleTrack);
            }
            else
            {
                subtitleService.SetSelectedSubtitleTrack(null);
            }
        }
    }
}
