using System.Windows.Controls;
using System.Windows;

namespace Editor;

public partial class NumericInputControl : UserControl
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        "Value",
        typeof(int),
        typeof(NumericInputControl),
        new FrameworkPropertyMetadata(
            0,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnValueChanged
        ));

    public int Value
    {
        get { return (int)GetValue(ValueProperty); }
        set { SetValue(ValueProperty, value); }
    }

    public NumericInputControl()
    {
        InitializeComponent();
        // Set DataContext for the TextBox explicitly instead of the whole UserControl
        NumberTextBox.DataContext = this;
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Handle when the Value property changes if there's specific logic needed
        if (d is NumericInputControl control)
        {
            if (e.NewValue != e.OldValue) // This check is good practice
            {
                // Optionally, handle any additional logic when the value changes
            }
        }
    }

    private void IncrementButton_Click(object sender, RoutedEventArgs e)
    {
        Value++;
    }

    private void DecrementButton_Click(object sender, RoutedEventArgs e)
    {
        Value--;
    }

    private void NumberTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(NumberTextBox.Text, out int newValue))
        {
            Value = newValue; // Directly update, removing unnecessary check
        }
        else
        {
            NumberTextBox.Text = Value.ToString(); // Revert to last valid value if parse fails
        }
    }
}
