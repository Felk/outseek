using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Outseek.AvaloniaClient.SharedViewModels;
using Outseek.AvaloniaClient.ViewModels;

namespace Outseek.AvaloniaClient.Views
{
    public class TimelineView : UserControl
    {
        public TimelineView()
        {
            InitializeComponent();
            DataContext = new TimelineViewModel(TimelineState.Instance);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

