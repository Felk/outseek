using System;
using System.Reactive;
using System.Runtime.CompilerServices;
using Outseek.AvaloniaClient.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Outseek.AvaloniaClient.SharedViewModels
{
    public class TimelineProcessorsState : ViewModelBase
    {
        [Reactive] public TimelineObjectViewModel SelectedTimelineObject { get; set; } = new();
        [Reactive] public ValueType VolatileParamsObject { get; set; } = null!;

        [Reactive] public bool IsDirty { get; set; } = true;

        public ReactiveCommand<Unit, Unit> Save { get; }
        public ReactiveCommand<Unit, Unit> Revert { get; }

        private static ValueType Copy(ValueType valueType)
        {
            // Just assigning the ValueType would make a reference, since the struct is boxed.
            // This unboxes the struct, makes a copy, and boxes it again.
            return (ValueType) RuntimeHelpers.GetObjectValue(valueType);
        }

        public TimelineProcessorsState()
        {
            this.WhenAnyValue(s => s.VolatileParamsObject, s => s.SelectedTimelineObject.ParamsObject)
                .Subscribe(_ => UpdatePropertyObjectIsDirty());

            this.WhenAnyValue(s => s.SelectedTimelineObject)
                .Subscribe(newViewModel => VolatileParamsObject = Copy(newViewModel.ParamsObject));

            IObservable<bool> isDirtyObservable = this.WhenAnyValue(t => t.IsDirty);
            Save = ReactiveCommand.Create(
                () => { SelectedTimelineObject.ParamsObject = Copy(VolatileParamsObject); }, isDirtyObservable);
            Revert = ReactiveCommand.Create(
                () => { VolatileParamsObject = Copy(SelectedTimelineObject.ParamsObject); }, isDirtyObservable);
        }

        public void UpdatePropertyObjectIsDirty()
        {
            IsDirty = !Equals(VolatileParamsObject, SelectedTimelineObject.ParamsObject);
        }
    }
}
