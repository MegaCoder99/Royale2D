using Shared;

namespace Editor;

public class GridRectSC : StateComponentWithModel<GridRect>
{
    #region model
    public int i1 { get => TrGetD(model.i1); set => TrSet(value); }
    public int j1 { get => TrGetD(model.j1); set => TrSet(value); }
    public int i2 { get => TrGetD(model.i2); set => TrSet(value); }
    public int j2 { get => TrGetD(model.j2); set => TrSet(value); }
    public GridRect ToModel() => new(i1, j1, i2, j2);
    #endregion

    public GridRectSC(EditorContext context, GridRect model) : base(context, model)
    {
    }

    public GridRectSC(EditorContext context, int i1, int j1, int i2, int j2) : 
        this(context, new(i1, j1, i2, j2))
    {
    }

    public static GridRectSC? TryNew(EditorContext context, GridRect? rect)
    {
        if (rect == null) return null;
        return new GridRectSC(context, rect.Value);
    }


    public static implicit operator GridRect?(GridRectSC? trGridRect)
    {
        if (trGridRect == null) return null;
        return trGridRect.ToModel();
    }

    public static implicit operator GridRect(GridRectSC trGridRect)
    {
        return trGridRect.ToModel();
    }

    public void IncIJ(int i, int j)
    {
        i1 += i;
        j1 += j;
        i2 += i;
        j2 += j;
    }
}
