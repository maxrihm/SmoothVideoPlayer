using System;
using SmoothVideoPlayer.Services;

namespace SmoothVideoPlayer.Services.TimeJumpFeature
{
    public class TimeJumpService : ITimeJumpService
    {
        readonly IMediaService mediaService;

        public TimeJumpService(IMediaService mediaService)
        {
            this.mediaService = mediaService;
        }

        public void JumpSeconds(int seconds)
        {
            var current = mediaService.GetCurrentPosition();
            var length = mediaService.GetLength();
            var newPosition = current + TimeSpan.FromSeconds(seconds);
            if (newPosition < TimeSpan.Zero) newPosition = TimeSpan.Zero;
            if (newPosition > length) newPosition = length;
            mediaService.Seek(newPosition);
        }
    }
} 