using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Outseek.AvaloniaClient.ViewModels;

namespace Outseek.AvaloniaClient.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = new MainWindowViewModel();
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            AddHandler(DragDrop.DropEvent, Drop);
            AddHandler(DragDrop.DragOverEvent, DragOver);
        }

        private static void DragOver(object? sender, DragEventArgs e)
        {
            if (e.Data.GetFileNames() == null ||
                e.Data.GetFileNames()!.Count() > 1)
            {
                e.DragEffects = DragDropEffects.None;
            }
        }

        private void Drop(object? sender, DragEventArgs e)
        {
            string? filename = e.Data.GetFileNames()?.FirstOrDefault();
            if (filename == null) return;
            ((MainWindowViewModel) DataContext!).MediaState.Filename = filename;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
