using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using Avalonia.Input;
using DynamicData;
using Outseek.API;
using Outseek.AvaloniaClient.SharedViewModels;
using Outseek.AvaloniaClient.Utils;
using Outseek.AvaloniaClient.ViewModels.TimelineObjects;
using Outseek.Backend.Processors;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Outseek.AvaloniaClient.ViewModels
{
    public sealed class TimelineObjectViewModel : ViewModelBase, IDisposable
    {
        public readonly TimelineProcessorNode Node;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public TimelineState TimelineState { get; }

        [Reactive] public string? Text { get; set; }
        [Reactive] public TimelineObjectViewModelBase? TimelineObject { get; set; }

        public bool IsExpanded {
            set
            {
                foreach (TimelineObjectViewModel child in Children) child.IsVisible = value;
            }
        }
        [Reactive] public bool IsVisible { get; set; } = true;

        public ObservableCollection<TimelineObjectViewModel> Children { get; }

        public ReactiveCommand<Unit, Unit> CopySegments { get; }
        public ReactiveCommand<DragEventArgs, Unit> Drop { get; }
        public static Func<DragEventArgs, bool> CheckDropAllowed => e => e.Data.Contains("processor");

        public TimelineObjectViewModel() : this(
            new TimelineState(), new TimelineProcessorNode(new RandomSegments(), Observable.Empty<ITimelineProcessContext>()), new WorkingAreaState())
        {
            // the default constructor is only used by the designer
        }

        public TimelineObjectViewModel(
            TimelineState timelineState,
            TimelineProcessorNode node,
            WorkingAreaState workingAreaState)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            TimelineState = timelineState;
            Node = node;
            Text = Node.Processor.Name;

            Node.WhenAnyValue(n => n.Output)
                .Subscribe(async timelineObject =>
                {
                    TimelineObjectViewModelBase timelineObjectViewModelBase = timelineObject switch
                    {
                        TimelineObject.Nothing nothing => new NothingViewModel(),
                        TimelineObject.Error error => new ErrorViewModel { ErrorText = error.ErrorText },
                        TimelineObject.Segments segments => new SegmentsViewModel(TimelineState, segments),
                        TimelineObject.TimedText timedText => new TimedTextViewModel(TimelineState, timedText),
                        _ => throw new Exception($"unknown timeline object '{timelineObject}'")
                    };
                    TimelineObject = timelineObjectViewModelBase;
                    try
                    {
                        await timelineObjectViewModelBase.Refresh(_cancellationTokenSource.Token);
                    }
                    catch (Exception ex)
                    {
                        TimelineObject = new ErrorViewModel { ErrorText = ex.Message };
                    }
                });

            TimelineObjectViewModel Wrap(TimelineProcessorNode n) => new(timelineState, n, workingAreaState);
            TimelineProcessorNode Unwrap(TimelineObjectViewModel vm) => vm.Node;
            Children = new WrappedObservableCollection<TimelineObjectViewModel, TimelineProcessorNode>(node.Children, Wrap, Unwrap);

            ObservableRange Clone(ObservableRange r) => new(timelineState, r.Range);
            CopySegments = ReactiveCommand.Create(
                () => workingAreaState.Segments.AddRange(((SegmentsViewModel)TimelineObject!).SelectedSegments.Select(Clone)),
                this.WhenAnyValue(vm => vm.TimelineObject).Select(to => to is SegmentsViewModel));

            Drop = ReactiveCommand.Create((DragEventArgs dragEventArgs) =>
            {
                var processor = (ITimelineProcessor) dragEventArgs.Data.Get("processor")!;

                IObservable<TimelineProcessContext> context = TimelineState
                    .WhenAnyValue(s => s.Start, s => s.End)
                    .Select(tpl => new TimelineProcessContext(tpl.Item1, tpl.Item2));
                Node.Children.Add(new TimelineProcessorNode(processor, context));

                // Workaround: The tree view items' IsExpanded property's binding is one-way to source,
                // and that set-only property sets the children's IsVisible property which is used to have
                // the timeline items' visibility be in sync with the tree view's expanded state.
                // This workaround ensures that if this node is collapsed the new child gets IsVisible set to false as well.
                // Checking if any existing children are hidden is sufficient, because the trees are expanded by default,
                // so if we just added the first child it should just stay visible anyway.
                if (Children.Any(c => !c.IsVisible)) IsExpanded = false;
            });
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}
