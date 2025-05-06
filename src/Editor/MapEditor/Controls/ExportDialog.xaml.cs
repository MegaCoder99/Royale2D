using Editor;
using Shared;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MapEditor;

public partial class ExportDialog : Window, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged(string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private string _selectedPath = "";
    public string selectedPath 
    {
        get => _selectedPath;
        set
        {
            _selectedPath = value;
            OnPropertyChanged(nameof(selectedPath));
        }
    }
    
    public ExportOption exportOption;

    public ExportDialog()
    {
        InitializeComponent();
        DataContext = this;
        Title = "Export map workspace";

        string savedPathKey = "ExportDialogPath";
        string defaultPath = "";

        fileFolderBrowserControl.IsFile = false;
        fileFolderBrowserControl.SetInitialPathAndKey(savedPathKey, defaultPath);

        selectedPath = fileFolderBrowserControl.SelectedPath;

        RadioNoTileset.IsChecked = true;
    }

    private void RadioButton_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb)
        {
            switch (rb.Content.ToString())
            {
                case "No Tileset":
                    exportOption = ExportOption.NoTileset;
                    break;
                case "No Images":
                    exportOption = ExportOption.NoImages;
                    break;
                case "All":
                    exportOption = ExportOption.All;
                    break;
            }
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (!selectedPath.IsSet())
        {
            Prompt.ShowError($"Folder path not set.");
            return;
        }

        if (selectedPath.IsSet() && !FolderPath.IsPathValid(selectedPath))
        {
            Prompt.ShowError($"Invalid folder path format.");
            return;
        }

        if (selectedPath.IsSet() && !Directory.Exists(selectedPath))
        {
            MessageBoxResult result = Prompt.ShowYesNoQuestion($"Folder path does not exist. Create it?", "Not found");
            if (result == MessageBoxResult.Yes)
            {
                Directory.CreateDirectory(selectedPath);
            }
            else
            {
                return;
            }
        }

        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
