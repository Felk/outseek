using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Outseek.AvaloniaClient.SharedViewModels;

namespace Outseek.AvaloniaClient.Views
{
    public class MainWindow : Window
    {
        private readonly MediaState _mediaState;
        
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            _mediaState = MediaState.Instance;

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
            _mediaState.Filename = filename;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
