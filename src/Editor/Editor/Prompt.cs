using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Editor;

public static class Prompt
{
    public static string SelectFile(string title, string initialDirectory, string fileExtensionFilter)
    {
        if (!Path.Exists(initialDirectory)) initialDirectory = "";
        initialDirectory = initialDirectory.Replace("/", "\\");

        var fileDialog = new OpenFileDialog
        {
            Title = title,
            InitialDirectory = initialDirectory,
            Filter = $"{fileExtensionFilter.ToUpperInvariant()} files (*.{fileExtensionFilter})|*.{fileExtensionFilter}|All files (*.*)|*.*"
        };

        if (fileDialog.ShowDialog() == true)
        {
            string fileName = fileDialog.FileName;
            return fileName;
        }

        return "";
    }

    public static string SelectFolder(string title, string initialDirectory)
    {
        if (!Path.Exists(initialDirectory)) initialDirectory = "";
        initialDirectory = initialDirectory.Replace("/", "\\");

        var folderDialog = new OpenFolderDialog
        {
            Title = title,
            InitialDirectory = initialDirectory
        };

        if (folderDialog.ShowDialog() == true)
        {
            var folderName = folderDialog.FolderName;
            return folderName;
        }

        return "";
    }

    public static void OpenFolderInExplorer(string folderPath)
    {
        if (!string.IsNullOrEmpty(folderPath) && System.IO.Directory.Exists(folderPath))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = folderPath,
                UseShellExecute = true, // Ensures Explorer is used
                Verb = "open" // Optional, explicitly specifies the action
            });
        }
        else
        {
            ShowError("The folder path does not exist.");
        }
    }

    public static void ShowMessage(string message, string title = "Attention")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public static void ShowWarning(string message, string title = "Warning")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public static string ShowError(string error, string title = "Error")
    {
        MessageBox.Show(error, title, MessageBoxButton.OK, MessageBoxImage.Error);
        return error;
    }

    public static MessageBoxResult ShowYesNoQuestion(string message, string title, MessageBoxResult defaultSelection = MessageBoxResult.Yes)
    {
        return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question, defaultSelection);
    }

    public static MessageBoxResult ShowExitConfirmPrompt()
    {
        return ShowYesNoQuestion("You have unsaved changes. Are you sure you want to exit?", "Attention: unsaved changes", MessageBoxResult.No);
    }
}
