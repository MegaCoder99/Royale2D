using Shared;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace Editor;

public partial class SelectFileFolderDialog : Window, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged(string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private string _selectedPath;
    public string selectedPath 
    {
        get => _selectedPath;
        set
        {
            _selectedPath = value;
            OnPropertyChanged(nameof(selectedPath));
        }
    }
    
    Func<string, string>? customValidator;
    bool allowCreateIfNotExists;

    bool isFile;
    string displayName;
    string displayNameFirstWord;

    public SelectFileFolderDialog(
        bool isFile,
        string title,
        string prompt,
        string browseLabel,
        string savedPathKey,
        Func<string, string>? customValidator = null,
        string defaultPath = "",
        string fileTypeFilter = "",
        bool allowCreateIfNotExists = false)
    {
        InitializeComponent();
        DataContext = this;
        Title = title;
        this.isFile = isFile;
        displayName = isFile ? "file" : "folder";
        displayNameFirstWord = isFile ? "File" : "Folder";

        if (prompt.IsSet()) promptTextBox.Text = prompt;
        else promptTextBox.Visibility = Visibility.Collapsed;

        this.customValidator = customValidator;
        this.allowCreateIfNotExists = allowCreateIfNotExists;

        fileFolderBrowserControl.IsFile = isFile;
        fileFolderBrowserControl.FileTypeFilter = fileTypeFilter;
        fileFolderBrowserControl.Label = browseLabel;
        fileFolderBrowserControl.SetInitialPathAndKey(savedPathKey, defaultPath);

        selectedPath = fileFolderBrowserControl.SelectedPath;
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (!selectedPath.IsSet())
        {
            Prompt.ShowError($"{displayNameFirstWord} path not set.");
            return;
        }

        if (!isFile)
        {
            if (selectedPath.IsSet() && !FolderPath.IsPathValid(selectedPath))
            {
                Prompt.ShowError($"Invalid {displayName} path format.");
                return;
            }

            if (selectedPath.IsSet() && !Directory.Exists(selectedPath))
            {
                if (!allowCreateIfNotExists)
                {
                    Prompt.ShowError($"{displayNameFirstWord} path does not exist.");
                    return;
                }
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
        }
        else
        {
            if (selectedPath.IsSet() && !File.Exists(selectedPath))
            {
                Prompt.ShowError("File not found");
                return;
            }
        }

        if (customValidator != null)
        {
            string validationResult = customValidator.Invoke(selectedPath);
            if (validationResult.IsSet())
            {
                Prompt.ShowError(validationResult);
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
