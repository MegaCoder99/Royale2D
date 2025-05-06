#pragma warning disable CS8618
using Editor;
using Shared;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MapEditor;

public partial class MainWindow : Window
{
    State state;
    MapWorkspace workspace;
    bool alreadyConfirmedQuit;

    public MainWindow()
    {
        InitializeComponent();
    }

    public void Init(MapWorkspace workspace, EditorContext context)
    {
        WindowState = WindowState.Maximized;
        Closing += new CancelEventHandler(OnClose);
        PreviewKeyDown += OnPreviewKeyDown;

        this.workspace = workspace;
        scriptTextBox.Text = WorkspaceConfig.main.lastMapScript ?? "";

        state = new State(context, workspace, mapCanvasControl, scratchCanvasControl);
        DataContext = state;

        state.OnPropertyChanged(null);
        state.RedrawAll();

        context.Init(state);
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (!Helpers.FocusedElementAllowsGlobalHotkeys(this, e))
        {
            return;
        }

        Key key = e.Key;

        if (key == Key.Z)
        {
            if (Helpers.ControlHeld())
            {
                state.undoManager?.ApplyUndo();
            }
        }
    }

    public void OnRunScriptClick(object sender, RoutedEventArgs e)
    {
        state.RunScript(scriptTextBox.Text);
    }

    private void OnClose(object? sender, CancelEventArgs e)
    {
        if (state.canSaveAll && !alreadyConfirmedQuit)
        {
            MessageBoxResult result = Prompt.ShowExitConfirmPrompt();
            if (result != MessageBoxResult.Yes)
            {
                e.Cancel = true;
                return;
            }
        }

        if (!string.IsNullOrEmpty(state.mapSectionsSC.selectedMapSection?.name))
        {
            WorkspaceConfig.main.lastMapSection = state.mapSectionsSC.selectedMapSection.name;
            WorkspaceConfig.main.lastMapSectionZoom = state.mapSectionsSC.zoom;
            WorkspaceConfig.main.lastMapCanvasScrollX = state.mapSectionsSC.scrollX;
            WorkspaceConfig.main.lastMapCanvasScrollY = state.mapSectionsSC.scrollY;
        }

        if (!string.IsNullOrEmpty(state.scratchSectionsSC.selectedMapSection?.name))
        {
            WorkspaceConfig.main.lastScratchSection = state.scratchSectionsSC.selectedMapSection.name;
            WorkspaceConfig.main.lastScratchCanvasZoom = state.scratchSectionsSC.zoom;
            WorkspaceConfig.main.lastScratchCanvasScrollX = state.scratchSectionsSC.scrollX;
            WorkspaceConfig.main.lastScratchCanvasScrollY = state.scratchSectionsSC.scrollY;
        }

        // CONFIG BOOKMARK (SAVE)

        WorkspaceConfig.main.lastMapScript = scriptTextBox.Text;
        WorkspaceConfig.main.lastMode = (int)state.selectedMode;

        WorkspaceConfig.main.showGrid = state.showGrid;
        WorkspaceConfig.main.showEntities = state.showEntities;
        WorkspaceConfig.main.showTileHitboxes = state.showTileHitboxes;
        WorkspaceConfig.main.showTileZIndices = state.showTileZIndices;
        WorkspaceConfig.main.showTileAnimations = state.showTileAnimations;
        WorkspaceConfig.main.showTileClumps = state.showTileClumps;
        WorkspaceConfig.main.showSameTiles = state.showSameTiles;
        WorkspaceConfig.main.hideScratchSection = state.hideScratchCanvas;
        WorkspaceConfig.main.mapCanvasMagentaBgColor = state.mapSectionsSC.magentaBgColor;
        WorkspaceConfig.main.scratchCanvasMagentaBgColor = state.scratchSectionsSC.magentaBgColor;
        WorkspaceConfig.main.lastSelectedMapLayerIndices = state.lastSelectedSectionsSC.selectedMapSection.layers.Select(
            (value, index) => new { value, index }).Where(vi => vi.value.isSelected).Select(vi => vi.index).ToList();
        WorkspaceConfig.main.showUnselectedMapLayers = state.mapSectionsSC.showUnselectedLayers;
    }

    private void ScrollIntoViewHandler(object sender, SelectionChangedEventArgs e)
    {
        var listBox = sender as ListBox;
        if (listBox?.SelectedItem != null)
        {
            listBox.ScrollIntoView(listBox.SelectedItem);
        }
    }

    private void ScrollIntoViewHandlerInTab(object sender, SelectionChangedEventArgs e)
    {
        ScrollIntoViewHandler(sender, e);
        var control = sender as FrameworkElement;
        var tabItem = FindParent<TabItem>(control!);
        if (tabItem != null)
        {
            // Set the TabItem as the selected tab
            clumpAnimTabControl.SelectedItem = tabItem;
        }
    }

    private T FindParent<T>(FrameworkElement child) where T : FrameworkElement
    {
        var parent = child.Parent;
        while (parent != null && !(parent is T))
        {
            parent = (parent as FrameworkElement)?.Parent;
        }
        return (parent as T)!;
    }

    private void OnUndoClick(object sender, RoutedEventArgs e)
    {
        state.undoManager?.ApplyUndo();
    }

    private void OnRedoClick(object sender, RoutedEventArgs e)
    {
        state.undoManager?.ApplyRedo();
    }

    private void OnSaveAllClick(object sender, RoutedEventArgs e)
    {
        state.SaveAll();
    }

    private void OnExportClick(object sender, RoutedEventArgs e)
    {
        state.Export();
    }

    private void OnAddLayerClick(object sender, RoutedEventArgs e)
    {
        state.mapSectionsSC.AddLayerCommit();
    }

    private void OnSelectLayerClick(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is MapSectionLayer layer)
        {
            state.mapSectionsSC.SelectLayerCommit(layer);
        }
    }

    private void OnRemoveLayerClick(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is MapSectionLayer layer)
        {
            state.mapSectionsSC.RemoveLayerCommit(state.mapSectionsSC.selectedMapSection.layers.IndexOf(layer));
        }
    }

    private void OnAddMapSectionClick(object sender, RoutedEventArgs e)
    {
        state.mapSectionsSC.AddMapSectionCommit();
    }

    private void OnImportMapSectionClick(object sender, RoutedEventArgs e)
    {
        state.mapSectionsSC.ImportMapSection();
    }

    private void OnAddScratchSectionClick(object sender, RoutedEventArgs e)
    {
        state.scratchSectionsSC.AddMapSectionCommit();
    }

    private void OnImportScratchSectionClick(object sender, RoutedEventArgs e)
    {
        state.scratchSectionsSC.ImportMapSection();
    }

    private void OnAddInstanceClick(object sender, RoutedEventArgs e)
    {
        state.mapSectionsSC.ToggleAddInstanceMode();
    }

    private void OnAddZoneClick(object sender, RoutedEventArgs e)
    {
        state.mapSectionsSC.AddZoneFromSelectionCommit();
    }
    
    private void OnAddTileAnimationClick(object sender, RoutedEventArgs e)
    {
        state.tileAnimationSC.AddTileAnimationCommit();
    }

    private void OnRemoveTileAnimationClick(object sender, RoutedEventArgs e)
    {
        state.tileAnimationSC.RemoveTileAnimationCommit();
    }

    private void OnAddTileClumpClick(object sender, RoutedEventArgs e)
    {
        state.tileClumpSC.AddTileClumpCommit();
    }

    private void OnRemoveTileClumpClick(object sender, RoutedEventArgs e)
    {
        state.tileClumpSC.RemoveTileClumpCommit();
    }

    private void OnSortTileClumpClick(object sender, RoutedEventArgs e)
    {
        state.tileClumpSC.SortTileClumpCommit();
    }

    private void OnAddTcSubsectionClick(object sender, RoutedEventArgs e)
    {
        state.tileClumpSC.AddTileClumpSubsectionCommit();
    }

    private void OnRemoveTcSubsectionClick(object sender, RoutedEventArgs e)
    {
        state.tileClumpSC.RemoveTileClumpSubsectionCommit();
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
        Prompt.ShowMessage("Your map editor version is: " + Helpers.GetVersion());
    }

    private void OnResizeMapSectionClick(object sender, RoutedEventArgs e)
    {         
        ResizeMapDialog dialog = new ResizeMapDialog("Resize map section", state.mapSectionsSC);
        if (dialog.ShowDialog() == true)
        {
            state.mapSectionsSC.ResizeMapSectionCommit(dialog.rows, dialog.cols, dialog.fromTopLeft);
        }
    }

    private void OnResizeScratchSectionClick(object sender, RoutedEventArgs e)
    {
        ResizeMapDialog dialog = new ResizeMapDialog("Resize scratch section", state.scratchSectionsSC);
        if (dialog.ShowDialog() == true)
        {
            state.scratchSectionsSC.ResizeMapSectionCommit(dialog.rows, dialog.cols, dialog.fromTopLeft);
        }
    }

    private void OnDebugClick(object sender, RoutedEventArgs e)
    {
        Prompt.ShowMessage("Debug button done");
    }
}
