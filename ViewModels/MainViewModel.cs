using Microsoft.Win32;
using SmoothVideoPlayer.Models;
using SmoothVideoPlayer.Services;
using SmoothVideoPlayer.Services.GotoTimeFeature;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SmoothVideoPlayer.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        readonly IMediaService mediaService;
        readonly ISubtitleService subtitleService;
        readonly ISubtitleStateService subtitleState;
        readonly IGotoTimeService gotoTimeService;
        string currentTime;
        string totalTime;
        List<MediaTrackView> audioTracks;
        MediaTrackView selectedAudioTrack;
        List<SubtitleTrackView> subtitleTracks;
        SubtitleTrackView selectedSubtitleTrack;
        SubtitleTrackView selectedSecondSubtitleTrack;
        string currentVideoFilePath;
        string gotoTimeInput;

        public IMediaService MediaService => mediaService;

        public MainViewModel(IMediaService mediaService, ISubtitleService subtitleService, IGotoTimeService gotoTimeService)
        {
            this.mediaService = mediaService;
            this.subtitleService = subtitleService;
            this.gotoTimeService = gotoTimeService;
            subtitleState = SubtitleStateService.Instance;
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
            ParseSubsCommand = new RelayCommand(async _ => await ParseSubs());
            JumpToPreviousSubtitleCommand = new RelayCommand(_ => JumpToPreviousSubtitle());
            JumpToNextSubtitleCommand = new RelayCommand(_ => JumpToNextSubtitle());
            GotoTimeCommand = new RelayCommand(_ => ExecuteGotoTime());
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
        public ICommand ParseSubsCommand { get; }
        public ICommand JumpToPreviousSubtitleCommand { get; }
        public ICommand JumpToNextSubtitleCommand { get; }
        public ICommand GotoTimeCommand { get; }
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
                if (selectedAudioTrack == value) return;
                selectedAudioTrack = value;
                OnPropertyChanged();
                
                // Call AudioTrackChanged automatically when track changes
                AudioTrackChanged();
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
                subtitleState.FirstSubtitleTrack = value;
                OnPropertyChanged();
            }
        }
        public SubtitleTrackView SelectedSecondSubtitleTrack
        {
            get => selectedSecondSubtitleTrack;
            set
            {
                selectedSecondSubtitleTrack = value;
                subtitleState.SecondSubtitleTrack = value;
                OnPropertyChanged();
            }
        }
        public string CurrentVideoFilePath
        {
            get => currentVideoFilePath;
            set
            {
                currentVideoFilePath = value;
                subtitleState.CurrentVideoFilePath = value;
                OnPropertyChanged();
            }
        }
        public string GotoTimeInput
        {
            get => gotoTimeInput;
            set
            {
                gotoTimeInput = value;
                OnPropertyChanged();
            }
        }
        async Task Open()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Video Files|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.flv;*.mpeg;*.mpg"
            };
            if (dlg.ShowDialog() == true)
            {
                CurrentVideoFilePath = dlg.FileName;
                mediaService.Play(CurrentVideoFilePath);
                var allTracks = mediaService.GetAudioTracks();
                AudioTracks = allTracks.ToList();
                if (AudioTracks.Any())
                {
                    SelectedAudioTrack = AudioTracks.First();
                    mediaService.SetAudioTrack(SelectedAudioTrack.Track.Id);
                }
                LoadSubtitlesIfFolderExists();
            }
        }
        void LoadSubtitlesIfFolderExists()
        {
            if (string.IsNullOrEmpty(CurrentVideoFilePath)) return;
            var folderPath = subtitleService.GetSubtitleFolderPath(CurrentVideoFilePath);
            if (!Directory.Exists(folderPath))
            {
                SubtitleTracks = new List<SubtitleTrackView>();
                SelectedSubtitleTrack = null;
                SelectedSecondSubtitleTrack = null;
                return;
            }
            subtitleService.AllSubtitleTracks.Clear();
            var foundTracks = subtitleService.ParseSubtitlesInFolder(folderPath);
            foreach (var t in foundTracks) subtitleService.AddSubtitleTrack(t);
            SubtitleTracks = new List<SubtitleTrackView>(subtitleService.AllSubtitleTracks);
            if (SubtitleTracks.Any())
            {
                SelectedSubtitleTrack = SubtitleTracks.First();
                SelectedSecondSubtitleTrack = null;
            }
        }
        async Task ParseSubs()
        {
            if (string.IsNullOrEmpty(CurrentVideoFilePath)) return;
            var libvlc = new LibVLCSharp.Shared.LibVLC();
            var media = new LibVLCSharp.Shared.Media(libvlc, new Uri(CurrentVideoFilePath));
            await media.Parse(LibVLCSharp.Shared.MediaParseOptions.ParseLocal);
            var subs = media.Tracks.Where(t => t.TrackType == LibVLCSharp.Shared.TrackType.Text).ToArray();
            subtitleService.AllSubtitleTracks.Clear();
            var folderPath = subtitleService.GetSubtitleFolderPath(CurrentVideoFilePath);
            for (int i = 0; i < subs.Length; i++)
            {
                var lang = string.IsNullOrEmpty(subs[i].Language) ? "UnknownLang" : subs[i].Language;
                var desc = string.IsNullOrEmpty(subs[i].Description) ? "NoDesc" : subs[i].Description;
                var extracted = await subtitleService.ExtractAndParseSubtitleTrackAsync(CurrentVideoFilePath, i, lang, desc, folderPath);
                if (extracted != null) subtitleService.AddSubtitleTrack(extracted);
            }
            SubtitleTracks = new List<SubtitleTrackView>(subtitleService.AllSubtitleTracks);
            if (SubtitleTracks.Any())
            {
                SelectedSubtitleTrack = SubtitleTracks.First();
                SelectedSecondSubtitleTrack = null;
            }
        }
        public void TogglePlayPause()
        {
            if (mediaService.IsPlaying()) mediaService.Pause();
            else mediaService.Play();
        }
        void Stop()
        {
            mediaService.Stop();
        }
        void AudioTrackChanged()
        {
            if (SelectedAudioTrack == null) return;

            // Set both VLC audio track for playback and FFmpeg index for extraction
            mediaService.SetAudioTrack(SelectedAudioTrack.Track.Id);
            mediaService.SetAudioFfmpegIndex(SelectedAudioTrack.FfmpegIndex);
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
                if (!string.IsNullOrEmpty(CurrentVideoFilePath))
                {
                    var folder = subtitleService.GetSubtitleFolderPath(CurrentVideoFilePath);
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                    var fileName = Path.GetFileName(path);
                    var dest = Path.Combine(folder, fileName);
                    File.Copy(path, dest, true);
                    var srt = ParseSingleSubtitle(dest);
                    if (srt != null) subtitleService.AddSubtitleTrack(srt);
                    SubtitleTracks = new List<SubtitleTrackView>(subtitleService.AllSubtitleTracks);
                }
            }
        }
        SubtitleTrackView ParseSingleSubtitle(string path)
        {
            var fs = File.OpenRead(path);
            var parser = new SubtitlesParser.Classes.Parsers.SrtParser();
            var items = parser.ParseStream(fs, System.Text.Encoding.UTF8);
            fs.Close();
            var trackItems = new List<ParsedSubtitleItem>();
            var index = 0;
            foreach (var it in items)
            {
                index++;
                trackItems.Add(new ParsedSubtitleItem
                {
                    LineNumber = index,
                    StartTime = TimeSpan.FromMilliseconds(it.StartTime),
                    EndTime = TimeSpan.FromMilliseconds(it.EndTime),
                    Lines = it.Lines
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
        void MediaService_OnTimeChanged(TimeSpan current, TimeSpan total)
        {
            subtitleState.CurrentTime = current;
            subtitleState.UpdateSubtitleText();
            CurrentTime = current.ToString(@"hh\:mm\:ss");
            TotalTime = total.ToString(@"hh\:mm\:ss");
        }
        void MediaService_OnStopped()
        {
            subtitleState.CurrentTime = TimeSpan.Zero;
            CurrentTime = "00:00:00";
            TotalTime = "00:00:00";
        }
        void JumpToPreviousSubtitle()
        {
            subtitleState.JumpToPrevious(subtitleState.FirstSubtitleTrack);
            if (subtitleState.CurrentTime != TimeSpan.Zero) mediaService.Seek(subtitleState.CurrentTime);
        }
        void JumpToNextSubtitle()
        {
            subtitleState.JumpToNext(subtitleState.FirstSubtitleTrack);
            mediaService.Seek(subtitleState.CurrentTime);
        }
        void ExecuteGotoTime()
        {
            var target = gotoTimeService.ParseGotoTime(GotoTimeInput, mediaService.GetLength());
            mediaService.Seek(target);
        }
    }
}
