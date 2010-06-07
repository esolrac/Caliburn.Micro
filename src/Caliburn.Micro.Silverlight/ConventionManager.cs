﻿namespace Caliburn.Micro
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Markup;

#if !SILVERLIGHT
    using System.Windows.Documents;
#endif

    public static class ConventionManager
    {
        static readonly BooleanToVisibilityConverter BooleanToVisibilityConverter = new BooleanToVisibilityConverter();

#if SILVERLIGHT
        public static DataTemplate DefaultDataTemplate = (DataTemplate)XamlReader.Load(
#else
        public static DataTemplate DefaultDataTemplate = (DataTemplate)XamlReader.Parse(
#endif
            "<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' " +
                           "xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' " +
                           "xmlns:cal='http://www.caliburnproject.org'> " +
                "<ContentControl cal:View.Model=\"{Binding}\" />" +
            "</DataTemplate>"
            );

        static readonly Dictionary<Type, ElementConvention> ElementConventions = new Dictionary<Type, ElementConvention>();

        public static Func<string, string> Singularize = original =>{
            return original.TrimEnd('s');
        };

        public static Action<Binding, ElementConvention, PropertyInfo> ApplyBindingMode = (binding, convention, property) =>{
            var setMethod = property.GetSetMethod();
            var canWriteToProperty = property.CanWrite && setMethod != null && setMethod.IsPublic;

            binding.Mode = canWriteToProperty ? BindingMode.TwoWay : BindingMode.OneWay;
        };

        public static Action<Binding, ElementConvention, PropertyInfo> ApplyValidation = (binding, convention, property) => {
#if SILVERLIGHT
            if(typeof(INotifyDataErrorInfo).IsAssignableFrom(property.DeclaringType))
                binding.ValidatesOnNotifyDataErrors = true;
            else 
#endif
            if(typeof(IDataErrorInfo).IsAssignableFrom(property.DeclaringType))
                binding.ValidatesOnDataErrors = true;
        };

        public static Action<Binding, ElementConvention, PropertyInfo> ApplyValueConverter = (binding, convention, property) =>{
            if(convention.BindableProperty == UIElement.VisibilityProperty && typeof(bool).IsAssignableFrom(property.PropertyType))
                binding.Converter = BooleanToVisibilityConverter;
        };

        public static Action<Binding, ElementConvention, PropertyInfo> ApplyStringFormat = (binding, convention, property) =>{
            if (typeof(DateTime).IsAssignableFrom(property.PropertyType))
                binding.Source = "{0:MM/dd/yyyy}";
        };

        static ConventionManager()
        {
#if SILVERLIGHT
            AddElementConvention<HyperlinkButton>(HyperlinkButton.ContentProperty, "DataContext", "Click");
#else
            AddElementConvention<Hyperlink>(Hyperlink.DataContextProperty, "DataContext", "Click");
            AddElementConvention<RichTextBox>(RichTextBox.DataContextProperty, "DataContext", "TextChanged");
            AddElementConvention<Menu>(Menu.ItemsSourceProperty,"DataContext", "Click");
            AddElementConvention<MenuItem>(MenuItem.ItemsSourceProperty, "DataContext", "Click");
            AddElementConvention<Label>(Label.ContentProperty, "Content", "DataContextChanged");
            AddElementConvention<DockPanel>(DockPanel.VisibilityProperty, "DataContext", "Loaded");
            AddElementConvention<UniformGrid>(UniformGrid.VisibilityProperty, "DataContext", "Loaded");
            AddElementConvention<WrapPanel>(WrapPanel.VisibilityProperty, "DataContext", "Loaded");
            AddElementConvention<Viewbox>(Viewbox.VisibilityProperty, "DataContext", "Loaded");
            AddElementConvention<BulletDecorator>(BulletDecorator.VisibilityProperty, "DataContext", "Loaded");
            AddElementConvention<Slider>(Slider.ValueProperty, "Value", "ValueChanged");
            AddElementConvention<Expander>(Expander.IsExpandedProperty, "IsExpanded", "Expanded");
            AddElementConvention<Window>(Window.DataContextProperty, "DataContext", "Loaded");
            AddElementConvention<StatusBar>(StatusBar.ItemsSourceProperty, "DataContext", "Loaded");
            AddElementConvention<ToolBar>(ToolBar.ItemsSourceProperty, "DataContext", "Loaded");
            AddElementConvention<ToolBarTray>(ToolBarTray.VisibilityProperty, "DataContext", "Loaded");
            AddElementConvention<TreeView>(TreeView.ItemsSourceProperty, "SelectedItem", "SelectedItemChanged");
            AddElementConvention<TabControl>(TabControl.ItemsSourceProperty, "ItemsSource", "SelectionChanged");
            AddElementConvention<TabItem>(TabItem.ContentProperty, "DataContext", "DataContextChanged");
            AddElementConvention<ListView>(ListView.ItemsSourceProperty, "SelectedItem", "SelectionChanged");
#endif
            AddElementConvention<ListBox>(ListBox.ItemsSourceProperty, "SelectedItem", "SelectionChanged");
            AddElementConvention<UserControl>(UserControl.VisibilityProperty, "DataContext", "Loaded");
            AddElementConvention<ComboBox>(ComboBox.ItemsSourceProperty, "SelectedItem", "SelectionChanged");
            AddElementConvention<Image>(Image.SourceProperty, "Source", "Loaded");
            AddElementConvention<ButtonBase>(ButtonBase.ContentProperty, "DataContext", "Click");
            AddElementConvention<Button>(Button.ContentProperty, "DataContext", "Click");
            AddElementConvention<ToggleButton>(ToggleButton.IsCheckedProperty, "IsChecked", "Click");
            AddElementConvention<RadioButton>(RadioButton.IsCheckedProperty, "IsChecked", "Click");
            AddElementConvention<CheckBox>(CheckBox.IsCheckedProperty, "IsChecked", "Click");
            AddElementConvention<TextBox>(TextBox.TextProperty, "Text", "TextChanged");
            AddElementConvention<TextBlock>(TextBlock.TextProperty, "Text", "DataContextChanged");
            AddElementConvention<StackPanel>(StackPanel.VisibilityProperty, "DataContext", "Loaded");
            AddElementConvention<Grid>(Grid.VisibilityProperty, "DataContext", "Loaded");
            AddElementConvention<Border>(Border.VisibilityProperty, "DataContext", "Loaded");
            AddElementConvention<ItemsControl>(ItemsControl.ItemsSourceProperty, "DataContext", "Loaded");
            AddElementConvention<ContentControl>(View.ModelProperty, "DataContext", "Loaded"); 
        }

        public static void AddElementConvention<T>(DependencyProperty bindableProperty, string parameterProperty, string eventName)
        {
            AddElementConvention(new ElementConvention(typeof(T), bindableProperty, parameterProperty, () => new System.Windows.Interactivity.EventTrigger{ EventName = eventName }));
        }

        public static void AddElementConvention(ElementConvention convention)
        {
            ElementConventions[convention.ElementType] = convention;
        }

        public static ElementConvention GetElementConvention(Type elementType)
        {
            if (elementType == null)
                return null;

            ElementConvention propertyConvention;
            ElementConventions.TryGetValue(elementType, out propertyConvention);
            return propertyConvention ?? GetElementConvention(elementType.BaseType);
        }
    }
}