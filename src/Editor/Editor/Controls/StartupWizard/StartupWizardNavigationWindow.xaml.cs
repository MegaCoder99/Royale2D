using System.Windows.Controls;
using System.Windows.Navigation;

namespace Editor
{
    public partial class StartupWizardNavigationWindow : NavigationWindow
    {
        public StartupWizardNavigationWindow(Page startPage)
        {
            InitializeComponent();
            this.Navigate(startPage);
        }
    }
}
