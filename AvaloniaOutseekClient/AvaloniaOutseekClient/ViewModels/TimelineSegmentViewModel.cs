using System;
using AvaloniaOutseekClient.SharedViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AvaloniaOutseekClient.ViewModels
{
    public class TimelineSegmentViewModel : ViewModelBase
    {
        public TimelineState TimelineState { get; }
        
        [Reactive] public string? Text { get; set; }

        private double _from = 0;
        private double _to = 100;

        public TimelineSegmentViewModel() : this(new TimelineState())
        {
            // the default constructor is only used by the designer
        }

        public TimelineSegmentViewModel(TimelineState timelineState)
        {
            TimelineState = timelineState;
            TimelineState.WhenAnyValue(vm => vm.DevicePixelsPerSecond)
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(StepScaled));
                    this.RaisePropertyChanged(nameof(FromScaled));
                    this.RaisePropertyChanged(nameof(ToScaled));
                });
            TimelineState.WhenAnyValue(vm => vm.Step)
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(StepScaled));
                });
        }

        public double From
        {
            get => _from;
            set
            {
                if (Equals(value, _from)) return;
                this.RaisePropertyChanging();
                this.RaisePropertyChanging(nameof(FromScaled));
                _from = value;
                this.RaisePropertyChanged(nameof(FromScaled));
                this.RaisePropertyChanged();
            }
        }

        public double To
        {
            get => _to;
            set
            {
                if (Equals(value, _to)) return;
                this.RaisePropertyChanging();
                this.RaisePropertyChanging(nameof(ToScaled));
                _to = value;
                this.RaisePropertyChanged(nameof(ToScaled));
                this.RaisePropertyChanged();
            }
        }

        public double FromScaled
        {
            get => From * TimelineState.DevicePixelsPerSecond;
            set => From = value / TimelineState.DevicePixelsPerSecond;
        }

        public double ToScaled
        {
            get => To * TimelineState.DevicePixelsPerSecond;
            set => To = value / TimelineState.DevicePixelsPerSecond;
        }

        public double StepScaled
        {
            get => TimelineState.Step * TimelineState.DevicePixelsPerSecond;
            set => TimelineState.Step = value / TimelineState.DevicePixelsPerSecond;
        }
    }
}
