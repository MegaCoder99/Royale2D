using System.Windows;

namespace Editor;

public partial class OptionsDialog : Window
{
    public List<string> buttonTexts { get; set; }
    public int selectedButtonIndex { get; private set; }

    public OptionsDialog(string title, string description, List<string> buttons)
    {
        InitializeComponent();
        DataContext = this;
        Title = title;
        descriptionText.Text = description;
        buttonTexts = buttons;
    }

    private void OnButtonClick(object sender, RoutedEventArgs e)
    {
        FrameworkElement button = (sender as FrameworkElement)!;
        selectedButtonIndex = buttonTexts.IndexOf((button.DataContext as string)!);

        DialogResult = true;
    }
}
