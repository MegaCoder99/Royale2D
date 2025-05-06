using System.Windows;

namespace Editor;

public partial class TextInputDialog : Window
{
    public TextInputDialog(string title, string question, string defaultAnswer = "")
    {
        InitializeComponent();
        this.Title = title; // Set the window title dynamically
        QuestionLabel.Content = question;
        InputTextBox.Text = defaultAnswer;
    }

    public string Answer => InputTextBox.Text;

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        this.DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        this.DialogResult = false;
    }
}