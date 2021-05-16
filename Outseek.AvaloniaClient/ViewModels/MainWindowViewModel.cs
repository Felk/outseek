using Outseek.AvaloniaClient.SharedViewModels;

namespace Outseek.AvaloniaClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public TimelineViewModel TimelineViewModel { get; }
        public VideoplayerViewModel VideoplayerViewModel { get; }
        public TimelineState TimelineState { get; }
        public MediaState MediaState { get; }

        public MainWindowViewModel()
        {
            TimelineState = new TimelineState();
            MediaState = new MediaState();

            TimelineViewModel = new TimelineViewModel(TimelineState);
            VideoplayerViewModel = new VideoplayerViewModel(TimelineState, MediaState);
        }
    }
}
