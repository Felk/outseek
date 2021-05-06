using System;
using Outseek.AvaloniaClient.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Outseek.AvaloniaClient.SharedViewModels
{
    public class TimelineState : ViewModelBase
    {
        // TODO don't use singleton
        public static readonly TimelineState Instance = new();

        [Reactive] public double DevicePixelsPerSecond { get; set; } = 1;

        [Reactive] public double Start { get; set; } = 0;
        [Reactive] public double End { get; set; } = TimeSpan.FromHours(3).TotalSeconds;
        [Reactive] public double Position { get; set; } = 0;
        [Reactive] public double Step { get; set; } = 1 / 30d; // assuming 30fps

        public TimelineState()
        {
            this.WhenAnyValue(t => t.Position, t => t.DevicePixelsPerSecond)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(PositionScaled)));
        }

        public double PositionScaled
        {
            get => Position * DevicePixelsPerSecond;
            set => Position = value / DevicePixelsPerSecond;
        }
    }
}
