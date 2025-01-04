namespace SmoothVideoPlayer.ViewModels
{
    public class TranslatorOverlayViewModel : BaseViewModel
    {
        string url;
        public string Url
        {
            get => url;
            set
            {
                url = value;
                OnPropertyChanged();
            }
        }
        public TranslatorOverlayViewModel()
        {
            Url = "https://translate.yandex.com/en/translator/English-Russian";
        }
    }
}
