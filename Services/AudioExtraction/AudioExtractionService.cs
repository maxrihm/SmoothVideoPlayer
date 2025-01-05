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
            var fileName = Guid.NewGuid().ToString() + ".mp3";
            var outputPath = Path.Combine(folder, fileName);
            var startSeconds = $"{start.TotalSeconds:0.###}";
            var lengthSeconds = $"{duration.TotalSeconds:0.###}";
            var exe = "ffmpeg";
            var args = $"-ss {startSeconds} -i \"{inputFilePath}\" -t {lengthSeconds} -map 0:a:{audioTrackIndex} -vn -c:a libmp3lame -q:a 3 -y \"{outputPath}\"";
            var psi = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
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
            if (!File.Exists(outputPath)) return null;
            return outputPath;
        }
    }
} 