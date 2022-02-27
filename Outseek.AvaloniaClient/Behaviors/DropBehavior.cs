using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace Outseek.AvaloniaClient.Behaviors;

public class DropBehavior : Behavior<Control>
{
    public static readonly StyledProperty<ICommand> CommandProperty =
        AvaloniaProperty.Register<DropBehavior, ICommand>(nameof(Command));

    public ICommand Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly StyledProperty<KeyModifiers?> ModifierProperty =
        AvaloniaProperty.Register<DropBehavior, KeyModifiers?>(nameof(Modifier));

    public KeyModifiers? Modifier
    {
        get => GetValue(ModifierProperty);
        set => SetValue(ModifierProperty, value);
    }

    public static readonly StyledProperty<DragDropEffects> DragDropEffectProperty =
        AvaloniaProperty.Register<DropBehavior, DragDropEffects>(nameof(DragDropEffect));

    public DragDropEffects DragDropEffect
    {
        get => GetValue(DragDropEffectProperty);
        set => SetValue(DragDropEffectProperty, value);
    }

    public static readonly StyledProperty<string?> DropHoverClassProperty =
        AvaloniaProperty.Register<DropBehavior, string?>(nameof(DropHoverClass));

    public string? DropHoverClass
    {
        get => GetValue(DropHoverClassProperty);
        set => SetValue(DropHoverClassProperty, value);
    }

    public static readonly StyledProperty<Func<DragEventArgs, bool>> DropAllowedProperty =
        AvaloniaProperty.Register<DropBehavior, Func<DragEventArgs, bool>>(nameof(DropAllowed));

    public Func<DragEventArgs, bool> DropAllowed
    {
        get => GetValue(DropAllowedProperty);
        set => SetValue(DropAllowedProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject?.AddHandler(DragDrop.DropEvent, Drop);
        AssociatedObject?.AddHandler(DragDrop.DragOverEvent, DragOver);
        AssociatedObject?.AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AssociatedObject?.AddHandler(DragDrop.DragLeaveEvent, DragLeave);
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject?.RemoveHandler(DragDrop.DropEvent, Drop);
        AssociatedObject?.RemoveHandler(DragDrop.DragOverEvent, DragOver);
        AssociatedObject?.RemoveHandler(DragDrop.DragEnterEvent, DragEnter);
        AssociatedObject?.RemoveHandler(DragDrop.DragLeaveEvent, DragLeave);
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        if (DropHoverClass != null) (sender as StyledElement)?.Classes.Remove(DropHoverClass);
        e.Handled = true;
        Command.Execute(e);
    }

    private void DragOver(object? sender, DragEventArgs e)
    {
        bool modifierOk = Modifier == null || Modifier == e.KeyModifiers;
        e.DragEffects = modifierOk && DropAllowed(e) ? DragDropEffect : DragDropEffects.None;
    }

    private void DragEnter(object? sender, DragEventArgs e)
    {
        if (DropHoverClass != null) (sender as StyledElement)?.Classes.Add(DropHoverClass);
    }

    private void DragLeave(object? sender, RoutedEventArgs e)
    {
        if (DropHoverClass != null) (sender as StyledElement)?.Classes.Remove(DropHoverClass);
    }
}
