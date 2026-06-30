using System.Collections;
using System.Windows.Input;
using CommunityToolkit.Maui.Views;

namespace ISOTankApp.Controls;

public partial class CustomDropdown : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(CustomDropdown), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(CustomDropdown), null);

    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(CustomDropdown), null, BindingMode.TwoWay, propertyChanged: OnSelectedItemChanged);

    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public static readonly BindableProperty ItemDisplayBindingProperty =
        BindableProperty.Create(nameof(ItemDisplayBinding), typeof(string), typeof(CustomDropdown), null, propertyChanged: OnItemDisplayBindingChanged);

    public string ItemDisplayBinding
    {
        get => (string)GetValue(ItemDisplayBindingProperty);
        set => SetValue(ItemDisplayBindingProperty, value);
    }

    public event EventHandler<EventArgs> SelectedIndexChanged;

    public CustomDropdown()
    {
        InitializeComponent();
    }

    private static void OnItemDisplayBindingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (CustomDropdown)bindable;
        if (newValue is string path && !string.IsNullOrEmpty(path))
        {
            control.NativePicker.ItemDisplayBinding = new Binding(path);
        }
    }

    private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (CustomDropdown)bindable;
        if (newValue != null)
        {
            if (!string.IsNullOrEmpty(control.ItemDisplayBinding))
            {
                var propertyInfo = newValue.GetType().GetProperty(control.ItemDisplayBinding);
                if (propertyInfo != null)
                {
                    control.SelectedTextLabel.Text = propertyInfo.GetValue(newValue)?.ToString() ?? control.Title;
                    control.SelectedTextLabel.TextColor = Color.FromArgb("#495057");
                }
                else 
                {
                    control.SelectedTextLabel.Text = newValue.ToString();
                    control.SelectedTextLabel.TextColor = Color.FromArgb("#495057");
                }
            }
            else
            {
                control.SelectedTextLabel.Text = newValue.ToString();
                control.SelectedTextLabel.TextColor = Color.FromArgb("#495057");
            }
        }
        else
        {
            control.SelectedTextLabel.Text = control.Title;
            control.SelectedTextLabel.TextColor = Color.FromArgb("#6C757D");
        }
        
        control.SelectedIndexChanged?.Invoke(control, EventArgs.Empty);
    }
}
