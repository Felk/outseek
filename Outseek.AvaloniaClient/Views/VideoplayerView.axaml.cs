using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LibVLCSharp.Avalonia;
using Outseek.AvaloniaClient.SharedViewModels;
using Outseek.AvaloniaClient.ViewModels;

namespace Outseek.AvaloniaClient.Views
{
    public class VideoplayerView : UserControl
    {
        public VideoplayerView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            VideoplayerViewModel vm = new(TimelineState.Instance, MediaState.Instance);
            DataContext = vm;
            
            // TODO properly figure out the video view lifetime requirements.
            // Initializing the media too early causes the video to be played in a separate window
            VideoView videoView = this.FindControl<VideoView>("VideoView");
            videoView.Initialized += (sender, args) =>
            {
                vm.InitPlayer(videoView);
            };
        }
    }
}
