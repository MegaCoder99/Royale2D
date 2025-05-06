using Shared;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;

namespace Editor
{
    public partial class OpenExistingWorkspacePage : Page
    {
        BaseStartupFactory startupFactory;

        public OpenExistingWorkspacePage(BaseStartupFactory startupFactory, bool disableBack)
        {
            InitializeComponent();
            this.startupFactory = startupFactory;
            if (disableBack)
            {
                backButton.Visibility = Visibility.Collapsed;
            }

            fileFolderBrowserControl.SetInitialPathAndKey(Config.OpenWorkspaceSavedPathKey, startupFactory.defaultWorkspacePath);
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = fileFolderBrowserControl.SelectedPath;
            if (!folderPath.IsSet())
            {
                Prompt.ShowError("Select a folder path.");
                return;
            }
            var folderPathObj = FolderPath.New(folderPath);
            if (!folderPathObj.Exists())
            {
                Prompt.ShowError("Folder path does not exist.");
                return;
            }

            if (!startupFactory.NewWorkspace(folderPath).IsValid(out string errorMessage))
            {
                Prompt.ShowError($"Folder is not a valid {startupFactory.displayName} workspace:\n\n{errorMessage}");
                return;
            }

            startupFactory.OpenExistingWorkspacePageNextAction(folderPath);
        }

        private void Back_Click(object sender, RoutedEventArgs e) => NavigationService.GoBack();
    }
}
