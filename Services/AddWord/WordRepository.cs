using SmoothVideoPlayer.Data;
using SmoothVideoPlayer.Models;

namespace SmoothVideoPlayer.Services.AddWord
{
    public class WordRepository : IWordRepository
    {
        readonly WordTranslationDbContext context;
        public WordRepository(WordTranslationDbContext context)
        {
            this.context = context;
        }
        public void Add(WordTranslationRecord record)
        {
            context.WordTranslationRecords.Add(record);
            context.SaveChanges();
        }
    }
} 