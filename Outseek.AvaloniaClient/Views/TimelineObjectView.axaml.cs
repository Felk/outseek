using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Outseek.AvaloniaClient.Views;

public class TimelineObjectView : UserControl
{
    public TimelineObjectView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
