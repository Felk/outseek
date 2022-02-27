using System.Collections.ObjectModel;
using Outseek.AvaloniaClient.ViewModels;

namespace Outseek.AvaloniaClient.SharedViewModels;

public class WorkingAreaState : ViewModelBase
{
    public ObservableCollection<ObservableRange> Segments { get; } = new();
}
