using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using ReactiveUI;
using static AvaloniaOutseekClient.Utils.MathUtils;

namespace AvaloniaOutseekClient.Controls
{
    /// <summary>
    /// A content control that horizontally positions its content according to a "From" and "To" property,
    /// allowing for the start and endpoint to be adjusted if IsEditable is true.
    /// </summary>
    public class HorizontalSegment : ContentControl
    {
        public static readonly StyledProperty<double> FromProperty =
            AvaloniaProperty.Register<HorizontalSegment, double>(nameof(From), defaultBindingMode: BindingMode.TwoWay);

        public static readonly StyledProperty<double> ToProperty =
            AvaloniaProperty.Register<HorizontalSegment, double>(nameof(To), defaultBindingMode: BindingMode.TwoWay);

        public static readonly StyledProperty<double> StepProperty =
            AvaloniaProperty.Register<HorizontalSegment, double>(nameof(Step), defaultValue: 1);

        public static readonly StyledProperty<bool> IsEditableProperty =
            AvaloniaProperty.Register<HorizontalSegment, bool>(nameof(IsEditable), defaultValue: true);

        public double From
        {
            get => GetValue(FromProperty);
            set => SetValue(FromProperty, value);
        }

        public double To
        {
            get => GetValue(ToProperty);
            set => SetValue(ToProperty, value);
        }

        public double Step
        {
            get => GetValue(StepProperty);
            set => SetValue(StepProperty, value);
        }

        public bool IsEditable
        {
            get => GetValue(IsEditableProperty);
            set => SetValue(IsEditableProperty, value);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            Grid containerGrid = (Grid) e.NameScope.Find("PART_ContainerGrid");
            GridSplitter leftSplitter = (GridSplitter) e.NameScope.Find("PART_LeftSplitter");
            GridSplitter rightSplitter = (GridSplitter) e.NameScope.Find("PART_RightSplitter");
            ColumnDefinition colDefFrom = containerGrid.ColumnDefinitions[0];
            ColumnDefinition colDefDuration = containerGrid.ColumnDefinitions[2];
            IObservable<GridLength> from = colDefFrom.GetObservable(ColumnDefinition.WidthProperty);
            IObservable<GridLength> duration = colDefDuration.GetObservable(ColumnDefinition.WidthProperty);
            colDefFrom.Width = new GridLength(From);
            colDefDuration.Width = new GridLength(To - From);

            this.GetObservable(IsEditableProperty).BindTo(leftSplitter, s => s.IsEnabled);
            this.GetObservable(IsEditableProperty).BindTo(rightSplitter, s => s.IsEnabled);

            // Only write values back to property once the drag is complete.
            // This has advantages, e.g. the drag can be aborted by pressing escape,
            // or potential viewmodel writes are rare enough to be usable for redo/undo functionality.
            void LeftSplitterDragCompleted(object? sender, VectorEventArgs ev) =>
                From = colDefFrom.Width.Value;
            void RightSplitterDragCompleted(object? sender, VectorEventArgs ev) =>
                To = colDefFrom.Width.Value + colDefDuration.Width.Value;
            leftSplitter.DragCompleted += LeftSplitterDragCompleted;
            rightSplitter.DragCompleted += RightSplitterDragCompleted;

            from.Subscribe(val =>
            {
                // enforce step and fix duration, so extending to the left doesn't cause the segment to just be moved.
                double stepSnappedVal = RoundToIncrement(val.Value, Step);
                colDefFrom.Width = new GridLength(stepSnappedVal);
                colDefDuration.Width = new GridLength(To - stepSnappedVal);
            });
            duration.Subscribe(val =>
            {
                // enforce step
                colDefDuration.Width = new GridLength(RoundToIncrement(val.Value, Step));
            });

            // push property changes back into the view
            this.GetObservable(FromProperty).Subscribe(fromValue =>
            {
                colDefFrom.Width = new GridLength(fromValue);
                colDefDuration.Width = new GridLength(To - fromValue);
            });
            this.GetObservable(ToProperty).Subscribe(toValue =>
            {
                colDefDuration.Width = new GridLength(toValue - From);
            });
        }
    }
}
