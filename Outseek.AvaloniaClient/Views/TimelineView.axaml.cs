using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Outseek.AvaloniaClient.ViewModels;
using Outseek.Backend.Processors;
using Dispatcher = Avalonia.Threading.Dispatcher;

namespace Outseek.AvaloniaClient.Views
{
    public class TimelineView : UserControl
    {
        private readonly ContentControl _zoomAdjuster;

        public TimelineView()
        {
            InitializeComponent();
            _zoomAdjuster = this.FindControl<ContentControl>("ZoomAdjuster");
        }

        private void OnInitialized(object? sender, EventArgs e)
        {
            // TODO find a place to do this instead of the code behind, probably with a behaviour: https://github.com/wieslawsoltes/AvaloniaBehaviors
            Task _ = Task.Run(() =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    TimelineViewModel dc = (TimelineViewModel) DataContext!;
            
                    dc.TimelineObjects.Add(new TimelineObjectViewModel(dc.TimelineState, new RandomSegments()));
                    dc.TimelineObjects.Add(new TimelineObjectViewModel(dc.TimelineState, new RandomSegments()));
                    dc.TimelineObjects.Add(new TimelineObjectViewModel(dc.TimelineState, new GetRandomChat()));
                });
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
        {
            ((TimelineViewModel) DataContext!).TimelineState.ViewportHoverPosition = e.GetPosition(_zoomAdjuster).X;
        }
    }
}
