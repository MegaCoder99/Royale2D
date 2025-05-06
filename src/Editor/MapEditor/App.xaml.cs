using System.Windows;

namespace MapEditor;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        StartupFactory factory = new(this);
        factory.Startup(e);
    }
}
