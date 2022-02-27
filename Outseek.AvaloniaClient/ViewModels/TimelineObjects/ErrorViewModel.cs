using System.Threading;
using System.Threading.Tasks;
using ReactiveUI.Fody.Helpers;

namespace Outseek.AvaloniaClient.ViewModels.TimelineObjects;

public class ErrorViewModel : TimelineObjectViewModelBase
{
    [Reactive] public string? ErrorText { get; set; }

    public override Task Refresh(CancellationToken cancellationToken) => Task.CompletedTask;
}
