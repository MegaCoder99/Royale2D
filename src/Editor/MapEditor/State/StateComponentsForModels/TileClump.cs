using Editor;
using Shared;

namespace MapEditor;

public class TileClump : StateComponentWithModel<TileClumpModel>, IComparable<TileClump>
{
    #region model
    public string name { get => TrGetD(model.name); set => TrSetV(value, ValidateStringIsSet()); }
    public int[,] tileIds { get; init; }
    public string transformTileClumpNameCsv { get => TrGetD(model.transformTileClumpNameCsv); set => TrSetV(value, ValidateTransformTileClumpNameCsv); }
    public string tags { get => TrGetD(model.tags); set => TrSet(value); }
    public string properties { get => TrGetD(model.properties); set => TrSet(value); }
    public TrackableList<TileClumpSubsection> subsections { get => TrListGet(model.subsections.SelectList(s => new TileClumpSubsection(context, s))); init => TrListSet(value); }

    public TileClumpModel ToModel()
    {
        return new TileClumpModel(name, tileIds, transformTileClumpNameCsv, tags, properties, subsections.SelectList(s => s.ToModel()));
    }
    #endregion

    public TileClumpSubsection? selectedSubsection { get => TrGet<TileClumpSubsection>(); set => TrSet(value); }
    public int rows => tileIds.GetLength(0);
    public int cols => tileIds.GetLength(1);

    public TileClump(EditorContext context, TileClumpModel model) : base(context, model)
    {
        tileIds = model.tileIds;
    }

    public TileClump(EditorContext context, string name, int[,] tileIds) : 
        this(context, new(name, tileIds, "", "", "", []))
    {
    }

    public bool ValidateTransformTileClumpNameCsv(string newValue)
    {
        string[] pieces = newValue.Split(',');
        foreach (string piece in pieces)
        {
            if (piece.Unset()) continue;
            if (context.editorState is State state)
            {
                if (state.tileClumpSC.tileClumps.FirstOrDefault(tc => tc.name == piece) == null)
                {
                    Prompt.ShowError($"Invalid transform tile clump name csv: tile clump '{piece}' not found.", "Validation Error");
                    return false;
                }
                else if (piece == name)
                {
                    Prompt.ShowError($"Invalid transform tile clump name csv: tile clump '{piece}' cannot be the same as this tile clump's name.", "Validation Error");
                    return false;
                }
            }
        }
        return true;
    }

    // REFACTOR could we make all the functions below use this, to DRY things?
    public bool CheckIfClumpMatches(Func<int, int, int?> getOtherTileIdFunc, int otherRowCount, int otherColCount, int i, int j)
    {
        for (int k = 0; k < rows; k++)
        {
            for (int l = 0; l < cols; l++)
            {
                if (i + k >= otherRowCount || j + l >= otherColCount)
                {
                    return false;
                }
                if (tileIds[k, l] != getOtherTileIdFunc(i + k, j + l))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public bool CheckIfClumpMatches(int?[,] otherTileIds, int i, int j)
    {
        for (int k = 0; k < rows; k++)
        {
            for (int l = 0; l < cols; l++)
            {
                if (i + k >= otherTileIds.GetLength(0) || j + l >= otherTileIds.GetLength(1))
                {
                    return false;
                }
                if (tileIds[k, l] != otherTileIds[i + k, j + l])
                {
                    return false;
                }
            }
        }
        return true;
    }

    public bool CheckIfClumpMatches(int[,] otherTileIds, int i, int j)
    {
        for (int k = 0; k < rows; k++)
        {
            for (int l = 0; l < cols; l++)
            {
                if (i + k >= otherTileIds.GetLength(0) || j + l >= otherTileIds.GetLength(1))
                {
                    return false;
                }
                if (tileIds[k, l] != otherTileIds[i + k, j + l])
                {
                    return false;
                }
            }
        }
        return true;
    }

    public int CompareTo(TileClump? other)
    {
        return name.CompareToNatural(other?.name);
    }
}

public class TileClumpSubsection : StateComponentWithModel<TileClumpSubsectionModel>
{
    #region model
    public string name { get => TrGetD(model.name); set => TrSetAll(value, [nameof(displayName)], validationCallback: ValidateStringIsSet()); }
    public List<GridCoords> cells { get; init; }

    public TileClumpSubsectionModel ToModel()
    {
        return new TileClumpSubsectionModel(name, cells);
    }
    #endregion

    public string displayName => name + " - " + (cells.Count > 0 ? string.Join(", ", cells.Select(c => $"({c})")) : "");

    public TileClumpSubsection(EditorContext context, TileClumpSubsectionModel model) : base(context, model)
    {
        cells = model.cells;
    }

    public TileClumpSubsection(EditorContext context, string name, List<GridCoords> cells) : this(context, new(name, cells))
    {
    }
}