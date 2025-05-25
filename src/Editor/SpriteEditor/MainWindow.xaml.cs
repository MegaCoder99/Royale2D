#pragma warning disable CS8618
using Editor;
using Shared;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpriteEditor;

public partial class MainWindow : Window
{
    public State state;
    SpriteWorkspace workspace;
    bool alreadyConfirmedQuit;

    public static int animFrameIndex = 0;
    public static double animTime = 0;
    public static double animMsPerFrame = 16.66;
    public static bool isAnimPlaying;

    public MainWindow()
    {
        InitializeComponent();
    }

    public void Init(SpriteWorkspace workspace, EditorContext context)
    {
        Closing += new CancelEventHandler(OnClose);
        PreviewKeyDown += OnPreviewKeyDown;
        WindowState = WindowState.Maximized;
        
        spriteCanvasControl.AnimateAction = UpdateAnimation;
        
        this.workspace = workspace;
        scriptTextBox.Text = WorkspaceConfig.main.lastScript ?? "";

        state = new State(context, workspace, spriteCanvasControl, spritesheetCanvasControl, ScrollToFrame);
        DataContext = state;
        
        state.OnPropertyChanged(null);
        state.Redraw(RedrawData.All);

        context.Init(state);
    }

    private void OnPlayClick(object sender, RoutedEventArgs e)
    {
        ToggleAnimationPlaying();
    }

    public void ToggleAnimationPlaying()
    {
        if (!isAnimPlaying)
        {
            state.spriteCanvas.StartAnimation();
            isAnimPlaying = true;
            playButton.Content = "Stop";
        }
        else
        {
            state.spriteCanvas.StopAnimation();
            isAnimPlaying = false;
            playButton.Content = "Play";
        }
    }

    public void UpdateAnimation(double currentTimeMs)
    {
        animTime += (currentTimeMs / 1000.0);
        if (animFrameIndex >= state.selectedSprite.frames.Count) animFrameIndex = 0;
        double cutoffTime = state.selectedSprite.frames[animFrameIndex].duration / 60.0;
        if (animTime >= cutoffTime)
        {
            animFrameIndex++;
            if (animFrameIndex >= state.selectedSprite.frames.Count)
            {
                animFrameIndex = 0;
            }
            animTime -= cutoffTime;
            state.spriteCanvas.InvalidateImage();
        }
    }

    public void ScrollToFrame(Frame frame)
    {
        var container = framesItemsControl.ItemContainerGenerator.ContainerFromItem(frame) as FrameworkElement;
        if (container != null)
        {
            container.BringIntoView();
        }
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (!Helpers.FocusedElementAllowsGlobalHotkeys(this, e))
        {
            return;
        }

        Key key = e.Key;

        if (key == Key.Q)
        {
            state.SelectPrevFrameCommit();
        }
        else if (key == Key.E)
        {
            state.SelectNextFrameCommit();
        }
        else if (key == Key.Z)
        {
            if (Helpers.ControlHeld())
            {
                state.undoManager?.ApplyUndo();
            }
        }
        else if (key == Key.P)
        {
            ToggleAnimationPlaying();
        }
    }

    private void ScrollIntoViewHandler(object sender, SelectionChangedEventArgs e)
    {
        var listBox = sender as ListBox;
        if (listBox?.SelectedItem != null)
        {
            listBox.ScrollIntoView(listBox.SelectedItem);
        }
    }

    private void OnClose(object? sender, CancelEventArgs e)
    {
        if ((state.canSave || state.canSaveAll) && !alreadyConfirmedQuit)
        {
            MessageBoxResult result = Prompt.ShowExitConfirmPrompt();
            if (result != MessageBoxResult.Yes)
            {
                e.Cancel = true;
                return;
            }
        }

        // CONFIG BOOKMARK (SAVE)

        WorkspaceConfig.main.lastSpriteFilter = state.spriteFilterText;
        WorkspaceConfig.main.lastSpriteName = state.selectedSprite?.name ?? "";
        WorkspaceConfig.main.lastBoxTagFilter = state.boxTagFilter;
        WorkspaceConfig.main.lastScript = scriptTextBox.Text;
    }

    public void OnRunScriptClick(object sender, RoutedEventArgs e)
    {
        state.RunScript(scriptTextBox.Text);
    }

    public void OnAddSpriteClick(object sender, RoutedEventArgs e)
    {
        AddSpriteDialog dialog = new(state.spritesheetNames, state.selectedSprite.spritesheetName);
        if (dialog.ShowDialog() == true)
        {
            state.AddSpriteCommit(dialog.spriteName, dialog.spritesheetName);
        }
    }

    private void OnUndoClick(object sender, RoutedEventArgs e)
    {
        state.undoManager?.ApplyUndo();
    }

    private void OnRedoClick(object sender, RoutedEventArgs e)
    {
        state.undoManager?.ApplyRedo();
    }

    private void OnDebugClick(object sender, RoutedEventArgs e)
    {
        Prompt.ShowMessage("Debug button done");
    }

    private void OnSaveAllClick(object sender, RoutedEventArgs e)
    {
        state.SaveAll();
    }

    private void OnExportClick(object sender, RoutedEventArgs e)
    {
        string defaultPath = workspace.baseFolderPath.AppendFolder(Exporter.DefaultExportFolder).fullPath;
        SelectFileFolderDialog dialog = new(false, "Select folder to export to", "", "Export folder path", Config.ExportSavedPathKey, defaultPath: defaultPath, allowCreateIfNotExists: true);

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        new Exporter(state, dialog.selectedPath).Export();

        OptionsDialog optionsDialog = new("Exported successfully", $"Exported successfully to path:\n{dialog.selectedPath}", ["Open folder", "Close"]);
        optionsDialog.ShowDialog();
        if (optionsDialog.selectedButtonIndex == 0)
        {
            Prompt.OpenFolderInExplorer(dialog.selectedPath);
        }
    }

    private void OnBulkDurationApplyClick(object sender, RoutedEventArgs e)
    {
        state.ApplyBulkDurationCommit();
    }

    public void OnAddGlobalHitboxClick(object sender, RoutedEventArgs e)
    {
        state.AddGlobalHitboxCommit();
    }

    public void OnSelectGlobalHitboxClick(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is Hitbox hitbox)
        {
            state.SelectGlobalHitboxCommit(hitbox);
            state.spriteCanvas.Focus();
        }
    }

    public void OnMoveGlobalHitboxUpClick(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is Hitbox hitbox)
        {
            state.MoveGlobalHitboxCommit(hitbox, -1);
            state.spriteCanvas.Focus();
        }
    }

    public void OnMoveGlobalHitboxDownClick(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is Hitbox hitbox)
        {
            state.MoveGlobalHitboxCommit(hitbox, 1);
            state.spriteCanvas.Focus();
        }
    }

    public void OnRemoveGlobalHitboxClick(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is Hitbox hitbox)
        {
            state.RemoveGlobalHitboxCommit(hitbox);
        }
    }

    public void OnAddFrameHitboxClick(object sender, RoutedEventArgs e)
    {
        state.AddFrameHitboxCommit();
        state.spriteCanvas.Focus();
    }

    public void OnAddFrameDrawboxClick(object sender, RoutedEventArgs e)
    {
        SetDrawboxDialog setDrawboxDialog = new(state.spritesheets, state.selectedSprite.spritesheetName, null);
        if (setDrawboxDialog.ShowDialog() == true)
        {
            state.AddFrameDrawboxCommit(setDrawboxDialog.spritesheetName, setDrawboxDialog.rect);
            state.spriteCanvas.Focus();
        }
    }

    public void OnEditFrameDrawboxClick(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is Drawbox drawbox)
        {
            SetDrawboxDialog setDrawboxDialog = new(state.spritesheets, drawbox.spritesheetName, drawbox);
            if (setDrawboxDialog.ShowDialog() == true)
            {
                state.EditFrameDrawboxCommit(drawbox, setDrawboxDialog.spritesheetName, setDrawboxDialog.rect);
            }
        }
    }

    public void OnCopyToAllFramesDrawboxClick(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is Drawbox drawbox)
        {
            state.CopyToAllFramesDrawboxCommit(drawbox);
        }
    }

    public void OnSelectFrameHitboxClick(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is Hitbox hitbox)
        {
            state.SelectFrameHitboxCommit(hitbox);
            state.spriteCanvas.Focus();
        }
    }

    public void OnRemoveFrameHitboxClick(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is Hitbox hitbox)
        {
            state.RemoveFrameHitboxCommit(hitbox);
        }
    }

    public void OnSelectFrameDrawboxClick(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is Drawbox drawbox)
        {
            state.SelectFrameDrawboxCommit(drawbox);
            state.spriteCanvas.Focus();
        }
    }

    public void OnRemoveFrameDrawboxClick(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is Drawbox drawbox)
        {
            state.RemoveFrameDrawboxCommit(drawbox);
        }
    }

    public void OnSelectPOIClick(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is POI poi)
        {
            state.SelectPOICommit(poi);
            state.spriteCanvas.Focus();
        }
    }

    public void OnAddFramePOIClick(object sender, RoutedEventArgs e)
    {
        state.addPOIMode = true;
    }

    public void OnRemovePOIClick(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is POI poi)
        {
            state.RemovePOICommit(poi);
        }
    }

    private void OnSelectFrameClick(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is Frame frame)
        {
            state.SelectFrameCommit(frame);
            state.spriteCanvas.Focus();
        }
    }

    private void OnMoveFrameUpClick(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is Frame frame)
        {
            state.MoveFrameUpCommit(frame);
        }
    }

    private void OnMoveFrameDownClick(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is Frame frame)
        {
            state.MoveFrameDownCommit(frame);
        }
    }

    private void OnChangeFrameRectClick(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is Frame frame)
        {
            var changeRectDialog = new ChangeRectDialog(frame.rect.ToModel());
            if (changeRectDialog.ShowDialog() == true)
            {
                state.OnChangeFrameRectClick(frame, changeRectDialog.X1, changeRectDialog.Y1, changeRectDialog.X2, changeRectDialog.Y2);
            }
        }
    }

    private void OnDeleteFrameClick(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is Frame frame)
        {
            state.DeleteFrameCommit(frame);
        }
    }

    private void OnAddFrameClick(object sender, RoutedEventArgs e)
    {
        state.AddPendingFrameCommit();
    }

    private void OnReplaceFrameClick(object sender, RoutedEventArgs e)
    {
        state.ReplaceWithPendingFrameCommit();
    }

    private void OnRecomputeFrameClick(object sender, RoutedEventArgs e)
    {
        state.RecomputeSelectedFrameCommit();
    }

    private void OnBrowseWorkspaceClick(object sender, RoutedEventArgs e)
    {
        Prompt.OpenFolderInExplorer(Config.main.workspacePath);
    }

    private void OnRecentWorkspaceClick(object sender, RoutedEventArgs e)
    {
        if (state.canSaveAll)
        {
            MessageBoxResult result = Prompt.ShowExitConfirmPrompt();
            if (result != MessageBoxResult.Yes)
            {
                return;
            }
        }

        alreadyConfirmedQuit = true;

        if (sender is MenuItem menuItem && menuItem.Header is string selectedWorkspace)
        {
            if (Config.main.workspacePath == selectedWorkspace)
            {
                Prompt.ShowError("You are already in this workspace.");
                return;
            }

            Config.main.workspacePath = selectedWorkspace;
        }

        Helpers.RestartApplication();
    }

    private void OnChangeWorkspaceClick(object sender, RoutedEventArgs e)
    {
        if (state.canSaveAll)
        {
            MessageBoxResult result = Prompt.ShowExitConfirmPrompt();
            if (result != MessageBoxResult.Yes)
            {
                return;
            }
        }

        alreadyConfirmedQuit = true;

        Helpers.RestartApplication(LaunchArgs.OpenWorkspaceArg);
    }

    private void OnNewWorkspaceClick(object sender, RoutedEventArgs e)
    {
        if (state.canSaveAll)
        {
            MessageBoxResult result = Prompt.ShowExitConfirmPrompt();
            if (result != MessageBoxResult.Yes)
            {
                return;
            }
        }

        alreadyConfirmedQuit = true;

        Helpers.RestartApplication(LaunchArgs.NewWorkspaceArg);
    }

    private void OnReloadWorkspaceClick(object sender, RoutedEventArgs e)
    {
        if (state.canSaveAll)
        {
            MessageBoxResult result = Prompt.ShowExitConfirmPrompt();
            if (result != MessageBoxResult.Yes)
            {
                return;
            }
        }

        alreadyConfirmedQuit = true;

        Helpers.RestartApplication();
    }

    private void OnCheckVersionClick(object sender, RoutedEventArgs e)
    {
        Prompt.ShowMessage("Your sprite editor version is: " + Helpers.GetVersion());
    }
}