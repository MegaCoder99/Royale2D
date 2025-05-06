using Editor;
using Shared;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MapEditor;

public partial class InitialImportPage : Page
{
    public string scratchImageFolderPath { get; private set; } = "";
    public string mapImageFolderPath { get; private set; } = "";
    public int tileSize { get; private set; }
    public Action<int, string, string> okAction;

    public InitialImportPage(Action<int, string, string> okAction)
    {
        InitializeComponent();
        tileSizeNic.Value = 8;
        scratchFolderBrowserControl.SetInitialPathAndKey(Config.ImportInitialMapSectionSavedPathKey, "");
        sectionFolderBrowserControl.SetInitialPathAndKey(Config.ImportInitialScratchSectionSavedPathKey, "");
        this.okAction = okAction;
    }

    private void Submit_Click(object sender, RoutedEventArgs e)
    {
        mapImageFolderPath = sectionFolderBrowserControl.SelectedPath;
        scratchImageFolderPath = scratchFolderBrowserControl.SelectedPath;
        tileSize = tileSizeNic.Value;

        if (scratchImageFolderPath.Unset() && mapImageFolderPath.Unset())
        {
            Prompt.ShowError("Both paths cannot be empty");
            return;
        }
        else if (tileSize < Tileset.MinTileSize)
        {
            Prompt.ShowError($"Tile size must be at least {Tileset.MinTileSize}");
            return;
        }
        else if (tileSize > Tileset.MaxTileSize)
        {
            Prompt.ShowError($"Tile size cannot exceed {Tileset.MaxTileSize}");
            return;
        }
        else if (scratchImageFolderPath.IsSet() && !Directory.Exists(scratchImageFolderPath))
        {
            Prompt.ShowError("scratchImageFolderPath not found");
            return;
        }
        else if (mapImageFolderPath.IsSet() && !Directory.Exists(mapImageFolderPath))
        {
            Prompt.ShowError("sectionImageFolderPath not found");
            return;
        }
        
        // This currently doesn't nicely gray out everything as thought for some reason. Also could put "processing..." for better UI.
        IsEnabled = false;
        okAction.Invoke(tileSize, mapImageFolderPath, scratchImageFolderPath);
    }

    private void Back_Click(object sender, RoutedEventArgs e) => NavigationService.GoBack();
}
