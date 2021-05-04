using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaOutseekClient.SharedViewModels;
using AvaloniaOutseekClient.ViewModels;

namespace AvaloniaOutseekClient.Views
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

