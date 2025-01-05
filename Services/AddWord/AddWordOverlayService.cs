using System;
using System.Windows;
using SmoothVideoPlayer.Views.AddWord;
using SmoothVideoPlayer.Models;
using SmoothVideoPlayer.Services;

namespace SmoothVideoPlayer.Services.AddWord
{
    public class AddWordOverlayService : IAddWordOverlayService
    {
        readonly IWordRepository repository;
        AddWordOverlayWindow window;
        bool isOpen;

        public AddWordOverlayService(IWordRepository repository)
        {
            this.repository = repository;
        }

        public void ToggleOverlay()
        {
            if (isOpen)
            {
                window.Hide();
                window.ClearFields();
                isOpen = false;
            }
            else
            {
                if (window == null) window = new AddWordOverlayWindow(this);
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
                window.FillFields(w);
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                var margin = 10;
                window.Left = screenWidth - window.Width - margin;
                window.Top = margin;
                window.Show();
                isOpen = true;
            }
        }

        public void AddWord(
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
            var r = new WordTranslationRecord
            {
                EngWord = eng,
                WordRu = ru,
                SubtitleEngContext = engContext,
                SubtitleRuContext = ruContext,
                SubEngKey = engSubKey,
                SubRuKey = ruSubKey,
                SubtitlesEngPath = engSubPath,
                SubtitlesRuPath = ruSubPath,
                MoviePath = moviePath,
                DateAdded = DateTime.Now
            };
            repository.Add(r);
        }
    }
}