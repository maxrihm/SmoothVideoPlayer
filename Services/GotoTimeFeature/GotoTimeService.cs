using System;

namespace SmoothVideoPlayer.Services.GotoTimeFeature
{
    public class GotoTimeService : IGotoTimeService
    {
        public TimeSpan ParseGotoTime(string input, TimeSpan videoLength)
        {
            if (string.IsNullOrWhiteSpace(input)) return TimeSpan.Zero;
            var parts = input.Split(':');
            var minutes = 0;
            var seconds = 0;
            if (parts.Length == 1)
            {
                int.TryParse(parts[0], out minutes);
            }
            else if (parts.Length == 2)
            {
                int.TryParse(parts[0], out minutes);
                int.TryParse(parts[1], out seconds);
            }
            var total = minutes * 60 + seconds;
            if (total < 0) total = 0;
            if (total > videoLength.TotalSeconds) total = (int)videoLength.TotalSeconds;
            return TimeSpan.FromSeconds(total);
        }
    }
} 