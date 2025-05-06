using System.Windows;

namespace SpriteEditor;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        //new PreLinkSpriteExporter().GenerateSpritesFromPreSprites();
        //Console.WriteLine("DONE"); return;

        StartupFactory factory = new(this);
        factory.Startup(e);
    }
}
