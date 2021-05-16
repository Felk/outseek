using System.Threading.Tasks;
using ReactiveUI.Fody.Helpers;

namespace Outseek.AvaloniaClient.ViewModels.TimelineObjects
{
    public class ErrorViewModel : TimelineObjectViewModelBase
    {
        [Reactive] public string? Error { get; set; }

        public override Task Refresh() => Task.CompletedTask;
    }
}
