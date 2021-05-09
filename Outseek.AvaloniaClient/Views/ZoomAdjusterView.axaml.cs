using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Outseek.AvaloniaClient.SharedViewModels;
using Outseek.AvaloniaClient.ViewModels;

namespace Outseek.AvaloniaClient.Views
{
    public class ZoomAdjusterView : UserControl
    {
        public ZoomAdjusterView()
        {
            InitializeComponent();
            DataContext = new ZoomAdjusterViewModel(TimelineState.Instance);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
