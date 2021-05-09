using Avalonia;
using Outseek.AvaloniaClient.SharedViewModels;

namespace Outseek.AvaloniaClient.ViewModels
{
    public class ZoomAdjusterViewModel : ViewModelBase
    {
        public TimelineState TimelineState { get; }

        public Rect Bounds
        {
            set => TimelineState.ViewportWidth = value.Width;
        }

        public ZoomAdjusterViewModel(TimelineState timelineState)
        {
            TimelineState = timelineState;
        }

        public ZoomAdjusterViewModel() : this(new TimelineState())
        {
            // the default constructor is only used by the designer
        }
    }
}
