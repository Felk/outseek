using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Metadata;
using Outseek.AvaloniaClient.Utils;

namespace Outseek.AvaloniaClient.Controls
{
    public enum ResizeThumbPlacement
    {
        Outside, Inside
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
        public static readonly StyledProperty<Thumb> DragThumbProperty =
            AvaloniaProperty.Register<ResizableThumbTrack, Thumb>(nameof(DragThumb));

        public static readonly StyledProperty<Thumb> ResizeStartThumbProperty =
            AvaloniaProperty.Register<ResizableThumbTrack, Thumb>(nameof(ResizeStartThumb));

        public static readonly StyledProperty<Thumb> ResizeEndThumbProperty =
            AvaloniaProperty.Register<ResizableThumbTrack, Thumb>(nameof(ResizeEndThumb));

        public static readonly StyledProperty<double> MinimumProperty =
            AvaloniaProperty.Register<ResizableThumbTrack, double>(nameof(Minimum),
                defaultBindingMode: BindingMode.TwoWay);
        
        public static readonly StyledProperty<double> MaximumProperty =
            AvaloniaProperty.Register<ResizableThumbTrack, double>(nameof(Maximum),
                defaultBindingMode: BindingMode.TwoWay);

        public static readonly StyledProperty<double> FromProperty =
            AvaloniaProperty.Register<ResizableThumbTrack, double>(nameof(From),
                defaultBindingMode: BindingMode.TwoWay);
        
        public static readonly StyledProperty<double> ToProperty =
            AvaloniaProperty.Register<ResizableThumbTrack, double>(nameof(To),
                defaultBindingMode: BindingMode.TwoWay);

        public static readonly StyledProperty<double> IncrementProperty =
            AvaloniaProperty.Register<ResizableThumbTrack, double>(nameof(Increment), defaultValue: 0);

        public static readonly StyledProperty<double> MinimumDistanceProperty =
            AvaloniaProperty.Register<ResizableThumbTrack, double>(nameof(MinimumDistance), defaultValue: 0);

        public static readonly StyledProperty<ResizeThumbPlacement> ResizeThumbPlacementProperty =
            AvaloniaProperty.Register<ResizableThumbTrack, ResizeThumbPlacement>(nameof(ResizeThumbPlacement),
                defaultValue: ResizeThumbPlacement.Inside);

        [Content]
        public Thumb DragThumb
        {
            get { return GetValue(DragThumbProperty); }
            set { SetValue(DragThumbProperty, value); }
        }

        public Thumb ResizeStartThumb
        {
            get { return GetValue(ResizeStartThumbProperty); }
            set { SetValue(ResizeStartThumbProperty, value); }
        }

        public Thumb ResizeEndThumb
        {
            get { return GetValue(ResizeEndThumbProperty); }
            set { SetValue(ResizeEndThumbProperty, value); }
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

        public double From
        {
            get { return GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }
        
        public double To
        {
            get { return GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
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

        static ResizableThumbTrack()
        {
            ResizeStartThumbProperty.Changed.AddClassHandler<ResizableThumbTrack>(
                (x, e) => x.ResizeStartThumbChanged(e));
            ResizeEndThumbProperty.Changed.AddClassHandler<ResizableThumbTrack>((x, e) => x.ResizeEndThumbChanged(e));
            DragThumbProperty.Changed.AddClassHandler<ResizableThumbTrack>((x, e) => x.DragThumbChanged(e));

            AffectsArrange<ResizableThumbTrack>(
                FromProperty, ToProperty, MinimumProperty, MaximumProperty, ResizeThumbPlacementProperty);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            // TODO make the components' usages null-safe
            DragThumb.Measure(availableSize);
            ResizeStartThumb.Measure(availableSize);
            ResizeEndThumb.Measure(availableSize);
            return availableSize;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            double availableSize = Maximum - Minimum;
            double targetSize = To - From;

            double segmentWidth = arrangeSize.Width * (targetSize / availableSize);
            double segmentOffset = arrangeSize.Width * (From / availableSize);
            if (segmentOffset is double.NaN) segmentOffset = 0;

            Point segmentStart = new Point(segmentOffset, 0);
            Size segmentSize = new Size(segmentWidth, arrangeSize.Height);

            // TODO this doesn't really belong here?
            ResizeStartThumb.Cursor = new Cursor(StandardCursorType.LeftSide);
            ResizeEndThumb.Cursor = new Cursor(StandardCursorType.RightSide);

            ResizeStartThumb.Arrange(new Rect(
                segmentStart,
                segmentSize.WithWidth(ResizeStartThumb.DesiredSize.Width)));
            if (ResizeThumbPlacement == ResizeThumbPlacement.Outside)
            {
                Size dragThumbWidth = segmentSize
                    .WithWidth(segmentSize.Width - ResizeStartThumb.DesiredSize.Width - ResizeEndThumb.DesiredSize.Width);
                if (dragThumbWidth.Width > 0)
                    DragThumb.Arrange(new Rect(
                        segmentStart.WithX(segmentOffset + ResizeStartThumb.DesiredSize.Width),
                        dragThumbWidth));
            }
            else
            {
                ResizeStartThumb.ZIndex = 1;
                ResizeEndThumb.ZIndex = 1;
                if (segmentSize.Width >= 0)
                    DragThumb.Arrange(new Rect(segmentStart, segmentSize));
            }
            ResizeEndThumb.Arrange(new Rect(
                segmentStart.WithX(segmentOffset + segmentWidth - ResizeEndThumb.DesiredSize.Width),
                segmentSize.WithWidth(ResizeEndThumb.DesiredSize.Width)));

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

        private void DragThumbDragged(object? sender, VectorEventArgs e)
        {
            double deltaPercent = e.Vector.X / Bounds.Width;
            double range = Maximum - Minimum;
            double delta = Math.Clamp(deltaPercent * range, Minimum - From, Maximum - To);
            
            From = MathUtils.RoundToIncrement(From + delta, Increment);
            To = MathUtils.RoundToIncrement(To + delta, Increment);
        }

        private void ResizeStartThumbDragged(object? sender, VectorEventArgs e)
        {
            double deltaPercent = e.Vector.X / Bounds.Width;
            double range = Maximum - Minimum;
            double delta = Math.Clamp(deltaPercent * range, Minimum - From, To - From - MinimumDistance);
            
            From = MathUtils.RoundToIncrement(From + delta, Increment);
        }

        private void ResizeEndThumbDragged(object? sender, VectorEventArgs e)
        {
            double deltaPercent = e.Vector.X / Bounds.Width;
            double range = Maximum - Minimum;
            double delta = Math.Clamp(deltaPercent * range, From - To + MinimumDistance, Maximum - To);
            
            To = MathUtils.RoundToIncrement(To + delta, Increment);
        }
    }
}
