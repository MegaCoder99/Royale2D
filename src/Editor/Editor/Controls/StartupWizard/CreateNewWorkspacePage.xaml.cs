using Shared;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Editor
{
    public partial class CreateNewWorkspacePage : Page
    {
        BaseStartupFactory startupFactory;
        public CreateNewWorkspacePage(BaseStartupFactory startupFactory, bool disableBack)
        {
            InitializeComponent();
            this.startupFactory = startupFactory;
            if (disableBack)
            {
                backButton.Visibility = Visibility.Collapsed;
            }
            fileFolderBrowserControl.SetInitialPathAndKey(Config.NewWorkspaceSavedPathKey, "");
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

            if (!folderPathObj.IsEmpty())
            {
                MessageBoxResult result = Prompt.ShowYesNoQuestion(
                    "Workspace folder is not empty. Delete contents of this folder and proceed?", "Workspace folder not empty",
                    defaultSelection: MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)
                {
                    folderPathObj.DeleteAndRecreate();
                }
                else
                {
                    return;
                }
            }
            
            NavigationService.Navigate(startupFactory.GetNewWorkspacePage(folderPath));
        }

        private void Back_Click(object sender, RoutedEventArgs e) => NavigationService.GoBack();
    }
}
