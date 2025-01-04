using System.Windows;

namespace SmoothVideoPlayer.Views.AddWord
{
    public partial class AddWordOverlayWindow : Window
    {
        public AddWordOverlayWindow()
        {
            InitializeComponent();
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
            MessageBox.Show("Word added: " + EnglishWordTextBox.Text);
        }
    }
} 