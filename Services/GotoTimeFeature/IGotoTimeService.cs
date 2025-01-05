using System;

namespace SmoothVideoPlayer.Services.GotoTimeFeature
{
    public interface IGotoTimeService
    {
        TimeSpan ParseGotoTime(string input, TimeSpan videoLength);
    }
} 