using Editor;
using Shared;
using System.Windows;
using System.Windows.Controls;

namespace SpriteEditor;

public class StartupFactory : BaseStartupFactory
{
    public override string displayName => "sprite";
    public override SpriteWorkspace NewWorkspace(string workspacePath) => new SpriteWorkspace(workspacePath);
    public override MainWindow NewMainWindow() => new MainWindow();
    public override void InitMainWindow(Window window, IWorkspace workspace)
    {
        // These two "as" casts may seem compile-time unsafe and look awkward, but making BaseStartupFactory have generic types
        // leads to generic type propagation hell and unreadable code. A clear example of compile time safety taken too far.
        (window as MainWindow)!.Init((workspace as SpriteWorkspace)!, EditorContext.GetContext());
    }
#if DEBUG
    public override string defaultWorkspacePath => "./../../../sample_sprite_workspace";
#else
    public override string defaultWorkspacePath => "./sample_sprite_workspace";
#endif

    public StartupFactory(Application application) : base(application)
    {
    }

    public override Page GetNewWorkspacePage(string workspacePath)
    {
        SpriteWorkspace workspace = NewWorkspace(workspacePath);
        return new InitialImportPage((selectedFilePath) =>
        {
            workspace.CreateFolders();
            FilePath.New(selectedFilePath).CopyTo(workspace.spritesheetPath);
            CreateAndShowEditorWindow(workspace);
        });
    }
}

