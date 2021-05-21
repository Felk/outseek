using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;

namespace Outseek.AvaloniaClient.Behaviors
{
    public class InitializedBehaviour : Behavior<Control>
    {
        public static readonly StyledProperty<ICommand> CommandProperty =
            AvaloniaProperty.Register<InitializedBehaviour, ICommand>(nameof(Command));

        public ICommand Command
        {
            get => GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
                AssociatedObject.Initialized += AssociatedObjectOnInitialized;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
                AssociatedObject.Initialized -= AssociatedObjectOnInitialized;
        }

        private void AssociatedObjectOnInitialized(object? sender, EventArgs e)
        {
            Command.Execute(null);
        }
    }
}
