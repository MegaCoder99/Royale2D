using System.Windows;
using System.Windows.Controls;

namespace Editor
{
    public partial class StartupWizardErrorPage : Page
    {
        public string PageTitle { get; set; }
        public string PagePrompt { get; set; }
        private Dictionary<string, Action<StartupWizardErrorPage>> buttonActions = new();

        public StartupWizardErrorPage(string title, string prompt, Dictionary<string, Action<StartupWizardErrorPage>> actions)
        {
            InitializeComponent();
            PageTitle = title;
            PagePrompt = prompt;
            buttonActions = actions;
            DataContext = this;

            foreach (KeyValuePair<string, Action<StartupWizardErrorPage>> action in buttonActions)
            {
                var button = new Button { Content = action.Key, Width = 150, Margin = new Thickness(5) };
                button.Click += (s, e) => action.Value(this);
                ButtonContainer.Children.Add(button);
            }
        }
    }
}
