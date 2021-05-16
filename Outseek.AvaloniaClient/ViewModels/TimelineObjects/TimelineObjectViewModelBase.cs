using System.Threading.Tasks;
using ReactiveUI.Fody.Helpers;

namespace Outseek.AvaloniaClient.ViewModels.TimelineObjects
{
    public abstract class TimelineObjectViewModelBase : ViewModelBase
    {
        [Reactive] public string? Error { get; set; }
        
        public abstract Task Refresh();
    }
}
