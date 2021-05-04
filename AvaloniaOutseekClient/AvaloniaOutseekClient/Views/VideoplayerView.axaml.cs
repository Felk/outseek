using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaOutseekClient.SharedViewModels;
using AvaloniaOutseekClient.ViewModels;
using LibVLCSharp.Avalonia;

namespace AvaloniaOutseekClient.Views
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
            VideoplayerViewModel vm = new(TimelineState.Instance);
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
