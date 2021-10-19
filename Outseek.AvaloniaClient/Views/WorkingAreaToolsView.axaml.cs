using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Outseek.AvaloniaClient.Views
{
    public class WorkingAreaToolsView : UserControl
    {
        public WorkingAreaToolsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

