using Shared;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Editor;

// Responsible for creating initial workspaces and windows on application startup.
// The goal is to avoid copy-paste code in each editors' App.xaml and App.xaml.cs files.
public abstract class BaseStartupFactory
{
    #region virtual methods
    // Override with display name of the workspace type. i.e. "map" or "sprite"
    public abstract string displayName { get; }

    // Override with a function that returns a new TWorkspace object from a folder path
    public abstract IWorkspace NewWorkspace(string workspacePath);

    // Override with function that returns main window of editor
    public abstract Window NewMainWindow();

    // Override with function that initializes main window of editor with workspace
    public abstract void InitMainWindow(Window window, IWorkspace workspace);

    // Override with function that returns a page for the first time setup wizard that handles creation of a new workspace, typically via file/folder import UI
    public abstract Page GetNewWorkspacePage(string workspacePath);

    public abstract string defaultWorkspacePath { get; }
    #endregion

    public Application application;
    StartupWizardNavigationWindow? startupWizardNavigationWindow;

    public BaseStartupFactory(Application application)
    {
        this.application = application;
        application.Exit += (object sender, ExitEventArgs e) =>
        {
#if !DEBUG
            try
            {
#endif
                if (Config.main?.workspacePath?.IsSet() == true && WorkspaceConfig.main != null)
                {
                    Config.main.Save();
                    WorkspaceConfig.main.Save(Config.main.workspacePath);
                }
                Logger.SaveToDisk();
#if !DEBUG
            }
            catch
            {
            }
#endif
        };
    }

    public void Startup(StartupEventArgs e)
    {
#if DEBUG
        // In local dev, just show the startup exception in debugger. No need for an annoying prompt everytime that hides stack trace
        StartupInternal(e);
#else
        try
        {
            StartupInternal(e);
        }
        catch (Exception ex)
        {
            Prompt.ShowError($"An error occurred during initialization:.\n\n{ex.Message}");
            Logger.LogException(ex);
            throw;  // Rethrow or else app process could linger without being shut down
        }
#endif
    }

    private void StartupInternal(StartupEventArgs e)
    {
        LaunchArgs.Init(e.Args);

        bool? isNewWorkspace = null;
        if (LaunchArgs.Contains(LaunchArgs.NewWorkspaceArg)) isNewWorkspace = true;
        if (LaunchArgs.Contains(LaunchArgs.OpenWorkspaceArg)) isNewWorkspace = false;

        string workspacePath = Config.main.workspacePath;
        
        Page? startupWizardPage = GetStartupWizardPage(workspacePath, isNewWorkspace);

        if (startupWizardPage != null)
        {
            startupWizardNavigationWindow = new(startupWizardPage);
            startupWizardNavigationWindow.Show();
        }
        else
        {
            CreateAndShowEditorWindow(workspacePath);
        }

        // If we reached the end of this function, it's NOT necessarily gonna be after the main editor window is shown. Any window that is returned by GetInitialWindow
        // will be the initial main window of the application, and the Startup flow would run to completion and the app would no longer be in startup at this point.
        // TLDR; if you need stuff to run once the app has truly been initialized, do it in CreateAndShowEditorWindow at the end, NOT here.
    }

    public Page? GetStartupWizardPage(string workspacePath, bool? isNewWorkspace)
    {
        StartupWizardErrorPage? startupWizardErrorPage = GetStartupWizardErrorPage(workspacePath, isNewWorkspace);
        if (startupWizardErrorPage != null)
        {
            return startupWizardErrorPage;
        }

        Page? firstTimeSetupPage = GetFirstTimeSetupWindow(workspacePath, isNewWorkspace);
        if (firstTimeSetupPage != null)
        {
            return firstTimeSetupPage;
        }

        return null;
    }

    public StartupWizardErrorPage? GetStartupWizardErrorPage(string workspacePath, bool? isNewWorkspace)
    {
        if (workspacePath.IsSet())
        {
            // The workspace path folder no longer exists. User deleted it (or a parent folder).
            if (!Directory.Exists(workspacePath))
            {
                return new StartupWizardErrorPage(
                    "Attention",
                    $"The saved {displayName} workspace path {workspacePath} no longer exists.\n\nNow proceeding to First Time Setup.",
                    new()
                    {
                        {
                            "OK",
                            (startupWizardErrorPage) =>
                            {
                                startupWizardErrorPage.NavigationService.Navigate(new FirstTimeSetupPage(this));
                            }
                        }
                    }
                );
            }
            // The workspace path folder still exists, but it became empty. User deleted everything inside it.
            else if (new FolderPath(workspacePath).IsEmpty())
            {
                return new StartupWizardErrorPage(
                    "Workspace folder was cleared out",
                    $"The saved {displayName} workspace folder path below is empty and no longer has any contents:\n\n{workspacePath}\n\nRecreate new workspace files?",
                    new()
                    {
                        {
                            "Yes", 
                            (startupWizardErrorPage) => 
                            {
                                startupWizardErrorPage.NavigationService.Navigate(GetNewWorkspacePage(workspacePath));
                            } 
                        }
                    }
                );
            }
            // The workspace path folder still exists, but its format is now invalid. User messed with the file/folder structure, or new workspace structure introduced.
            else if (!NewWorkspace(workspacePath).IsValid(out string errorMessage))
            {
                return new StartupWizardErrorPage(
                    "Error",
                    $"The saved {displayName} workspace path {workspacePath} is not a valid workspace. Please fix the errors below:\n\n{errorMessage}",
                    new()
                    {
                        {
                            "Restart to First Time Setup",
                            (startupWizardErrorPage) =>
                            {
                                startupWizardErrorPage.NavigationService.Navigate(new FirstTimeSetupPage(this));
                            }
                        },
                        { "Exit", (_) => { Application.Current.Shutdown(); } }
                    }
                );
            }
        }

        return null;
    }

    public Page? GetFirstTimeSetupWindow(string workspacePath, bool? isNewWorkspace)
    {
        // If workspace path is not set, prompt user to create or open a new workspace
        if (workspacePath.Unset() || isNewWorkspace != null)
        {
            // If isNewWorkspace is null, it means we will prompt the user to populate this value, i.e. whether or not they want to create a new workspace
            if (isNewWorkspace == null)
            {
                return new FirstTimeSetupPage(this);
            }
            // Other two options means user is changing workspace from the file menu, not from first time setup
            else if (isNewWorkspace == false)
            {
                return new OpenExistingWorkspacePage(this, true);
            }
            else if (isNewWorkspace == true)
            {
                return new CreateNewWorkspacePage(this, true);
            }
        }

        return null;
    }

    public void OpenExistingWorkspacePageNextAction(string workspacePath)
    {
        CreateAndShowEditorWindow(workspacePath);
    }

    // Creates, inits, show and returns the main editor window.

    public void CreateAndShowEditorWindow(string workspacePath)
    {
        CreateAndShowEditorWindow(NewWorkspace(workspacePath));
    }

    public void CreateAndShowEditorWindow(IWorkspace workspace)
    {
        Config.main.workspacePath = workspace.baseFolderPath.fullPath;
        Config.main.recentWorkspaces.AddIfNotExists(Config.main.workspacePath);
        WorkspaceConfig.Init(Config.main.workspacePath);

        Window editorWindow = NewMainWindow();

        editorWindow.Show();

        startupWizardNavigationWindow?.Close();

        workspace.LoadFromDisk(false);

        // Unfortunately WPF has a weird complex requirement where the window has to be shown to get DPI and scaling values used by canvases
        // This forces weird roundabout structure and "Init()" patterns in our factory and the editor window classes.
        DpiHelper.Init(editorWindow);
        UISizes.Init(editorWindow);

        InitMainWindow(editorWindow, workspace);

        // Once intialization over and app is up and running, handle global exceptions so they don't crash app in middle of work
        application.BindExceptionHandlers();
    }
}
