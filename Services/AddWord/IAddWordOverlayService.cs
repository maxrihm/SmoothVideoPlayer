namespace SmoothVideoPlayer.Services.AddWord
{
    public interface IAddWordOverlayService
    {
        bool IsOverlayOpen { get; }
        void ToggleOverlay();
        void CloseOverlay();
        void AddWord(
            string eng,
            string ru,
            string engContext,
            string ruContext,
            string engSubKey,
            string ruSubKey,
            string engSubPath,
            string ruSubPath,
            string moviePath
        );
    }
}
