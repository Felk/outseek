using System.Threading;
using System.Threading.Tasks;

namespace Outseek.AvaloniaClient.ViewModels.TimelineObjects;

public abstract class TimelineObjectViewModelBase : ViewModelBase
{
    /// <summary>
    /// Refreshes the timeline object view model by re-reading its respective timeline object's data.
    /// <para>
    /// WARNING: If the implementation includes reading an async stream, interacting with the ui _must_ be wrapped in
    /// <c>Dispatcher.UIThread.Post(...)</c>, because there is no guarantee what thread the async enumerable is processed from.
    /// </para>
    /// </summary>
    public abstract Task Refresh(CancellationToken cancellationToken);
}
