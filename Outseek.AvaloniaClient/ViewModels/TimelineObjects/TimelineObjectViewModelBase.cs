using System.Threading.Tasks;

namespace Outseek.AvaloniaClient.ViewModels.TimelineObjects
{
    public abstract class TimelineObjectViewModelBase : ViewModelBase
    {
        public abstract Task Refresh();
    }
}
