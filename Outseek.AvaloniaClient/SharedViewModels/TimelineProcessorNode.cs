using System;
using System.Collections.ObjectModel;
using Outseek.API;
using Outseek.AvaloniaClient.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Outseek.AvaloniaClient.SharedViewModels
{
    public class TimelineProcessorNode : ViewModelBase
    {
        [Reactive] public TimelineObject Input { get; set; } = new TimelineObject.Nothing();
        [Reactive] public TimelineObject Output { get; set; } = new TimelineObject.Nothing();
        [Reactive] public TimelineProcessorParams Params { get; set; }

        [Reactive] private ITimelineProcessContext Context { get; set; } = new TimelineProcessContext(0, 100);

        public readonly ITimelineProcessor Processor;
        public ObservableCollection<TimelineProcessorNode> Children { get; } = new();

        public TimelineProcessorNode(ITimelineProcessor processor, IObservable<ITimelineProcessContext> context)
        {
            Processor = processor;
            Params = Processor.GetDefaultParams();
            context.Subscribe(c => Context = c);
            this.WhenAnyValue(node => node.Input, node => node.Params, node => node.Context)
                .Subscribe(_ => Refresh());
            this.WhenAnyValue(node => node.Output)
                .Subscribe(o =>
                {
                    foreach (TimelineProcessorNode child in Children)
                        child.Input = o;
                });
            Children.CollectionChanged += (_, _) =>
            {
                foreach (TimelineProcessorNode child in Children)
                    child.Input = Output;
            };
        }

        private void Refresh()
        {
            try
            {
                Output = RerunProcessor();
            }
            catch (Exception ex)
            {
                Output = new TimelineObject.Error(ex, ex.Message);
            }
        }

        private TimelineObject RerunProcessor()
        {
            if (Input.GetType() != Processor.InputType)
            {
                throw new InvalidCastException(
                    $"Input type mismatch! Expected: {Processor.InputType.Name}, but got: {Input.GetType().Name}. " +
                    "This processor is invalid for its parent's output type.");
            }

            TimelineObject timelineObject = Processor.Process(Context, Input, Params);
            if (timelineObject.GetType() != Processor.OutputType)
            {
                throw new InvalidCastException(
                    $"Output type mismatch! Expected: {Processor.OutputType.Name}, but got: {timelineObject.GetType().Name}. " +
                    "This processor misbehaved by not producing the advertised data type.");
            }

            return timelineObject;
        }
    }
}
