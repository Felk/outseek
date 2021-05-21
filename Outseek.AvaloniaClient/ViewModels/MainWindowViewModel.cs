using Outseek.AvaloniaClient.SharedViewModels;

namespace Outseek.AvaloniaClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public TimelineViewModel TimelineViewModel { get; }
        public VideoplayerViewModel VideoplayerViewModel { get; }
        public TimelineProcessorParamsViewModel TimelineProcessorParamsViewModel { get; }
        public TimelineState TimelineState { get; }
        public MediaState MediaState { get; }
        public TimelineProcessorsState TimelineProcessorsState { get; }

        public MainWindowViewModel()
        {
            TimelineState = new TimelineState();
            MediaState = new MediaState();
            TimelineProcessorsState = new TimelineProcessorsState();

            TimelineViewModel = new TimelineViewModel(TimelineState, TimelineProcessorsState);
            VideoplayerViewModel = new VideoplayerViewModel(TimelineState, MediaState);
            TimelineProcessorParamsViewModel = new TimelineProcessorParamsViewModel(TimelineProcessorsState);
        }
    }
}
