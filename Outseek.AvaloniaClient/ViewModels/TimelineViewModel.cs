using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Input;
using Outseek.API;
using Outseek.AvaloniaClient.SharedViewModels;
using ReactiveUI;

namespace Outseek.AvaloniaClient.ViewModels;

public class TimelineViewModel : ViewModelBase
{
    public TimelineState TimelineState { get; }
    public TimelineProcessorsState TimelineProcessorsState { get; }
    public ZoomAdjusterViewModel ZoomAdjusterViewModel { get; }
    public WorkingAreaViewModel WorkingAreaViewModel { get; }
    public WorkingAreaToolsViewModel WorkingAreaToolsViewModel { get; }

    public ObservableCollection<TimelineObjectViewModel> TimelineObjects { get; } = new();

    // TODO avoid duplicate code with TimelineObjectViewModel
    public ReactiveCommand<DragEventArgs, Unit> Drop { get; }
    public static Func<DragEventArgs, bool> CheckDropAllowed => e => e.Data.Contains("processor");

    public TimelineViewModel() : this(new TimelineState(), new TimelineProcessorsState(), new WorkingAreaViewModel(), new WorkingAreaToolsViewModel())
    {
        // the default constructor is only used by the designer
    }

    public TimelineViewModel(
        TimelineState timelineState,
        TimelineProcessorsState timelineProcessorsState,
        WorkingAreaViewModel workingAreaViewModel,
        WorkingAreaToolsViewModel workingAreaToolsViewModel)
    {
        TimelineState = timelineState;
        TimelineProcessorsState = timelineProcessorsState;
        ZoomAdjusterViewModel = new ZoomAdjusterViewModel(timelineState);
        WorkingAreaViewModel = workingAreaViewModel;
        WorkingAreaToolsViewModel = workingAreaToolsViewModel;

        Drop = ReactiveCommand.Create((DragEventArgs dragEventArgs) =>
        {
            var processor = (ITimelineProcessor) dragEventArgs.Data.Get("processor")!;

            IObservable<TimelineProcessContext> context = TimelineState
                .WhenAnyValue(s => s.Start, s => s.End)
                .Select(tpl => new TimelineProcessContext(tpl.Item1, tpl.Item2));
            TimelineObjects.Add(new TimelineObjectViewModel(TimelineState, new TimelineProcessorNode(processor, context), WorkingAreaViewModel.WorkingAreaState));
        });
    }
}
