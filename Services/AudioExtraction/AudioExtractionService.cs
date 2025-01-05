using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace SmoothVideoPlayer.Services.AudioExtraction
{
    public class AudioExtractionService : IAudioExtractionService
    {
        public async Task<string> ExtractAudioAsync(string inputFilePath, TimeSpan start, TimeSpan duration, int audioTrackIndex)
        {
            var folder = @"C:\Users\morge\OneDrive\Translations\Audio";
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            var guid = Guid.NewGuid().ToString();
            var outPath = Path.Combine(folder, guid + ".mp3");
            var ss = $"{start.TotalSeconds:0.###}";
            var ts = $"{duration.TotalSeconds:0.###}";
            var ffmpeg = "ffmpeg";
            var args = $"-ss {ss} -i \"{inputFilePath}\" -t {ts} -map 0:a:{audioTrackIndex} -vn -c:a libmp3lame -q:a 3 -y \"{outPath}\"";
            var psi = new ProcessStartInfo
            {
                FileName = ffmpeg,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            await Task.Run(() =>
            {
                using (var p = new Process { StartInfo = psi })
                {
                    p.OutputDataReceived += (s, e) => { };
                    p.ErrorDataReceived += (s, e) => { };
                    p.Start();
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                    p.WaitForExit();
                }
            });
            if (!File.Exists(outPath)) return null;
            return outPath;
        }
    }
} 