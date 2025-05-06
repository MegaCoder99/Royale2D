using Editor;
using Shared;
using System.Windows;

namespace SpriteEditor;

public partial class AddSpriteDialog : Window
{
    public string spriteName = "";
    public string spritesheetName = "";

    public AddSpriteDialog(IEnumerable<string> spritesheetNames, string preselectedSpritesheetName)
    {
        InitializeComponent();
        SpritesheetComboBox.ItemsSource = spritesheetNames;
        SpritesheetComboBox.SelectedItem = preselectedSpritesheetName;
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        spriteName = InputTextBox.Text;
        spritesheetName = (SpritesheetComboBox.SelectedItem as string) ?? "";

        if (spriteName.Unset())
        {
            Prompt.ShowError("Please enter a sprite name.");
            return;
        }
        if (spritesheetName.Unset())
        {
            Prompt.ShowError("Please select a spritesheet.");
            return;
        }
        this.DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        this.DialogResult = false;
    }
}