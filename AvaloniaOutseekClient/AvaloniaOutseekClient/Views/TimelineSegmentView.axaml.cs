using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaOutseekClient.Views
{
    public class TimelineSegmentView : UserControl
    {
        public TimelineSegmentView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
