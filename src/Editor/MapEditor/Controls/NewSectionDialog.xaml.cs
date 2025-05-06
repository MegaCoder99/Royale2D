using Editor;
using System.Windows;

namespace MapEditor;

public partial class NewSectionDialog : Window
{
    public string name { get; private set; } = "";
    public int rows { get; private set; }
    public int cols { get; private set; }

    private IEnumerable<string> takenNames;

    public NewSectionDialog(IEnumerable<string> takenNames, int currentRows, int currentCols, bool isScratch)
    {
        InitializeComponent();
        this.takenNames = takenNames;
        rowsNic.Value = currentRows;
        colsNic.Value = currentCols;
        if (isScratch)
        {
            Title = "New Scratch Section";
        }
        else
        {
            Title = "New Map Section";
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (takenNames.Contains(NameTextBox.Text))
        {
            Prompt.ShowError("Name already used");
            return;
        }
        else if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            Prompt.ShowError("Name cannot be empty");
            return;
        }
        else if (rowsNic.Value < 1)
        {
            Prompt.ShowError("Rows must be greater than 0");
            return;
        }
        else if (colsNic.Value < 1)
        {
            Prompt.ShowError("Cols must be greater than 0");
            return;
        }

        name = NameTextBox.Text;
        rows = rowsNic.Value;
        cols = colsNic.Value;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
