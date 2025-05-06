using Editor;
using Shared;

namespace SpriteEditor;

public class POI : StateComponentWithModel<POIModel>, ISelectable
{
    #region model
    public string tags { get => TrGetD(model.tags); set => TrSet(value); }
    public int x { get => TrGetD(model.x); set => TrSet(value); }
    public int y { get => TrGetD(model.y); set => TrSet(value); }
    public POIModel ToModel() => new POIModel(tags, x, y);
    #endregion

    public bool selected { get => TrGet<bool>(); set => TrSet(value, [nameof(border)]); }
    public string border => selected ? "LightGreen" : "Transparent";

    public POI(EditorContext context, POIModel model) : base(context, model)
    {
    }

    public POI(EditorContext context, string tags, int x, int y) : base(context, new(tags, x, y))
    {
    }

    public void Move(int moveX, int moveY) 
    {
        x += moveX;
        y += moveY;
    }
    
    public MyRect GetRect()
    {
        return new MyRect(x - 2, y - 2, x + 2, y + 2);
    }
}
