using Editor;
using Shared;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace MapEditor;

public partial class ImportSectionDialog : Window, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public string SelectedPath { get; set; } = "";

    public ImportSectionDialog()
    {
        InitializeComponent();
        DataContext = this;
        fileBrowserControl.IsFile = true;
        fileBrowserControl.FileTypeFilter = "png";
        fileBrowserControl.SetInitialPathAndKey(Config.ImportSectionSavedPathKey, "");
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        SelectedPath = fileBrowserControl.SelectedPath;

        if (SelectedPath.Unset())
        {             
            MessageBox.Show("Please select a path", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        else if (!File.Exists(SelectedPath))
        {
            MessageBox.Show("The selected file does not exist", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        else if (Path.GetExtension(SelectedPath).Trim('.').ToLowerInvariant() != "png")
        {
            MessageBox.Show("The selected file is not a PNG file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
