using System;
using Avalonia;
using Outseek.AvaloniaClient.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Outseek.AvaloniaClient.SharedViewModels
{
    public class TimelineState : ViewModelBase
    {
        //
        // logical values, in seconds:
        //
        [Reactive] public double Start { get; set; } = 0;
        [Reactive] public double End { get; set; } = TimeSpan.FromMinutes(10).TotalSeconds;
        [Reactive] public double PlaybackPosition { get; set; } = 0;
        [Reactive] public double Step { get; set; } = 1 / 30d; // assuming 30fps
        
        //
        // viewport values, generic:
        //
        // <1 => zoomed into some part of the timeline
        // =1 => video duration perfectly stretches across the available width
        // >1 => zoom out beyond the video's bounds
        [Reactive] public double ZoomScale { get; set; } = 1;
        // 0 => scrolled all the way to the left
        // 1 => scrolled all the way to the right
        [Reactive] public double ScrollOffset { get; set; } = 0;
        
        //
        // viewport values, in device pixels:
        //
        [Reactive] public double ViewportWidth { get; set; } = 1; // some control must set this
        public double ViewportWidthScaled => ViewportWidth / ZoomScale;
        public double ScrollableWidth => ViewportWidth / ZoomScale - ViewportWidth;
        public double DevicePixelsPerSecond => ViewportWidthScaled / (End - Start);
        public double PlaybackPositionScaled => PlaybackPosition * DevicePixelsPerSecond - ScrollOffset * ScrollableWidth;
        public double EndScaled => End * DevicePixelsPerSecond;
        public Vector ScrollPositionScaled => new(ScrollOffset * ScrollableWidth, 0);

        public TimelineState()
        {
            this.WhenAnyValue(t => t.ViewportWidth, t => t.ZoomScale)
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(ViewportWidthScaled));
                    this.RaisePropertyChanged(nameof(ScrollableWidth));
                });
            this.WhenAnyValue(t => t.ViewportWidthScaled, t => t.Start, t => t.End)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(DevicePixelsPerSecond)));
            this.WhenAnyValue(t => t.PlaybackPosition, t => t.DevicePixelsPerSecond, t => t.ScrollOffset, t => t.ScrollableWidth)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(PlaybackPositionScaled)));
            this.WhenAnyValue(t => t.End, t => t.DevicePixelsPerSecond)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(EndScaled)));
            this.WhenAnyValue(t => t.ScrollOffset, t => t.ScrollableWidth)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(ScrollPositionScaled)));
        }
    }
}
