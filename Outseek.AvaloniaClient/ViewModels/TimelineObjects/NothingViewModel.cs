using System.Threading;
using System.Threading.Tasks;

namespace Outseek.AvaloniaClient.ViewModels.TimelineObjects;

public class NothingViewModel : TimelineObjectViewModelBase
{
    public override Task Refresh(CancellationToken cancellationToken) => Task.CompletedTask;
}
