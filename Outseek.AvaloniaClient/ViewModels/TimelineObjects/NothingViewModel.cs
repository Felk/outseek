using System.Threading.Tasks;

namespace Outseek.AvaloniaClient.ViewModels.TimelineObjects
{
    public class NothingViewModel : TimelineObjectViewModelBase
    {
        public override Task Refresh() => Task.CompletedTask;
    }
}
