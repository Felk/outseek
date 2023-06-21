using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Outseek.API;

namespace Outseek.AvaloniaClient.Views;

public partial class TimelineProcessorExplorerView : UserControl
{
    public TimelineProcessorExplorerView()
    {
        InitializeComponent();
    }

    private void OnTimelineProcessorItemPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var draggedElement = sender as StyledElement;
        if (draggedElement?.DataContext is ITimelineProcessor draggedProcessor)
        {
            var data = new DataObject();
            data.Set("processor", draggedProcessor);
            DragDrop.DoDragDrop(e, data, DragDropEffects.Copy);
        }
        else
        {
            Console.Error.WriteLine(
                "Initiated processor drag from somewhere that isn't element with a timeline processor as data context! Sender: " + sender);
        }
    }
}
