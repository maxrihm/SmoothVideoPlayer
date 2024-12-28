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
        SubtitleTrackView selectedSubtitleTrack;
        int currentSubtitleIndex;

        public async Task<List<SubtitleTrackView>> ExtractAndParseSubtitleTracksAsync(string filePath, int totalSubtitleTracks)
        {
            var tracks = new List<SubtitleTrackView>();
            for (int i = 0; i < totalSubtitleTracks; i++)
            {
                try
                {
                    var extractedPath = await ExtractSubtitlesForTrackAsync(filePath, i);
                    if (!string.IsNullOrEmpty(extractedPath) && File.Exists(extractedPath))
                    {
                        var parsed = ParseSubtitles(extractedPath);
                        var trackName = $"Subtitle Track {i}";
                        tracks.Add(new SubtitleTrackView
                        {
                            Name = trackName,
                            FilePath = extractedPath,
                            ParsedSubtitles = parsed
                        });
                    }
                }
                catch
                {
                }
            }
            return tracks;
        }

        async Task<string> ExtractSubtitlesForTrackAsync(string inputPath, int trackIndex)
        {
            var folder = Path.GetDirectoryName(inputPath) ?? Environment.CurrentDirectory;
            var outPath = Path.Combine(folder, $"extracted_track_{trackIndex}_{Guid.NewGuid():N}.srt");
            const string ffmpeg = "ffmpeg";
            var args = $"-i \"{inputPath}\" -map 0:s:{trackIndex} -c:s srt -y \"{outPath}\"";
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
            if (File.Exists(outPath))
            {
                return outPath;
            }
            return null;
        }

        List<ParsedSubtitleItem> ParseSubtitles(string srtPath)
        {
            var list = new List<ParsedSubtitleItem>();
            var parser = new SrtParser();
            using (var fs = File.OpenRead(srtPath))
            {
                var items = parser.ParseStream(fs, Encoding.UTF8);
                foreach (var item in items)
                {
                    list.Add(new ParsedSubtitleItem
                    {
                        StartTime = TimeSpan.FromMilliseconds(item.StartTime),
                        EndTime = TimeSpan.FromMilliseconds(item.EndTime),
                        Lines = item.Lines
                    });
                }
            }
            return list;
        }

        public SubtitleTrackView GetSelectedSubtitleTrack()
        {
            return selectedSubtitleTrack;
        }

        public void SetSelectedSubtitleTrack(SubtitleTrackView track)
        {
            selectedSubtitleTrack = track;
            currentSubtitleIndex = 0;
        }

        public string GetSubtitleForTime(TimeSpan currentTime)
        {
            if (selectedSubtitleTrack == null)
            {
                return string.Empty;
            }
            var subs = selectedSubtitleTrack.ParsedSubtitles;
            if (subs == null || subs.Count == 0)
            {
                return string.Empty;
            }
            while (currentSubtitleIndex > 0 && currentTime < subs[currentSubtitleIndex].StartTime)
            {
                currentSubtitleIndex--;
            }
            while (currentSubtitleIndex < subs.Count - 1 && currentTime > subs[currentSubtitleIndex].EndTime)
            {
                currentSubtitleIndex++;
            }
            var item = subs[currentSubtitleIndex];
            if (currentTime >= item.StartTime && currentTime < item.EndTime)
            {
                return string.Join("\n", item.Lines);
            }
            return string.Empty;
        }
    }
} 