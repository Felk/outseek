using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Outseek.AvaloniaClient.ViewModels;

namespace Outseek.AvaloniaClient.Views;

public partial class TimelineView : UserControl
{
    private readonly ContentControl _zoomAdjuster;

    public TimelineView()
    {
        InitializeComponent();
        _zoomAdjuster = this.FindControl<ContentControl>("ZoomAdjuster")!;
    }

    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        ((TimelineViewModel) DataContext!).TimelineState.ViewportHoverPosition = e.GetPosition(_zoomAdjuster).X;
    }
}
