namespace Editor;

public enum CommitContextType
{
    Code,
    UI
}

public class CommitContext
{
    public RedrawData? redrawData;
    public HashSet<DirtyFlag> dirtyFlags;
    public CommitContextType type;

    public CommitContext(CommitContextType type, RedrawData? redrawData, HashSet<DirtyFlag> dirtyFlags)
    {
        this.redrawData = redrawData;
        this.dirtyFlags = dirtyFlags;
        this.type = type;
    }
}
