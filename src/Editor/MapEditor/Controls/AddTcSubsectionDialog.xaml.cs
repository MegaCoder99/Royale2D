using Editor;
using Shared;
using System.Windows;

namespace MapEditor;

public partial class AddTcSubsectionDialog : Window
{
    public string name => nameInputTextBox.Text;

    public AddTcSubsectionDialog()
    {
        InitializeComponent();
        nameInputTextBox.Text = "AboveLayer";
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (!name.IsSet())
        {
            Prompt.ShowError("Name is required.");
            return;
        }

        this.DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        this.DialogResult = false;
    }
}