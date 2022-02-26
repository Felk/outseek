using System;
using System.Reactive;
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
        [Reactive] public double ViewportHoverPosition { get; set; } = 0; // some control must set this
        public double ViewportWidthScaled => ViewportWidth / ZoomScale;
        public double ScrollableWidth => ViewportWidth / ZoomScale - ViewportWidth;
        public double DevicePixelsPerSecond => ViewportWidthScaled / (End - Start);
        public double PlaybackPositionScaled => PlaybackPosition * DevicePixelsPerSecond - ScrollOffset * ScrollableWidth;
        public double EndScaled => End * DevicePixelsPerSecond;
        public Vector ScrollPositionScaled => new(ScrollOffset * ScrollableWidth, 0);
        public Size ViewportWidthScaledAsSize => new(ViewportWidthScaled, 0);

        //
        // commands
        //
        public ReactiveCommand<Vector, Unit> Scroll { get; }
        public ReactiveCommand<Vector, Unit> Zoom { get; }

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
            this.WhenAnyValue(t => t.ViewportWidthScaled)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(ViewportWidthScaledAsSize)));

            Scroll = ReactiveCommand.Create((Vector delta) =>
            {
                const double scrollPercentage = 0.1;
                ScrollOffset = Math.Clamp(ScrollOffset - Math.Sign(delta.Y) * ZoomScale * scrollPercentage, 0, 1);
            });
            Zoom = ReactiveCommand.Create((Vector delta) =>
            {
                const double zoomFactor = 1.05;

                double prevZoomScale = ZoomScale;
                ZoomScale = Math.Clamp(delta.Y < 0 ? ZoomScale * zoomFactor : ZoomScale / zoomFactor, 0, 1);

                double fixpointWithinViewport = Math.Clamp(ViewportHoverPosition, 0, ViewportWidth);
                // fixpointPrev = ScrollOffsetPrev * ScrollableWidthPrev + fixpointWithinViewport;
                // offsetAtFixpointPrev = fixpointPrev / ViewportWidthScaledPrev;
                // fixpointNew = ScrollOffsetNew * ScrollableWidthNew + fixpointWithinViewport;
                // offsetAtFixpointNew = fixpointNew / ViewportWidthScaledNew;
                //
                // The following formula comes from the equation "offsetAtFixpointPrev = offsetAtFixpointNew"
                // by un-substituting everything and solving for ScrollOffsetNew:
                double newOffset = (fixpointWithinViewport * (ZoomScale - prevZoomScale) +
                                    ScrollOffset * ViewportWidth * (prevZoomScale - 1)) /
                                   (ViewportWidth * (ZoomScale - 1));
                ScrollOffset = Math.Clamp(double.IsNaN(newOffset) ? 0 : newOffset, 0, 1);
            });
        }
    }
}
