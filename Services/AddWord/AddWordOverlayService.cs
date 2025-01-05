using System;
using System.Windows;
using SmoothVideoPlayer.Views.AddWord;
using SmoothVideoPlayer.Models;

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
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                var margin = 10;
                window.Left = screenWidth - window.Width - margin;
                window.Top = margin;
                window.Show();
                isOpen = true;
            }
        }
        public void AddWord(string eng, string ru, string engContext, string ruContext)
        {
            var record = new WordTranslationRecord
            {
                EngWord = eng,
                WordRu = ru,
                SubtitleEngContext = engContext,
                SubtitleRuContext = ruContext,
                DateAdded = DateTime.Now
            };
            repository.Add(record);
        }
    }
} 