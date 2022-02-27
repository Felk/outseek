using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Outseek.AvaloniaClient.Views.TimelineObjects;

public class SegmentsView : UserControl
{
    public SegmentsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
