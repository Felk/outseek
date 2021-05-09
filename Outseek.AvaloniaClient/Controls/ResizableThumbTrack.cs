using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Metadata;

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

        public static readonly StyledProperty<double> ScaleProperty =
            AvaloniaProperty.Register<ResizableThumbTrack, double>(nameof(Scale),
                defaultBindingMode: BindingMode.TwoWay);

        public static readonly StyledProperty<double> OffsetProperty =
            AvaloniaProperty.Register<ResizableThumbTrack, double>(nameof(Offset),
                defaultBindingMode: BindingMode.TwoWay);

        public static readonly StyledProperty<double> MinScaleProperty =
            AvaloniaProperty.Register<ResizableThumbTrack, double>(nameof(MinScale), defaultValue: 0);

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

        public double Scale
        {
            get { return GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        public double Offset
        {
            get { return GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        public double MinScale
        {
            get { return GetValue(MinScaleProperty); }
            set { SetValue(MinScaleProperty, value); }
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

            AffectsArrange<ResizableThumbTrack>(ScaleProperty, OffsetProperty, ResizeThumbPlacementProperty);
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
            double segmentWidth = arrangeSize.Width * Scale;
            double segmentOffset = (arrangeSize.Width - segmentWidth) * Offset;
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
                if (segmentSize.Width > 0)
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
            double delta = e.Vector.X / Bounds.Width;
            Offset = Math.Clamp(Offset + delta, 0, 1);
        }

        private void ResizeStartThumbDragged(object? sender, VectorEventArgs e)
        {
            double delta = e.Vector.X / Bounds.Width;
            double oldScale = Scale;
            double newScale = oldScale - delta;
            double newOffset = (Bounds.Width * (1 - oldScale) * Offset + e.Vector.X) /
                               (Bounds.Width * (1 - newScale));

            // TODO instead of aborting the whole drag, set the values to their maximum
            if (newOffset < 0 || newOffset > 1) return;
            if (newScale < MinScale || newScale > 1) return;

            Scale = newScale;
            Offset = newOffset;
        }

        private void ResizeEndThumbDragged(object? sender, VectorEventArgs e)
        {
            double delta = e.Vector.X / Bounds.Width;
            double newOffset = Offset * (1 - Scale) / (1 - Scale - delta);
            double newScale = Scale + delta;

            // TODO instead of aborting the whole drag, set the values to their maximum
            if (newOffset < 0 || newOffset > 1) return;
            if (newScale < MinScale || newScale > 1) return;

            Scale = newScale;
            Offset = newOffset;
        }
    }
}
