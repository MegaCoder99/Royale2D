namespace Editor;

public class EditorContext
{
    private static EditorContext? main;

    // Reference sparingly, only on initial creation or if there is no other way to get context
    public static EditorContext GetContext()
    {
        if (main == null) main = new EditorContext();
        return main;
    }

    // Reference and cast sparingly. One ok use case is validating deeply nested state fields, if you need to check the higher level state's fields for that.
    // Do NOT abuse this and make any mutations on the state from this! This is for read-only purposes only and even then, avoid use and rely on events and commit context instead.
    public IEditorState? editorState { get; private set; }

    public UndoManager? undoManager { get; private set; }

    private CommitContext? currentCommitContext;

    // No one should be creating an editor context except the GetContext() singleton code. Hide constructor to prevent erroneous usage
    private EditorContext()
    {
    }

    // editorState and undoManager will be null before this is called. This is intentional. When we are initializing the whole state for the first time and all the massive amount of fields, we don't want
    // undo enqueuing, state dirty'ing, commit contexts, or anything to run at all. They should only run once initalization is complete and we're ready for user-made changes and state management duties to begin.
    public void Init(IEditorState editorState)
    {
        this.editorState = editorState;
        undoManager = new UndoManager(this, editorState);
    }

    public bool IsInitialized() => editorState != null && undoManager != null;

    public bool IsCommitContextSet() => currentCommitContext != null;

    // Call sparingly due to perf concerns. Tight loops with 100's/1000's of entries should not be calling this
    // Generally, you want to avoid firing this manually and have trackable properties specify it in TrSetE or TrSetAll.
    // Generally, this should only be called within a commit context. If not, the changes made by EditorEventHandler will not be undo'able.
    // A possible future improvement, if/when need arises, could be to make events classes, not enums, with arbitrary strongly-typed data in them
    public void FireEvent(EditorEvent editorEvent, StateComponent firer)
    {
        if (editorState == null) return;

        editorState.EditorEventHandler(editorEvent, firer);
    }

    public bool cuttingUndoBranch;
    private void ApplyCommit(CommitContext commitContext, Action? commitAction)
    {
        commitAction?.Invoke();
        if (editorState == null) return;

        foreach (DirtyFlag dirtyFlag in commitContext.dirtyFlags)
        {
            editorState.SetDirty(dirtyFlag);
        }

        cuttingUndoBranch = true;
        undoManager?.CutUndoBranch(commitContext.redrawData);
        cuttingUndoBranch = false;
        if (commitContext.redrawData != null)
        {
            //Console.WriteLine($"Redrawing flag {commitContext.redrawData.redrawFlag}, target {commitContext.redrawData.redrawTarget}");
            editorState.Redraw(commitContext.redrawData.Value);
        }
    }

    public void SetCommitContext(CommitContextType type, RedrawData? redrawData, HashSet<DirtyFlag> dirtyFlags)
    {
        if (cuttingUndoBranch == true)
        {
            if (type == CommitContextType.Code)
            {
                Helpers.AssertFailed("SetCommitContext called when cutting undo branch.");
            }
            if (type == CommitContextType.UI)
            {
                Helpers.AssertFailed("SetCommitContext called when cutting undo branch.");
            }
            return;
        }

        if (type == CommitContextType.Code && currentCommitContext?.type == CommitContextType.UI)
        {
            currentCommitContext = null;
            if (alreadyInUICommitContextBlock)
            {
                Helpers.AssertFailed("A code commit context cannot be triggered from a UI binding.");
            }
        }

        if (type == CommitContextType.Code && currentCommitContext?.type == CommitContextType.Code)
        {
            Helpers.AssertFailed("A code commit context cannot be triggered from within another one.");
        }

        if (currentCommitContext == null)
        {
            currentCommitContext = new CommitContext(type, redrawData, dirtyFlags);
        }
    }

    bool alreadyInUICommitContextBlock;
    public void ApplyUICommitContextIfExists(Action commitAction)
    {
        if (cuttingUndoBranch == true)
        {
            Helpers.AssertFailed("ApplyUICommitContextIfExists called when cutting undo branch.");
            return;
        }

        if (alreadyInUICommitContextBlock || currentCommitContext?.type != CommitContextType.UI)
        {
            commitAction.Invoke();
            return;
        }
        alreadyInUICommitContextBlock = true;
        try
        {
            int currentNodeCount = CurrentUndoNodeCount();
            commitAction.Invoke();
            if (CurrentUndoNodeCount() > currentNodeCount)
            {
                ApplyCommit(currentCommitContext, null);
            }
        }
        finally
        {
            currentCommitContext = null;
            alreadyInUICommitContextBlock = false;
        }
    }

    bool alreadyInCodeCommitContextBlock;

    public void ApplyCodeCommit(RedrawData? redrawData, HashSet<DirtyFlag> dirtyFlags, Func<ExtraCommitData?> commitAction)
    {
        if (cuttingUndoBranch == true)
        {
            Helpers.AssertFailed("ApplyCodeCommit called when cutting undo branch.");
            return;
        }

        if (alreadyInCodeCommitContextBlock)
        {
            commitAction.Invoke();
            return;
        }
        alreadyInCodeCommitContextBlock = true;
        SetCommitContext(CommitContextType.Code, redrawData, dirtyFlags);
        try
        {
            int currentNodeCount = CurrentUndoNodeCount();
            ExtraCommitData? extraCommitData = commitAction.Invoke();
            if (CurrentUndoNodeCount() > currentNodeCount)
            {
                if (extraCommitData?.overrideRedrawData != null)
                {
                    currentCommitContext!.redrawData = extraCommitData.overrideRedrawData;
                }
                if (extraCommitData?.overrideDirtyFlags != null)
                {
                    currentCommitContext!.dirtyFlags = extraCommitData.overrideDirtyFlags;
                }
                ApplyCommit(currentCommitContext!, null);
            }
        }
        finally
        {
            currentCommitContext = null;
            alreadyInCodeCommitContextBlock = false;
        }
    }

    public void ApplyCodeCommit(RedrawData? redrawData, HashSet<DirtyFlag> dirtyFlags, Action commitAction)
    {
        ApplyCodeCommit(redrawData, dirtyFlags, () => { commitAction.Invoke(); return null; });
    }

    public void ApplyCodeCommit(RedrawData? redrawData, DirtyFlag? dirtyFlag, Action commitAction)
    {
        ApplyCodeCommit(redrawData, dirtyFlag != null ? [dirtyFlag.Value] : [], commitAction);
    }

    private int CurrentUndoNodeCount()
    {
        return undoManager?.currentNodeCount ?? 0;
    }
}

public record ExtraCommitData(RedrawData? overrideRedrawData, HashSet<DirtyFlag> overrideDirtyFlags);