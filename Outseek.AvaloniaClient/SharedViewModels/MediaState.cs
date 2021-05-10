using Outseek.AvaloniaClient.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace Outseek.AvaloniaClient.SharedViewModels
{
    public class MediaState : ViewModelBase
    {
        // TODO don't use singleton
        public static readonly MediaState Instance = new(); 
        
        [Reactive] public string? Filename { get; set; } = null;
    }
}
