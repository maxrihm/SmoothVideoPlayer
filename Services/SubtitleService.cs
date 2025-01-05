using SmoothVideoPlayer.Models;
using SubtitlesParser.Classes.Parsers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SmoothVideoPlayer.Services
{
    public class SubtitleService : ISubtitleService
    {
        Dictionary<SubtitleTrackView, int> trackIndexes = new Dictionary<SubtitleTrackView, int>();
        public List<SubtitleTrackView> AllSubtitleTracks { get; } = new List<SubtitleTrackView>();

        public async Task<SubtitleTrackView> ExtractAndParseSubtitleTrackAsync(
            string filePath,
            int trackIndex,
            string language,
            string description,
            string outputFolder)
        {
            if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);
            var safeLanguage = MakeSafe(language);
            var safeDescription = MakeSafe(description);
            var outPath = Path.Combine(outputFolder, $"track_{trackIndex}_{safeLanguage}_{safeDescription}.srt");
            var ffmpeg = "ffmpeg";
            var args = $"-i \"{filePath}\" -map 0:s:{trackIndex} -c:s srt -y \"{outPath}\"";
            var psi = new ProcessStartInfo
            {
                FileName = ffmpeg,
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            await Task.Run(() =>
            {
                using (var process = new Process { StartInfo = psi })
                {
                    process.OutputDataReceived += (s, e) => { };
                    process.ErrorDataReceived += (s, e) => { };
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                }
            });
            if (!File.Exists(outPath)) return null;
            var parsed = ParseSubtitles(outPath);
            var displayName = Path.GetFileNameWithoutExtension(outPath);
            var track = new SubtitleTrackView
            {
                Name = displayName,
                FilePath = outPath,
                ParsedSubtitles = parsed
            };
            return track;
        }

        public void AddSubtitleTrack(SubtitleTrackView track)
        {
            if (!AllSubtitleTracks.Contains(track))
            {
                AllSubtitleTracks.Add(track);
                if (!trackIndexes.ContainsKey(track))
                {
                    trackIndexes[track] = 0;
                }
            }
        }

        public string GetSubtitleForTime(TimeSpan currentTime, SubtitleTrackView track)
        {
            if (track == null) return string.Empty;
            if (track.ParsedSubtitles == null || track.ParsedSubtitles.Count == 0) return string.Empty;
            if (!trackIndexes.ContainsKey(track)) trackIndexes[track] = 0;
            var currentIndex = trackIndexes[track];
            var subs = track.ParsedSubtitles;
            while (currentIndex > 0 && currentTime < subs[currentIndex].StartTime) currentIndex--;
            while (currentIndex < subs.Count - 1 && currentTime > subs[currentIndex].EndTime) currentIndex++;
            trackIndexes[track] = currentIndex;
            var item = subs[currentIndex];
            if (currentTime >= item.StartTime && currentTime < item.EndTime)
            {
                return string.Join("\n", item.Lines);
            }
            return string.Empty;
        }

        public List<SubtitleTrackView> ParseSubtitlesInFolder(string folderPath)
        {
            var result = new List<SubtitleTrackView>();
            var files = Directory.GetFiles(folderPath, "*.srt");
            foreach (var f in files)
            {
                if (!File.Exists(f)) continue;
                var parsed = ParseSubtitles(f);
                var name = Path.GetFileNameWithoutExtension(f);
                var tv = new SubtitleTrackView
                {
                    Name = name,
                    FilePath = f,
                    ParsedSubtitles = parsed
                };
                result.Add(tv);
            }
            return result;
        }

        public string GetSubtitleFolderPath(string videoFilePath)
        {
            var name = Path.GetFileNameWithoutExtension(videoFilePath);
            var parent = Path.GetDirectoryName(videoFilePath) ?? Environment.CurrentDirectory;
            var folder = Path.Combine(parent, name + "_subs");
            return folder;
        }

        List<ParsedSubtitleItem> ParseSubtitles(string srtPath)
        {
            var list = new List<ParsedSubtitleItem>();
            var parser = new SrtParser();
            using (var fs = File.OpenRead(srtPath))
            {
                var items = parser.ParseStream(fs, Encoding.UTF8);
                var index = 0;
                foreach (var item in items)
                {
                    index++;
                    list.Add(new ParsedSubtitleItem
                    {
                        LineNumber = index,
                        StartTime = TimeSpan.FromMilliseconds(item.StartTime),
                        EndTime = TimeSpan.FromMilliseconds(item.EndTime),
                        Lines = item.Lines
                    });
                }
            }
            return list;
        }

        string MakeSafe(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "Unknown";
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                input = input.Replace(c, '_');
            }
            return input;
        }
    }
}
