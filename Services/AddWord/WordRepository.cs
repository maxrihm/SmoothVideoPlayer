using SmoothVideoPlayer.Data;
using SmoothVideoPlayer.Models;
using System.Threading.Tasks;

namespace SmoothVideoPlayer.Services.AddWord
{
    public class WordRepository : IWordRepository
    {
        readonly WordTranslationDbContext context;
        public WordRepository(WordTranslationDbContext context)
        {
            this.context = context;
        }
        public async Task AddAsync(WordTranslationRecord record)
        {
            context.WordTranslationRecords.Add(record);
            await context.SaveChangesAsync();
        }
    }
} 