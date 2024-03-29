﻿using System;
using System.Reactive;
using System.Reactive.Linq;
using Outseek.API;
using Outseek.AvaloniaClient.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Outseek.AvaloniaClient.SharedViewModels;

public class TimelineProcessorsState : ViewModelBase
{
    [Reactive] public TimelineObjectViewModel SelectedTimelineObject { get; set; } = new();
    [Reactive] public TimelineProcessorParams VolatileParamsObject { get; set; } = null!;

    [Reactive] public bool IsDirty { get; set; } = true;

    public ReactiveCommand<Unit, Unit> Save { get; }
    public ReactiveCommand<Unit, Unit> Revert { get; }

    public TimelineProcessorsState()
    {
        this.WhenAnyValue(s => s.VolatileParamsObject, s => s.SelectedTimelineObject.Node.Params)
            .Where(tuple => tuple.Item1 != null && tuple.Item2 != null)
            .Subscribe(_ => UpdatePropertyObjectIsDirty());

        this.WhenAnyValue(s => s.SelectedTimelineObject)
            .Subscribe(newViewModel => VolatileParamsObject = newViewModel.Node.Params.DeepCopy());

        IObservable<bool> isDirtyObservable = this.WhenAnyValue(t => t.IsDirty);
        Save = ReactiveCommand.Create(
            () => { SelectedTimelineObject.Node.Params = VolatileParamsObject.DeepCopy(); },
            isDirtyObservable);
        Revert = ReactiveCommand.Create(
            () => { VolatileParamsObject = SelectedTimelineObject.Node.Params.DeepCopy(); },
            isDirtyObservable);
    }

    public void UpdatePropertyObjectIsDirty()
    {
        IsDirty = !VolatileParamsObject.Equals(SelectedTimelineObject.Node.Params);
    }
}
