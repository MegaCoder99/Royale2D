using Editor;
using Shared;
using System.Windows;

namespace MapEditor;

public partial class ResizeMapDialog : Window
{
    public int rows { get; private set; }
    public int cols { get; private set; }
    public bool fromTopLeft { get; private set; }
    SectionsSC sectionsSC;

    public ResizeMapDialog(string title, SectionsSC sectionsSC)
    {
        InitializeComponent();
        Title = title;
        rowsNic.Value = sectionsSC.selectedMapSection.rowCount;
        colsNic.Value = sectionsSC.selectedMapSection.colCount;
        this.sectionsSC = sectionsSC;
        if (sectionsSC.selectedTileCoords.Count != 1)
        {
            toSelectedCellButton.Visibility = Visibility.Hidden;
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        rows = rowsNic.Value;
        cols = colsNic.Value;
        fromTopLeft = fromTopLeftCheckbox.IsChecked == true;

        if (rows < 1)
        {
            Prompt.ShowError("Rows must be greater than 0");
            return;
        }
        else if (cols < 1)
        {
            Prompt.ShowError("Cols must be greater than 0");
            return;
        }
        else if (rows == sectionsSC.selectedMapSection.rowCount && cols == sectionsSC.selectedMapSection.colCount)
        {
            Prompt.ShowError("New size must be different from the current size");
            return;
        }

        DialogResult = true;
    }

    private void OnToSelectedCellClick(object sender, RoutedEventArgs e)
    {
        GridCoords selectedCell = sectionsSC.selectedTileCoords[0];
        if (fromTopLeftCheckbox.IsChecked == true)
        {
            rowsNic.Value = sectionsSC.selectedMapSection.rowCount - selectedCell.i;
            colsNic.Value = sectionsSC.selectedMapSection.colCount - selectedCell.j;
        }
        else
        {
            rowsNic.Value = selectedCell.i + 1;
            colsNic.Value = selectedCell.j + 1;
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
