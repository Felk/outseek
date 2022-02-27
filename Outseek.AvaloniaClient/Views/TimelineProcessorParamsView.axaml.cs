using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Outseek.AvaloniaClient.ViewModels;

namespace Outseek.AvaloniaClient.Views;

public class TimelineProcessorParamsView : UserControl
{
    public TimelineProcessorParamsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void PropertyGrid_OnPropertyObjectChanged(object? sender, EventArgs e)
    {
        // TODO avoid wiring this up in code behind
        TimelineProcessorParamsViewModel vm = (TimelineProcessorParamsViewModel) DataContext!;
        vm.TimelineProcessorsState.UpdatePropertyObjectIsDirty();
    }
}
