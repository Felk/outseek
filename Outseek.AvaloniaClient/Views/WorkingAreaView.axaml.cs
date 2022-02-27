using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Outseek.AvaloniaClient.Views;

public class WorkingAreaView : UserControl
{
    public WorkingAreaView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
