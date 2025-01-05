using SmoothVideoPlayer.Models;
using System.Threading.Tasks;

namespace SmoothVideoPlayer.Services.AddWord
{
    public interface IWordRepository
    {
        Task AddAsync(WordTranslationRecord record);
    }
} 