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
        string subtitleTextSecond;
        List<MediaTrackView> audioTracks;
        MediaTrackView selectedAudioTrack;
        List<SubtitleTrackView> subtitleTracks;
        SubtitleTrackView selectedSubtitleTrack;
        SubtitleTrackView selectedSecondSubtitleTrack;

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
            FirstSubtitleTrackChangedCommand = new RelayCommand(_ => FirstSubtitleTrackChanged());
            SecondSubtitleTrackChangedCommand = new RelayCommand(_ => SecondSubtitleTrackChanged());
            UploadSubtitleCommand = new RelayCommand(async _ => await UploadSubtitle());
            CurrentTime = "00:00:00";
            TotalTime = "00:00:00";
        }

        public ICommand OpenCommand { get; }
        public ICommand TogglePlayPauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand AudioTrackChangedCommand { get; }
        public ICommand FirstSubtitleTrackChangedCommand { get; }
        public ICommand SecondSubtitleTrackChangedCommand { get; }
        public ICommand UploadSubtitleCommand { get; }

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

        public string SubtitleTextSecond
        {
            get => subtitleTextSecond;
            set
            {
                subtitleTextSecond = value;
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

        public SubtitleTrackView SelectedSecondSubtitleTrack
        {
            get => selectedSecondSubtitleTrack;
            set
            {
                selectedSecondSubtitleTrack = value;
                OnPropertyChanged();
            }
        }

        void MediaService_OnTimeChanged(TimeSpan current, TimeSpan total)
        {
            CurrentTime = current.ToString(@"hh\:mm\:ss");
            TotalTime = total.ToString(@"hh\:mm\:ss");
            SubtitleText = subtitleService.GetSubtitleForTime(current, SelectedSubtitleTrack);
            SubtitleTextSecond = subtitleService.GetSubtitleForTime(current, SelectedSecondSubtitleTrack);
        }

        void MediaService_OnStopped()
        {
            CurrentTime = "00:00:00";
            TotalTime = "00:00:00";
            SubtitleText = "";
            SubtitleTextSecond = "";
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
                    SelectedSecondSubtitleTrack = null;
                }
            }
        }

        async Task<List<SubtitleTrackView>> GetSubtitleTracks(string filePath)
        {
            var libvlc = new LibVLCSharp.Shared.LibVLC();
            var media = new LibVLCSharp.Shared.Media(libvlc, new Uri(filePath));
            await media.Parse(LibVLCSharp.Shared.MediaParseOptions.ParseLocal);
            var subs = media.Tracks.Where(t => t.TrackType == LibVLCSharp.Shared.TrackType.Text).ToArray();
            var extracted = await subtitleService.ExtractAndParseSubtitleTracksAsync(filePath, subs.Length);
            return new List<SubtitleTrackView>(subtitleService.AllSubtitleTracks);
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

        void FirstSubtitleTrackChanged()
        {
        }

        void SecondSubtitleTrackChanged()
        {
        }

        async Task UploadSubtitle()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Subtitle Files|*.srt;*.vtt;*.ass;*.ssa"
            };
            if (dlg.ShowDialog() == true)
            {
                var path = dlg.FileName;
                var parsed = await Task.Run(() => subtitleService.ExtractAndParseSubtitleTracksAsync(path, 0));
                if (parsed != null && parsed.Count == 0)
                {
                    var srt = await Task.Run(() => ParseSingleSubtitle(path));
                    if (srt != null)
                    {
                        subtitleService.AddSubtitleTrack(srt);
                    }
                }
                SubtitleTracks = new List<SubtitleTrackView>(subtitleService.AllSubtitleTracks);
            }
        }

        SubtitleTrackView ParseSingleSubtitle(string path)
        {
            var fs = System.IO.File.OpenRead(path);
            var parser = new SubtitlesParser.Classes.Parsers.SrtParser();
            var items = parser.ParseStream(fs, System.Text.Encoding.UTF8);
            fs.Close();
            var trackItems = new List<ParsedSubtitleItem>();
            foreach (var item in items)
            {
                trackItems.Add(new ParsedSubtitleItem
                {
                    StartTime = TimeSpan.FromMilliseconds(item.StartTime),
                    EndTime = TimeSpan.FromMilliseconds(item.EndTime),
                    Lines = item.Lines
                });
            }
            var trackName = System.IO.Path.GetFileNameWithoutExtension(path);
            var t = new SubtitleTrackView
            {
                Name = trackName,
                FilePath = path,
                ParsedSubtitles = trackItems
            };
            return t;
        }
    }
}
