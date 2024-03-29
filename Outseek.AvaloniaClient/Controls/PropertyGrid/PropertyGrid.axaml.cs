﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace Outseek.AvaloniaClient.Controls.PropertyGrid;

public class PropertyGrid : UserControl
{
    public static readonly StyledProperty<object?> PropertyObjectProperty =
        AvaloniaProperty.Register<PropertyGrid, object?>(
            nameof(PropertyObject), typeof(object), notifying: OnSelectedObjectChanged);

    public object? PropertyObject
    {
        get => GetValue(PropertyObjectProperty);
        set => SetValue(PropertyObjectProperty, value);
    }

    public event EventHandler<EventArgs>? PropertyObjectChanged;

    private readonly Grid _gridMain;

    public PropertyGrid()
    {
        InitializeComponent();

        _gridMain = this.FindControl<Grid>("GridMain");
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private static void OnSelectedObjectChanged(IAvaloniaObject sender, bool beforeChanging)
    {
        if (beforeChanging) return;
        if (sender is not PropertyGrid propGrid) return;

        propGrid.UpdatePropertyControls();
    }

    private Control GetControlForProperty(PropertyInfo propInfo)
    {
        Control control;
        AvaloniaProperty property;

        if (propInfo.PropertyType == typeof(string))
            (control, property) = (new TextBox(), TextBox.TextProperty);
        else if (new[] {typeof(int), typeof(long), typeof(float), typeof(double)}.Contains(propInfo.PropertyType))
            (control, property) = (new NumericUpDown(), NumericUpDown.ValueProperty);
        else
            throw new ArgumentException($"unsupported property type {propInfo.PropertyType}");

        control[!property] = new Binding(propInfo.Name, BindingMode.TwoWay);
        control.GetObservable(property).Subscribe(val =>
        {
            if (val != null) PropertyObjectChanged?.Invoke(this, new EventArgs());
        });
        return control;
    }

    private void UpdatePropertyControls()
    {
        _gridMain.Children.Clear();
        _gridMain.RowDefinitions.Clear();

        if (PropertyObject == null) return;

        PropertyInfo[] propertyInfos = PropertyObject.GetType().GetProperties();
        int row = 0;
        foreach (PropertyInfo propInfo in propertyInfos)
        {
            if (!propInfo.CanRead || !propInfo.CanWrite) continue;

            var displayNameAttribute = propInfo.GetCustomAttribute<DisplayNameAttribute>();
            Label label = new() {Content = displayNameAttribute?.DisplayName ?? propInfo.Name};
            label.Classes.Add("propertygrid-label");
            label.Classes.Add("propertygrid-cell-elem");
            label.SetValue(Grid.RowProperty, row);
            label.SetValue(Grid.ColumnProperty, 0);
            _gridMain.Children.Add(label);

            Control propEdit = GetControlForProperty(propInfo);
            propEdit.Classes.Add("propertygrid-cell-elem");
            propEdit.SetValue(Grid.RowProperty, row);
            propEdit.SetValue(Grid.ColumnProperty, 2);
            propEdit.DataContext = PropertyObject;
            _gridMain.Children.Add(propEdit);

            _gridMain.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            row++;
        }
    }
}
