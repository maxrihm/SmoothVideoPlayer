using SmoothVideoPlayer.Models;

namespace SmoothVideoPlayer.Services.AddWord
{
    public interface IWordRepository
    {
        void Add(WordTranslationRecord record);
    }
} 