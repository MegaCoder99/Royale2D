using Editor;
using Shared;
using System.Windows.Input;

namespace SpriteEditor;

// This partial class contains just the field definitions. For now this works and is organized differently from map editor because it's much simpler.
// If and when complexity grows, can re-org files in a different structure with more separate state components
public partial class State
{
    public TrackableList<Sprite> sprites { get => TrListGet<Sprite>(); init => TrListSet(value); }
    public Sprite selectedSprite { get => TrGet<Sprite>(); set => TrSet(value); }
    public string spriteFilterText { get => TrGetD(""); set => TrSet(value); }

    public List<Spritesheet> spritesheets { get; set; }
    public TrackableList<string> spritesheetNames { get => TrListGet<string>(); init => TrListSet(value); }
    public Spritesheet selectedSpritesheet => selectedSprite.GetSpritesheet(this);
    
    public List<string> alignments => new([Alignment.TopLeft, Alignment.TopMid, Alignment.TopRight, Alignment.MidLeft, Alignment.Center, Alignment.MidRight, Alignment.BotLeft, Alignment.BotMid, Alignment.BotRight]);
    public List<string> wrapModes => new([WrapMode.Loop, WrapMode.Once]);
    
    public Frame? selectedFrame => selectedSprite.selectedFrame;
    public Frame pendingFrame { get => TrGet<Frame>(); set => TrSet(value, [nameof(canAddPendingFrame), nameof(canReplacePendingFrame)]); }

    public ISelectable? selection { get => TrGet<ISelectable?>(); set => TrSetC(value, OnSelectionChange); }
    public void OnSelectionChange(ISelectable? oldSelection, ISelectable? newSelection)
    {
        if (oldSelection != null) oldSelection.selected = false;
        if (newSelection != null) newSelection.selected = true;
    }

    public Ghost? ghost { get => TrGet<Ghost?>(); set => TrSet(value); }
    public bool hideGizmos { get => TrGet<bool>(); set => TrSet(value); }
    public string boxTagFilter { get => TrGetD(""); set => TrSet(value); }
    public int bulkDuration { get => TrGet<int>(); set => TrSet(value); }
    
    public string workspaceLabel => "Workspace: " + Config.main.workspacePath;
    public List<string> recentWorkspaces => Config.main.recentWorkspaces;

    public bool canSave => selectedSprite?.isDirty ?? false;
    public bool canSaveAll => sprites.Any(s => s.isDirty);
    public bool canUndo => undoManager?.canUndo == true;
    public bool canRedo => undoManager?.canRedo == true;
    public bool canAddPendingFrame => pendingFrame != null;
    public bool canReplacePendingFrame => pendingFrame != null;
    public bool canRecomputeSelectedFrame => selectedFrame != null;

    public void OnUndoRedo()
    {
        OnPropertyChanged(nameof(canSave));
        OnPropertyChanged(nameof(canSaveAll));
        OnPropertyChanged(nameof(canUndo));
        OnPropertyChanged(nameof(canRedo));
    }

    private bool _addPOIMode;
    public bool addPOIMode
    {
        get => _addPOIMode;
        set
        {
            if (value == true) Mouse.OverrideCursor = Cursors.Pen;
            else Mouse.OverrideCursor = Cursors.Arrow;
            _addPOIMode = value;
        }
    }
}
