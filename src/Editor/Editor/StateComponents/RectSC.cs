using Shared;
namespace Editor;

public class RectSC : StateComponentWithModel<MyRect>
{
    #region model
    public int x1 { get => TrGetD(model.x1); set => TrSet(value); }
    public int y1 { get => TrGetD(model.y1); set => TrSet(value); }
    public int x2 { get => TrGetD(model.x2); set => TrSet(value); }
    public int y2 { get => TrGetD(model.y2); set => TrSet(value); }
    public MyRect ToModel() => new MyRect(x1, y1, x2, y2);
    #endregion

    public int w => x2 - x1;
    public int h => y2 - y1;
    public int area => w * h;

    public RectSC(EditorContext context, MyRect model) : base(context, model)
    {
    }

    public RectSC(EditorContext context, int x1, int y1, int x2, int y2) : this(context, new MyRect(x1, y1, x2, y2))
    {
    }

    public static RectSC? TryNew(EditorContext context, MyRect? rect)
    {
        if (rect == null) return null;
        return new RectSC(context, rect.Value);
    }

    public static implicit operator MyRect?(RectSC? trRect)
    {
        if (trRect == null) return null;
        return trRect.ToModel();
    }

    public static implicit operator MyRect(RectSC trRect)
    {
        return trRect.ToModel();
    }

    public void IncXY(int x, int y)
    {
        x1 += x;
        y1 += y;
        x2 += x;
        y2 += y;
    }

    public bool EqualTo(RectSC rect)
    {
        return x1 == rect.x1 && y1 == rect.y1 && x2 == rect.x2 && y2 == rect.y2;
    }
}
