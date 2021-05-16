using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Outseek.AvaloniaClient.ViewModels;
using Outseek.Backend.Processors;

namespace Outseek.AvaloniaClient.Views
{
    public class TimelineView : UserControl
    {
        public TimelineView()
        {
            InitializeComponent();
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
    }
}
