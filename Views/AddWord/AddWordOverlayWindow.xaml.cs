using System.Windows;
using SmoothVideoPlayer.Services.AddWord;
using SmoothVideoPlayer.Models;

namespace SmoothVideoPlayer.Views.AddWord
{
    public partial class AddWordOverlayWindow : Window
    {
        readonly IAddWordOverlayService service;

        public AddWordOverlayWindow(IAddWordOverlayService service)
        {
            InitializeComponent();
            this.service = service;
        }

        void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            var margin = 10;
            Left = SystemParameters.PrimaryScreenWidth - ActualWidth - margin;
            Top = margin;
        }

        public void FillFields(WordTranslationRecord record)
        {
            EnglishWordTextBox.Text = record.EngWord ?? "";
            WordRuTextBox.Text = record.WordRu ?? "";
            SubtitleEngContextTextBox.Text = record.SubtitleEngContext ?? "";
            SubtitleRuContextTextBox.Text = record.SubtitleRuContext ?? "";
            SubEngKeyTextBox.Text = record.SubEngKey ?? "";
            SubRuKeyTextBox.Text = record.SubRuKey ?? "";
            SubtitlesEngPathTextBox.Text = record.SubtitlesEngPath ?? "";
            SubtitlesRuPathTextBox.Text = record.SubtitlesRuPath ?? "";
        }

        public void ClearFields()
        {
            EnglishWordTextBox.Text = "";
            WordRuTextBox.Text = "";
            SubtitleEngContextTextBox.Text = "";
            SubtitleRuContextTextBox.Text = "";
            SubEngKeyTextBox.Text = "";
            SubRuKeyTextBox.Text = "";
            SubtitlesEngPathTextBox.Text = "";
            SubtitlesRuPathTextBox.Text = "";
        }

        void AddButton_Click(object sender, RoutedEventArgs e)
        {
            service.AddWord(
                EnglishWordTextBox.Text,
                WordRuTextBox.Text,
                SubtitleEngContextTextBox.Text,
                SubtitleRuContextTextBox.Text,
                SubEngKeyTextBox.Text,
                SubRuKeyTextBox.Text,
                SubtitlesEngPathTextBox.Text,
                SubtitlesRuPathTextBox.Text,
                ""
            );
            ClearFields();
            Hide();
        }
    }
}
