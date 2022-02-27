using Outseek.AvaloniaClient.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace Outseek.AvaloniaClient.SharedViewModels;

public class MediaState : ViewModelBase
{
    [Reactive] public string? Filename { get; set; } = null;
    [Reactive] public bool IsPlaying { get; set; } = false;
}
