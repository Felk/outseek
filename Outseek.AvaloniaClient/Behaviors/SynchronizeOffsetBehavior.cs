using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;

namespace Outseek.AvaloniaClient.Behaviors
{
    public class SynchronizeOffsetBehavior : Behavior<ScrollViewer>
    {
        public static readonly StyledProperty<ScrollViewer?> SourceProperty =
            AvaloniaProperty.Register<SynchronizeOffsetBehavior, ScrollViewer?>(nameof(Source));

        public ScrollViewer? Source
        {
            get => GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        private void AssociatedScrollViewer_ScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            if (Source != null && AssociatedObject != null)
                Source.Offset = AssociatedObject.Offset;
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
                AssociatedObject.ScrollChanged += AssociatedScrollViewer_ScrollChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
                AssociatedObject.ScrollChanged -= AssociatedScrollViewer_ScrollChanged;
        }
    }
}
