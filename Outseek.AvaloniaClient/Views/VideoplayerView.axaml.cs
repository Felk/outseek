using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using LibVLCSharp.Avalonia;
using Outseek.AvaloniaClient.ViewModels;

namespace Outseek.AvaloniaClient.Views;

public partial class VideoplayerView : UserControl
{
    public VideoplayerView()
    {
        InitializeComponent();
        InitializeComponentCustom();

        AddHandler(DragDrop.DropEvent, Drop);
        AddHandler(DragDrop.DragOverEvent, DragOver);
    }

    private void InitializeComponentCustom()
    {
        // TODO properly figure out the video view lifetime requirements.
        // Initializing the media too early causes the video to be played in a separate window
        VideoView videoView = this.FindControl<VideoView>("VideoView")!;
        videoView.Initialized += (sender, args) =>
        {
            // TODO re-enable after there's a fix for LibVLCSharp.Avalonia: https://code.videolan.org/videolan/LibVLCSharp/-/issues/598
            // ((VideoplayerViewModel) DataContext!).InitPlayer(videoView);
        };
    }

    private static void DragOver(object? sender, DragEventArgs e)
    {
        if (e.Data.GetFiles() == null ||
            e.Data.GetFiles()!.Count() > 1)
        {
            e.DragEffects = DragDropEffects.None;
        }
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        IStorageItem? file = e.Data.GetFiles()?.FirstOrDefault();
        if (file == null) return;
        ((VideoplayerViewModel) DataContext!).MediaState.Filename = file.Path.AbsolutePath;
    }
}
