namespace Editor;

public interface IEditorState
{
    public void SetDirty(DirtyFlag dirtyFlag);
    public void Redraw(RedrawData redrawData);
    public void OnUndoRedo();

    // Similarly to TrSetC callbacks, the code that handles the editor event needs to use the QueueOnPropertyChanged/QueueGenericAction helpers
    // to have such side effects undo'able. Also like TrSetC callbacks, it can change trackables without issue as it should be in a commit context.
    // Think of this as just a TrSetC callback that can apply an "action at a distance" to your root state level and elsewhere.
    // As with TrSetC, be wary of infinite recursion! Do not run code that changes the same property as the one triggering the event.
    public void EditorEventHandler(EditorEvent editorEvent, StateComponent firer);
}

