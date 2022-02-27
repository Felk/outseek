using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;

namespace Outseek.AvaloniaClient.Behaviors;

public class MouseWheelBehavior : Behavior<Control>
{
    public static readonly StyledProperty<ICommand> CommandProperty =
        AvaloniaProperty.Register<MouseWheelBehavior, ICommand>(nameof(Command));

    public ICommand Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly StyledProperty<KeyModifiers> ModifierProperty =
        AvaloniaProperty.Register<MouseWheelBehavior, KeyModifiers>(nameof(Modifier));

    public KeyModifiers Modifier
    {
        get => GetValue(ModifierProperty);
        set => SetValue(ModifierProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject?.AddHandler(InputElement.PointerWheelChangedEvent, AssociatedObjectOnPointerWheelChanged, handledEventsToo: true);
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject?.RemoveHandler(InputElement.PointerWheelChangedEvent, AssociatedObjectOnPointerWheelChanged);
    }

    private void AssociatedObjectOnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (e.KeyModifiers != Modifier) return;
        Command.Execute(e.Delta);
    }
}
