using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using LibVLCSharp.Avalonia;
using Outseek.AvaloniaClient.ViewModels;

namespace Outseek.AvaloniaClient.Views;

public class VideoplayerView : UserControl
{
    public VideoplayerView()
    {
        InitializeComponent();

        AddHandler(DragDrop.DropEvent, Drop);
        AddHandler(DragDrop.DragOverEvent, DragOver);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        // TODO properly figure out the video view lifetime requirements.
        // Initializing the media too early causes the video to be played in a separate window
        VideoView videoView = this.FindControl<VideoView>("VideoView");
        videoView.Initialized += (sender, args) =>
        {
            ((VideoplayerViewModel) DataContext!).InitPlayer(videoView);
        };
    }

    private static void DragOver(object? sender, DragEventArgs e)
    {
        if (e.Data.GetFileNames() == null ||
            e.Data.GetFileNames()!.Count() > 1)
        {
            e.DragEffects = DragDropEffects.None;
        }
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        string? filename = e.Data.GetFileNames()?.FirstOrDefault();
        if (filename == null) return;
        ((VideoplayerViewModel) DataContext!).MediaState.Filename = filename;
    }
}
