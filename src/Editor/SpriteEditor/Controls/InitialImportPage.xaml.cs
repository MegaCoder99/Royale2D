using Editor;
using Shared;
using System.Windows;
using System.Windows.Controls;

namespace SpriteEditor;

public partial class InitialImportPage : Page
{
    public string imageFilePath { get; private set; } = "";
    public Action<string> okAction;

    public InitialImportPage(Action<string> okAction)
    {
        InitializeComponent();
        spritesheetFileControl.IsFile = true;
        spritesheetFileControl.FileTypeFilter = "png";
        spritesheetFileControl.SetInitialPathAndKey(Config.ImportSpritesheetSavedPathKey, "");
        this.okAction = okAction;
    }

    private void Submit_Click(object sender, RoutedEventArgs e)
    {
        string filePath = spritesheetFileControl.SelectedPath;

        if (!filePath.IsSet())
        {
            Prompt.ShowError("Select a file path.");
            return;
        }

        if (!FilePath.New(filePath).Exists())
        {
            Prompt.ShowError("File does not exist.");
            return;
        }
        if (FilePath.New(filePath).ext.ToLowerInvariant() != "png")
        {
            Prompt.ShowError("File not a PNG file.");
            return;
        }

        // IMPROVE: figure out why this doesn't nicely gray out everything. Also put "processing..."
        IsEnabled = false;
        okAction.Invoke(filePath);
    }

    private void Back_Click(object sender, RoutedEventArgs e) => NavigationService.GoBack();
}
