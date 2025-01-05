using System.Windows;
using SmoothVideoPlayer.Services.AddWord;

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
        public void ClearFields()
        {
            EnglishWordTextBox.Text = "";
            WordRuTextBox.Text = "";
            SubtitleEngContextTextBox.Text = "";
            SubtitleRuContextTextBox.Text = "";
        }
        void AddButton_Click(object sender, RoutedEventArgs e)
        {
            service.AddWord(
                EnglishWordTextBox.Text,
                WordRuTextBox.Text,
                SubtitleEngContextTextBox.Text,
                SubtitleRuContextTextBox.Text
            );
            ClearFields();
            Hide();
        }
    }
} 