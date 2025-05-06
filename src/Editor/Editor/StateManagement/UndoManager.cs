namespace Editor;

public class UndoManager
{
    public const int MaxUndoStackSize = 1000;

    EditorContext context;
    IEditorState editorState;
    public UndoManager(EditorContext context, IEditorState editorState)
    {
        this.context = context;
        this.editorState = editorState;
    }

    private List<UndoNode> curUndoRedoNodes = [];
    private List<UndoNodeGroup> undoStack = [];
    private List<UndoNodeGroup> redoStack = [];

    public int currentNodeCount => curUndoRedoNodes.Count;
    public bool canUndo => undoStack.Count > 0;
    public bool canRedo => redoStack.Count > 0;

    public void Nuke()
    {
        undoStack.Clear();
        redoStack.Clear();
        editorState.OnUndoRedo();
    }

    // Usually you should avoid calling this function or AddUndoNode manually, and instead use helper functions in StateComponent or just rely on trackables which do all undo pushes for you
    // The exception is if you want to avoid trackables for more fine grained control/performance in massive data sets, as with Tileset's tile dicts or MapSectionLayer's tile id grid
    public void AddUndoNode(UndoNode undoNode)
    {
        if (context.cuttingUndoBranch == true)
        {
            string exMsg =
@"Cannot add an undo node while cutting an undo branch. Some possible causes:
-In an EditorEvent handler, you set a trackable instead of just doing OnPropertyChangeds.
-OnPropertyChanged somehow cleared out a field and is trying to set it again.";
            Helpers.AssertFailed(exMsg);
            return;
        }

        // This can be a bit confusing. But the propertyChangedAction will be invoked always, even if we "short out" in the return check below due to not being in a commit context.
        // This prevents every single caller from having to manually invoke it, so it always applies instantly AND adds an entry for it in the undo stack
        undoNode.propertyChangedAction?.Invoke();

        if (!context.IsCommitContextSet())
        {
            // Examples of where this could happen on purpose: undirty'ing the state, quick scripts, export process, etc.
            // Examples of where this could happen by accident: not setting a converter on a trackable, changing a trackable outside commit context when you wanted undo, etc.
            return;
        }

        curUndoRedoNodes.Add(undoNode);
    }

    public void AddUndoNode(Action? undoAction, Action? redoAction, Action? propertyChangedAction)
    {
        AddUndoNode(new UndoNode(undoAction, redoAction, propertyChangedAction));
    }

    public void CutUndoBranch(RedrawData? redrawData)
    {
        var undoNodeGroup = new UndoNodeGroup(redrawData);
        undoStack.Add(undoNodeGroup);

        if (undoStack.Count > MaxUndoStackSize) 
        {
            undoStack.RemoveAt(0); 
        }

        if (curUndoRedoNodes.Count == 0)
        {
            Helpers.AssertFailed("No undo nodes in commit. This is a sign you forgot to add an undo node to your commit.");
        }

        foreach (UndoNode node in curUndoRedoNodes)
        {
            undoNodeGroup.undoRedoNodes.Add(node);
        }

        curUndoRedoNodes.Clear();

        // It is a common behavior for applications with undo/redo systems to clear the redo stack when a new action is added to the undo stack. This is because the redo stack represents
        // actions that can only logically follow from the current state. If a new action alters the state, any redo operations based on the previous state would no longer be valid.
        redoStack.Clear();

        editorState.OnUndoRedo();
    }

    public void ApplyUndo()
    {
        if (undoStack.Count == 0) return;
        UndoNodeGroup last = undoStack.Last();
        undoStack.Remove(last);
        redoStack.Add(last);

        last.InvokeUndo(editorState);
        editorState.OnUndoRedo();
    }

    public void ApplyRedo()
    {
        if (redoStack.Count == 0) return;
        UndoNodeGroup last = redoStack.Last();
        redoStack.Remove(last);
        undoStack.Add(last);

        last.InvokeRedo(editorState);
        editorState.OnUndoRedo();
    }

    public void MergeLastNUndoGroups(int n)
    {
        if (n < 2) return;
        if (undoStack.Count < n) return;
        UndoNodeGroup mergedGroup = new UndoNodeGroup(null);
        for (int i = n - 1; i >= 0; i--)
        {
            UndoNodeGroup group = undoStack[undoStack.Count - 1 - i];
            foreach (UndoNode node in group.undoRedoNodes)
            {
                mergedGroup.undoRedoNodes.Add(node);
            }
            foreach (RedrawData redrawData in group.redrawDatas)
            {
                if (!mergedGroup.redrawDatas.Contains(redrawData))
                {
                    mergedGroup.redrawDatas.Add(redrawData);
                }
            }
        }
        for (int i = 0; i < n; i++)
        {
            undoStack.RemoveAt(undoStack.Count - 1);
        }
        undoStack.Add(mergedGroup);
    }
}

public class UndoNode
{
    // As with propertyChangedAction, these two actions below should not change editor/commit/undo context fields. If you use trackable, this will be enforced for you.
    public Action undoAction;
    public Action redoAction;

    // This could encompass a little more code than just OnPropertyChanged()'s (the main use case). It is stuff that will run on every undo and redo, designed for XAML re-renders.
    // You can also run some "side-effect" code that does NOT change a trackable or do anything else of the sort.
    // This is important: propertyChangedAction must NOT change any trackables, add any undo nodes, or call EditorContext/UndoManager mutation methods in any way.
    // Unfortunately this design is reliant on OnPropertyChanged calls not modifying other properties. In some cases WPF will cause that to happen, as with the documented issue
    // where listview selected item can be cleared out, so we just have to avoid writing code that runs into those cases when possible.
    // (Not end of world when it happens: worst that happens is a debug assert fails and the code doesn't work as expected)
    public Action propertyChangedAction;

    public UndoNode(Action? undoAction, Action? redoAction, Action? propertyChangedAction)
    {
        this.undoAction = undoAction ?? (() => { });
        this.redoAction = redoAction ?? (() => { });
        this.propertyChangedAction = propertyChangedAction ?? (() => { });
    }
}

public class UndoNodeGroup
{
    public List<UndoNode> undoRedoNodes = [];

    // This is a list for cases where we merge multiple undo groups into one
    public List<RedrawData> redrawDatas = [];

    public UndoNodeGroup(RedrawData? redrawData)
    {
        if (redrawData != null && !redrawDatas.Contains(redrawData.Value))
        {
            redrawDatas.Add(redrawData.Value);
        }
    }

    public void InvokeUndo(IEditorState editorState)
    {
        // When undo'ing the undo action nodes, we must go in reverse to respect temporal ordering, or else things like list changes will be undone in the wrong order
        for (int i = undoRedoNodes.Count - 1; i >= 0; i--)
        {
            undoRedoNodes[i].undoAction.Invoke();
            // We must invoke property changed actions alongside and right after the corresponding undo action or we could run into the
            // issue described by the "In Data Bindings for ListBox, ComboBox, etc..." gotcha in the readme docs. Same for InvokeRedo
            undoRedoNodes[i].propertyChangedAction.Invoke();
        }

        foreach (RedrawData redrawData in redrawDatas)
        {
            editorState.Redraw(redrawData);
        }
    }

    public void InvokeRedo(IEditorState editorState)
    {
        foreach (UndoNode node in undoRedoNodes)
        {
            node.redoAction.Invoke();
            node.propertyChangedAction.Invoke();
        }
        
        foreach (RedrawData redrawData in redrawDatas)
        {
            editorState.Redraw(redrawData);
        }
    }
}
