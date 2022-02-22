using System.Collections.ObjectModel;
using Outseek.API;

namespace Outseek.AvaloniaClient.ViewModels
{
    public class TimelineProcessorExplorerViewModel : ViewModelBase
    {
        public ObservableCollection<ITimelineProcessor> Processors { get; } = new();

        public TimelineProcessorExplorerViewModel()
        {
        }
    }
}
