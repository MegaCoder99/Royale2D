using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Editor
{
    public partial class FirstTimeSetupPage : Page
    {
        BaseStartupFactory startupFactory;

        public FirstTimeSetupPage(BaseStartupFactory startupFactory)
        {
            InitializeComponent();
            this.startupFactory = startupFactory;
        }

        private void CreateNewWorkspace_Click(object sender, RoutedEventArgs e) =>
            NavigationService.Navigate(new CreateNewWorkspacePage(startupFactory, false));

        private void OpenExistingWorkspace_Click(object sender, RoutedEventArgs e) =>
            NavigationService.Navigate(new OpenExistingWorkspacePage(startupFactory, false));

        private void Exit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
    }
}