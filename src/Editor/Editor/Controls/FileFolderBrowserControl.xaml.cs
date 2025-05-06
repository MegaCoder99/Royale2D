using Shared;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Editor;

public partial class FileFolderBrowserControl : UserControl
{
    public static readonly DependencyProperty SelectedPathProperty =
        DependencyProperty.Register(
            "SelectedPath",
            typeof(string),
            typeof(FileFolderBrowserControl),
            new PropertyMetadata(string.Empty, OnSelectedPathChanged));

    private static void OnSelectedPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = d as FileFolderBrowserControl;
        if (control != null)
        {
            control.OnSelectedPathChanged((string)e.OldValue, (string)e.NewValue);
        }
    }

    protected virtual void OnSelectedPathChanged(string oldValue, string newValue)
    {
        SaveLastPath();
    }

    public string SelectedPath
    {
        get { return (string)GetValue(SelectedPathProperty); }
        set 
        {
            SetValue(SelectedPathProperty, value);
        }
    }

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            "Label",
            typeof(string),
            typeof(FileFolderBrowserControl),
            new PropertyMetadata(string.Empty));

    public string Label
    {
        get { return (string)GetValue(LabelProperty); }
        set { SetValue(LabelProperty, value); }
    }

    public bool IsFile;
    public string FileTypeFilter = "";

    string savedPathKey = "";
    Dictionary<string, string> lastBrowsePaths => IsFile ? Config.main.lastFileBrowserPaths : Config.main.lastFolderBrowserPaths;
    
    public FileFolderBrowserControl()
    {
        InitializeComponent();
    }

    public void SetInitialPathAndKey(string savedPathKey, string defaultPath)
    {
        this.savedPathKey = savedPathKey;
        string savedPath = lastBrowsePaths.GetValueOrDefault(savedPathKey, defaultPath);
        if (savedPath.IsSet())
        {
            SelectedPath = savedPath;
        }
    }

    public void SaveLastPath()
    {
        if (SelectedPath.IsSet() && savedPathKey.IsSet())
        {
            lastBrowsePaths[savedPathKey] = SelectedPath;
        }
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        if (!IsFile)
        {
            string selectedPath = Prompt.SelectFolder("Select a folder...", SelectedPath ?? "");
            if (selectedPath.IsSet())
            {
                SelectedPath = selectedPath;
            }
        }
        else
        {
            string selectedPath = Prompt.SelectFile("Select a file...", Path.GetDirectoryName(SelectedPath) ?? "", FileTypeFilter);
            if (selectedPath.IsSet())
            {
                SelectedPath = selectedPath;
            }
        }
    }
}