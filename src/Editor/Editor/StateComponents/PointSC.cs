using Shared;
namespace Editor;

public class PointSC : StateComponentWithModel<MyPoint>
{
    public int x { get => TrGetD(model.x); set => TrSet(value); }
    public int y { get => TrGetD(model.y); set => TrSet(value); }
    public MyPoint ToModel() => new MyPoint(x, y);

    public PointSC(EditorContext context, MyPoint model) : base(context, model)
    {
    }

    public static implicit operator MyPoint(PointSC trPoint)
    {
        return trPoint.ToModel();
    }
}