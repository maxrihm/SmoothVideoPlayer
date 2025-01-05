using Microsoft.EntityFrameworkCore;
using SmoothVideoPlayer.Models;

namespace SmoothVideoPlayer.Data
{
    public class WordTranslationDbContext : DbContext
    {
        public DbSet<WordTranslationRecord> WordTranslationRecords { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=C:\Users\morge\OneDrive\Translations\TranslationDB.db");
        }
    }
} 