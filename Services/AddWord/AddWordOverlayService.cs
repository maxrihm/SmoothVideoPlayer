using System;
using System.Threading.Tasks;
using SmoothVideoPlayer.Models;
using SmoothVideoPlayer.Services.AudioExtraction;
using SmoothVideoPlayer.Services.OverlayManager;

namespace SmoothVideoPlayer.Services.AddWord
{
    public class AddWordOverlayService : IAddWordOverlayService
    {
        readonly IWordRepository repository;
        readonly IMediaService mediaService;
        readonly IAudioExtractionService audioExtractionService;
        readonly IOverlayManager overlayManager;
        bool isOverlayOpen;
        Views.AddWord.AddWordOverlayWindow window;
        public bool IsOverlayOpen => isOverlayOpen;

        public AddWordOverlayService(IWordRepository repository, IMediaService mediaService, IOverlayManager overlayManager)
        {
            this.repository = repository;
            this.mediaService = mediaService;
            audioExtractionService = new AudioExtractionService();
            this.overlayManager = overlayManager;
        }

        public void CloseOverlay()
        {
            if (isOverlayOpen && window != null)
            {
                window.ClearFields();
                window.Hide();
                isOverlayOpen = false;
                overlayManager.UnregisterOverlay();
            }
        }

        public void ToggleOverlay()
        {
            if (isOverlayOpen)
            {
                CloseOverlay();
            }
            else
            {
                if (window == null) window = new Views.AddWord.AddWordOverlayWindow(this);
                var s = SubtitleStateService.Instance;
                var w = new WordTranslationRecord
                {
                    EngWord = s.FirstSubtitleText,
                    WordRu = "",
                    SubtitleEngContext = s.FirstSubtitleText,
                    SubtitleRuContext = s.SecondSubtitleText,
                    SubEngKey = s.CurrentFirstSubtitleLineNumber > 0 ? s.CurrentFirstSubtitleLineNumber.ToString() : "",
                    SubRuKey = s.CurrentSecondSubtitleLineNumber > 0 ? s.CurrentSecondSubtitleLineNumber.ToString() : "",
                    SubtitlesEngPath = s.FirstSubtitleTrack != null ? s.FirstSubtitleTrack.FilePath : "",
                    SubtitlesRuPath = s.SecondSubtitleTrack != null ? s.SecondSubtitleTrack.FilePath : "",
                    MoviePath = s.CurrentVideoFilePath
                };
                var screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
                var margin = 10;
                window.Left = screenWidth - window.Width - margin;
                window.Top = margin;
                window.FillFields(w);
                window.Show();
                isOverlayOpen = true;
                overlayManager.RegisterOverlay();
            }
        }

        public async void AddWord(
            string eng,
            string ru,
            string engContext,
            string ruContext,
            string engSubKey,
            string ruSubKey,
            string engSubPath,
            string ruSubPath,
            string moviePath
        )
        {
            var s = SubtitleStateService.Instance;
            var index = s.CurrentFirstSubtitleLineNumber - 1;
            var track = s.FirstSubtitleTrack;
            var start = TimeSpan.Zero;
            var duration = TimeSpan.Zero;
            if (track != null && index >= 0 && index < track.ParsedSubtitles.Count)
            {
                start = track.ParsedSubtitles[index].StartTime;
                var end = track.ParsedSubtitles[index].EndTime;
                duration = end - start;
            }
            var record = new WordTranslationRecord
            {
                EngWord = eng,
                WordRu = ru,
                SubtitleEngContext = engContext,
                SubtitleRuContext = ruContext,
                SubEngKey = engSubKey,
                SubRuKey = ruSubKey,
                SubtitlesEngPath = engSubPath,
                SubtitlesRuPath = ruSubPath,
                MoviePath = s.CurrentVideoFilePath,
                DateAdded = DateTime.Now
            };
            var screenshotPath = mediaService.TakeSnapshot(@"C:\Users\morge\OneDrive\Translations\Screenshots");
            record.ScreenshotPath = screenshotPath;
            var audioIndex = mediaService.SelectedAudioFfmpegIndex ?? 0;
            var audioPath = await audioExtractionService.ExtractAudioAsync(record.MoviePath, start, duration, audioIndex);
            record.AudioPath = audioPath;
            if (record.AudioPath != null) await repository.AddAsync(record);
        }
    }
}