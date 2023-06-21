using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using Outseek.AvaloniaClient.SharedViewModels;
using Outseek.AvaloniaClient.Utils;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Outseek.AvaloniaClient.ViewModels;

public class WorkingAreaToolsViewModel : ViewModelBase
{
    public WorkingAreaViewModel WorkingAreaViewModel { get; }
    public MediaState MediaState { get; }
    [Reactive] public OpenTimelineIO? Otio { get; set; }

    public ReactiveCommand<Unit, Unit> Export { get; }

    public WorkingAreaToolsViewModel(WorkingAreaViewModel workingAreaViewModel, MediaState mediaState)
    {
        WorkingAreaViewModel = workingAreaViewModel;
        MediaState = mediaState;

        IObservable<bool> otioAvailable = this.WhenAnyValue(vm => vm.Otio).Select(otio => otio != null);
        Export = ReactiveCommand.Create(() =>
        {
            Window mainWindow = ((IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!).MainWindow!;
            mainWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
            {
                DefaultExtension = ".otio",
                SuggestedFileName = "my_highlights.otio",
                Title = "Choose file to save to",
            }).ContinueWith(async task =>
            {
                if (task.Exception != null)
                {
                    var messagebox = MessageBoxManager.GetMessageBoxStandardWindow(
                        "Could not select file", task.Exception.ToString(), ButtonEnum.Ok, Icon.Error);
                    await messagebox.Show();
                }
                else
                {
                    IStorageFile? selectedFile = task.Result;
                    if (selectedFile != null && mediaState.Filename != null)
                        Otio?.SaveSegments(
                            mediaState.Filename,
                            selectedFile.Path.AbsolutePath,
                            workingAreaViewModel.WorkingAreaState.Segments.Select(r => r.Range));
                }
            });
        }, canExecute: otioAvailable);
    }

    public WorkingAreaToolsViewModel() : this(new WorkingAreaViewModel(), new MediaState())
    {
        // the default constructor is only used by the designer
    }
}
