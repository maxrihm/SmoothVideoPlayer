using System.Windows;
using System.Windows.Controls;

namespace SmoothVideoPlayer.Views.Controls
{
    public partial class SelectableSubtitleView : UserControl
    {
        public static readonly DependencyProperty SubtitleTextProperty =
            DependencyProperty.Register(nameof(SubtitleText), typeof(string), typeof(SelectableSubtitleView), new PropertyMetadata(string.Empty));

        public string SubtitleText
        {
            get => (string)GetValue(SubtitleTextProperty);
            set => SetValue(SubtitleTextProperty, value);
        }

        public SelectableSubtitleView()
        {
            InitializeComponent();
        }
    }
}
