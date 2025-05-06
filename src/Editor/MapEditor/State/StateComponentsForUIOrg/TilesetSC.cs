using Editor;
using Shared;

namespace MapEditor;

public class TilesetSC : StateComponent
{
    public Tileset tileset;

    public bool isDirty { get => TrGet<bool>(); set => TrSet(value); }

    public TilesetSC(EditorContext context, Dictionary<int, Tile> idToTile) : base(context)
    {
        tileset = new Tileset(idToTile.Values.ToList());
    }

    public void Save(MapWorkspace workspace, bool forceSave)
    {
        if (isDirty || forceSave)
        {
            workspace.SaveTileset(tileset.idToTile);
            isDirty = false;
        }
    }

    public Tile AddNewTile(string tileHash)
    {
        return tileset.AddNewTile(tileHash, (undoAction, redoAction) =>
        {
            undoManager?.AddUndoNode(new UndoNode(undoAction, redoAction, null));
            isDirty = true;
        });
    }

    // Call this vs one on tileset if you want it undo'able, but currently places calling these are power scripts that may not need it
    public void RemoveTile(int tileId)
    {
        tileset.RemoveTile(tileId, (undoAction, redoAction) =>
        {
            undoManager?.AddUndoNode(new UndoNode(undoAction, redoAction, null));
            isDirty = true;
        });
    }

    public bool ChangeTile(List<Tile> newTiles)
    {
        return tileset.ChangeTile(newTiles, (undoAction, redoAction) =>
        {
            context.FireEvent(EditorEvent.TileDataChange, this);
            undoManager?.AddUndoNode(new UndoNode(undoAction, redoAction, null));
            isDirty = true;
        });
    }
}
