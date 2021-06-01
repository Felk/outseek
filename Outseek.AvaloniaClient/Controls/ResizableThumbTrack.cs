using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Metadata;
using Outseek.AvaloniaClient.Utils;
using Range = Outseek.AvaloniaClient.Utils.Range;

namespace Outseek.AvaloniaClient.Controls
{
    public enum ResizeThumbPlacement
    {
        Outside,
        Inside
    }

    /// <summary>
    /// A <see cref="Track"/> with a resizable thumb.
    /// In a normal track the size of the thumb gets calculated from minimum, maximum and viewport size.
    /// This control allows the thumb size to be directly manipulated,
    /// reflecting the changes back into the viewport size property. 
    /// </summary>
    [PseudoClasses(":vertical", ":horizontal")] // TODO currently only horizontal works
    public class ResizableThumbTrack : Control
    {
        public static readonly StyledProperty<Thumb?> DragThumbProperty =
            AvaloniaProperty.Register<ResizableThumbTrack, Thumb?>(nameof(DragThumb));

        public static readonly StyledProperty<Thumb?> ResizeStartThumbProperty =
            AvaloniaProperty.Register<ResizableThumbTrack, Thumb?>(nameof(ResizeStartThumb));

        public static readonly StyledProperty<Thumb?> ResizeEndThumbProperty =
            AvaloniaProperty.Register<ResizableThumbTrack, Thumb?>(nameof(ResizeEndThumb));

        public static readonly StyledProperty<Control?> BackdropProperty =
            AvaloniaProperty.Register<ResizableThumbTrack, Control?>(nameof(Backdrop));

        public static readonly StyledProperty<double> MinimumProperty =
            AvaloniaProperty.Register<ResizableThumbTrack, double>(nameof(Minimum));

        public static readonly StyledProperty<double> MaximumProperty =
            AvaloniaProperty.Register<ResizableThumbTrack, double>(nameof(Maximum));

        public static readonly StyledProperty<Range> RangeProperty =
            AvaloniaProperty.Register<ResizableThumbTrack, Range>(nameof(Range),
                defaultBindingMode: BindingMode.TwoWay);

        public static readonly StyledProperty<double> IncrementProperty =
            AvaloniaProperty.Register<ResizableThumbTrack, double>(nameof(Increment), defaultValue: 0);

        public static readonly StyledProperty<double> MinimumDistanceProperty =
            AvaloniaProperty.Register<ResizableThumbTrack, double>(nameof(MinimumDistance), defaultValue: 0);

        public static readonly StyledProperty<ResizeThumbPlacement> ResizeThumbPlacementProperty =
            AvaloniaProperty.Register<ResizableThumbTrack, ResizeThumbPlacement>(nameof(ResizeThumbPlacement),
                defaultValue: ResizeThumbPlacement.Inside);

        public static readonly StyledProperty<bool> IsResizableProperty =
            AvaloniaProperty.Register<ResizableThumbTrack, bool>(nameof(IsResizable), defaultValue: true);

        public static readonly StyledProperty<bool> IsDraggableProperty =
            AvaloniaProperty.Register<ResizableThumbTrack, bool>(nameof(IsDraggable), defaultValue: true);

        [Content]
        public Thumb? DragThumb
        {
            get { return GetValue(DragThumbProperty); }
            set { SetValue(DragThumbProperty, value); }
        }

        public Thumb? ResizeStartThumb
        {
            get { return GetValue(ResizeStartThumbProperty); }
            set { SetValue(ResizeStartThumbProperty, value); }
        }

        public Thumb? ResizeEndThumb
        {
            get { return GetValue(ResizeEndThumbProperty); }
            set { SetValue(ResizeEndThumbProperty, value); }
        }

        public Control? Backdrop
        {
            get { return GetValue(BackdropProperty); }
            set { SetValue(BackdropProperty, value); }
        }

        public double Minimum
        {
            get { return GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public double Maximum
        {
            get { return GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public Range Range
        {
            get { return GetValue(RangeProperty); }
            set { SetValue(RangeProperty, value); }
        }

        public double Increment
        {
            get { return GetValue(IncrementProperty); }
            set { SetValue(IncrementProperty, value); }
        }

        public double MinimumDistance
        {
            get { return GetValue(MinimumDistanceProperty); }
            set { SetValue(MinimumDistanceProperty, value); }
        }

        public ResizeThumbPlacement ResizeThumbPlacement
        {
            get { return GetValue(ResizeThumbPlacementProperty); }
            set { SetValue(ResizeThumbPlacementProperty, value); }
        }

        public bool IsResizable
        {
            get { return GetValue(IsResizableProperty); }
            set { SetValue(IsResizableProperty, value); }
        }

        public bool IsDraggable
        {
            get { return GetValue(IsDraggableProperty); }
            set { SetValue(IsDraggableProperty, value); }
        }

        static ResizableThumbTrack()
        {
            ResizeStartThumbProperty.Changed.AddClassHandler<ResizableThumbTrack>(
                (x, e) => x.ResizeStartThumbChanged(e));
            ResizeEndThumbProperty.Changed.AddClassHandler<ResizableThumbTrack>((x, e) => x.ResizeEndThumbChanged(e));
            DragThumbProperty.Changed.AddClassHandler<ResizableThumbTrack>((x, e) => x.DragThumbChanged(e));
            BackdropProperty.Changed.AddClassHandler<ResizableThumbTrack>((x, e) => x.BackdropChanged(e));

            AffectsArrange<ResizableThumbTrack>(
                RangeProperty, MinimumProperty, MaximumProperty, ResizeThumbPlacementProperty);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Backdrop?.Measure(availableSize);
            ResizeStartThumb?.Measure(availableSize);
            ResizeEndThumb?.Measure(availableSize);
            if (DragThumb != null)
            {
                DragThumb.Measure(availableSize);
                return DragThumb.DesiredSize;
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            double availableSize = Maximum - Minimum;
            if (availableSize <= 0) return new Size();
            double targetSize = Range.Size;

            double segmentWidth = arrangeSize.Width * (targetSize / availableSize);
            double segmentOffset = arrangeSize.Width * (Range.From / availableSize);
            if (segmentOffset is double.NaN) segmentOffset = 0;

            Point segmentStart = new Point(segmentOffset, 0);
            Size segmentSize = new Size(segmentWidth, arrangeSize.Height);

            // TODO this doesn't really belong here?
            if (ResizeStartThumb != null)
            {
                ResizeStartThumb.Cursor = new Cursor(StandardCursorType.LeftSide);
                ResizeStartThumb.IsEnabled = IsResizable;
            }

            if (ResizeEndThumb != null)
            {
                ResizeEndThumb.Cursor = new Cursor(StandardCursorType.RightSide);
                ResizeEndThumb.IsEnabled = IsResizable;
            }

            if (DragThumb != null)
            {
                DragThumb.IsEnabled = IsDraggable;
            }

            ResizeStartThumb?.Arrange(new Rect(
                segmentStart,
                segmentSize.WithWidth(ResizeStartThumb.DesiredSize.Width)));
            if (ResizeThumbPlacement == ResizeThumbPlacement.Outside)
            {
                double startThumbWidth = ResizeStartThumb?.DesiredSize.Width ?? 0;
                double endThumbWidth = ResizeEndThumb?.DesiredSize.Width ?? 0;
                Size dragThumbWidth = segmentSize
                    .WithWidth(
                        segmentSize.Width - startThumbWidth - endThumbWidth);
                if (dragThumbWidth.Width > 0)
                    DragThumb?.Arrange(new Rect(
                        segmentStart.WithX(segmentOffset + startThumbWidth),
                        dragThumbWidth));
            }
            else
            {
                if (ResizeStartThumb != null)
                    ResizeStartThumb.ZIndex = 1;
                if (ResizeEndThumb != null)
                    ResizeEndThumb.ZIndex = 1;
                if (segmentSize.Width >= 0)
                    DragThumb?.Arrange(new Rect(segmentStart, segmentSize));
            }

            ResizeEndThumb?.Arrange(new Rect(
                segmentStart.WithX(segmentOffset + segmentWidth - ResizeEndThumb.DesiredSize.Width),
                segmentSize.WithWidth(ResizeEndThumb.DesiredSize.Width)));

            Backdrop?.Arrange(new Rect(segmentStart, segmentSize));

            return arrangeSize;
        }

        private void ResizeStartThumbChanged(AvaloniaPropertyChangedEventArgs e) =>
            ThumbChanged(e, ResizeStartThumbDragged);

        private void ResizeEndThumbChanged(AvaloniaPropertyChangedEventArgs e) =>
            ThumbChanged(e, ResizeEndThumbDragged);

        private void DragThumbChanged(AvaloniaPropertyChangedEventArgs e) => ThumbChanged(e, DragThumbDragged);

        private void ThumbChanged(AvaloniaPropertyChangedEventArgs e, EventHandler<VectorEventArgs> eventHandler)
        {
            Thumb? oldThumb = (Thumb?) e.OldValue;
            Thumb? newThumb = (Thumb?) e.NewValue;

            if (oldThumb != null)
            {
                oldThumb.DragDelta -= eventHandler;

                LogicalChildren.Remove(oldThumb);
                VisualChildren.Remove(oldThumb);
            }

            if (newThumb != null)
            {
                newThumb.DragDelta += eventHandler;
                LogicalChildren.Add(newThumb);
                VisualChildren.Add(newThumb);
            }
        }

        private void BackdropChanged(AvaloniaPropertyChangedEventArgs e)
        {
            Control? oldControl = (Control?) e.OldValue;
            Control? newControl = (Control?) e.NewValue;

            if (oldControl != null)
            {
                LogicalChildren.Remove(oldControl);
                VisualChildren.Remove(oldControl);
            }

            if (newControl != null)
            {
                LogicalChildren.Add(newControl);
                VisualChildren.Add(newControl);
            }
        }

        private void DragThumbDragged(object? sender, VectorEventArgs e)
        {
            double deltaPercent = e.Vector.X / Bounds.Width;
            double range = Maximum - Minimum;
            // double delta = Math.Clamp(deltaPercent * range, Minimum - From, Maximum - To);
            double delta = deltaPercent * range;
            if (delta < Minimum - Range.From) delta = Minimum - Range.From;
            if (delta > Maximum - Range.To) delta = Maximum - Range.To;

            Range = new Range(
                MathUtils.RoundToIncrement(Range.From + delta, Increment),
                MathUtils.RoundToIncrement(Range.To + delta, Increment));
        }

        private void ResizeStartThumbDragged(object? sender, VectorEventArgs e)
        {
            double deltaPercent = e.Vector.X / Bounds.Width;
            double range = Maximum - Minimum;
            double delta = Math.Clamp(deltaPercent * range, Minimum - Range.From, Range.Size - MinimumDistance);

            Range = new Range(MathUtils.RoundToIncrement(Range.From + delta, Increment), Range.To);
        }

        private void ResizeEndThumbDragged(object? sender, VectorEventArgs e)
        {
            double deltaPercent = e.Vector.X / Bounds.Width;
            double range = Maximum - Minimum;
            double delta = Math.Clamp(deltaPercent * range, -Range.Size + MinimumDistance,
                Maximum - Range.To);

            Range = new Range(Range.From, MathUtils.RoundToIncrement(Range.To + delta, Increment));
        }
    }
}
