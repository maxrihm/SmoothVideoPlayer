using System;

namespace SmoothVideoPlayer.Services.TimeJumpFeature
{
    public interface ITimeJumpService
    {
        void JumpSeconds(int seconds);
    }
} 