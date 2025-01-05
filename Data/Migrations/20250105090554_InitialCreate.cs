using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmoothVideoPlayer.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WordTranslationRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EngWord = table.Column<string>(type: "TEXT", nullable: true),
                    WordRu = table.Column<string>(type: "TEXT", nullable: true),
                    SubtitleEngContext = table.Column<string>(type: "TEXT", nullable: true),
                    SubtitleRuContext = table.Column<string>(type: "TEXT", nullable: true),
                    MoviePath = table.Column<string>(type: "TEXT", nullable: true),
                    ScreenshotPath = table.Column<string>(type: "TEXT", nullable: true),
                    SubtitlesEngPath = table.Column<string>(type: "TEXT", nullable: true),
                    SubtitlesRuPath = table.Column<string>(type: "TEXT", nullable: true),
                    AudioPath = table.Column<string>(type: "TEXT", nullable: true),
                    SubRuKey = table.Column<string>(type: "TEXT", nullable: true),
                    SubEngKey = table.Column<string>(type: "TEXT", nullable: true),
                    DateAddedToAnki = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateAdded = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateUpdated = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordTranslationRecords", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WordTranslationRecords");
        }
    }
}
