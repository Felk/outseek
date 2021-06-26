using System;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI.Fody.Helpers;

namespace Outseek.AvaloniaClient.ViewModels.TimelineObjects
{
    public class ErrorViewModel : TimelineObjectViewModelBase
    {
        [Reactive] public Exception? Exception { get; set; }
        [Reactive] public string? Error { get; set; }

        public override Task Refresh(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
