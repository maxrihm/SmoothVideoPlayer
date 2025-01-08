namespace SmoothVideoPlayer.Services.Translator
{
    public interface ITranslatorOverlayService
    {
        bool IsOverlayOpen { get; }
        void ToggleOverlay();
    }

    public class TranslatorOverlayAggregatorService : ITranslatorOverlayService
    {
        readonly ITranslatorOverlayService yandexService;
        readonly ITranslatorOverlayService googleService;
        bool anyOverlayOpen;

        public bool IsOverlayOpen => anyOverlayOpen;

        public TranslatorOverlayAggregatorService(
            ITranslatorOverlayService yandexService,
            ITranslatorOverlayService googleService)
        {
            this.yandexService = yandexService;
            this.googleService = googleService;
        }

        public void ToggleOverlay()
        {
            if (!anyOverlayOpen)
            {
                yandexService.ToggleOverlay();
                googleService.ToggleOverlay();
                anyOverlayOpen = true;
            }
            else
            {
                yandexService.ToggleOverlay();
                googleService.ToggleOverlay();
                anyOverlayOpen = false;
            }
        }
    }
}
