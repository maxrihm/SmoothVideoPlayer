namespace SmoothVideoPlayer.Services.AddWord
{
    public interface IAddWordOverlayService
    {
        void ToggleOverlay();
        void AddWord(string eng, string ru, string engContext, string ruContext);
    }
}
