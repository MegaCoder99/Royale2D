using Editor;
using Shared;
using System.Windows;
using System.Windows.Controls;

namespace SpriteEditor;

public partial class SetDrawboxDialog : Window
{
    public MyRect rect = new MyRect();
    public string spritesheetName = "";
    public DrawboxCanvas drawboxCanvas;
    public List<Spritesheet> spritesheets;

    public SetDrawboxDialog(List<Spritesheet> spritesheets, string preselectedSpritesheetName, Drawbox? drawboxToEdit)
    {
        InitializeComponent();
        this.spritesheets = spritesheets;
        spritesheetComboBox.ItemsSource = spritesheets.Select(s => s.name);
        spritesheetComboBox.SelectedItem = preselectedSpritesheetName;
        drawboxCanvas = new DrawboxCanvas(drawboxCanvasControl, spritesheets.First(s => s.name == preselectedSpritesheetName), 2);
        if (drawboxToEdit != null)
        {
            Title = "Edit Drawbox Data";
            drawboxCanvas.rect = drawboxToEdit.rect;
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        spritesheetName = (spritesheetComboBox.SelectedItem as string) ?? "";
        
        if (drawboxCanvas.rect == null)
        {
            Prompt.ShowError("Please select a pixel clump.");
            return;
        }
        
        rect = drawboxCanvas.rect.Value;

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

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var comboBox = sender as ComboBox;
        string selectedItem = (comboBox?.SelectedItem as string)!;
        drawboxCanvas?.ChangeSpritesheet(spritesheets.First(s => s.name == selectedItem));
    }
}