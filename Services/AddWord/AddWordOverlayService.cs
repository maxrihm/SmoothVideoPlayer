using System.Windows;
using SmoothVideoPlayer.Views.AddWord;

namespace SmoothVideoPlayer.Services.AddWord
{
    public class AddWordOverlayService : IAddWordOverlayService
    {
        AddWordOverlayWindow window;
        bool isOpen;
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
                if (window == null)
                {
                    window = new AddWordOverlayWindow();
                    window.WindowStartupLocation = WindowStartupLocation.Manual;
                }
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                var margin = 10;
                window.Left = screenWidth - window.Width - margin;
                window.Top = margin;
                window.Show();
                isOpen = true;
            }
        }
    }
} 