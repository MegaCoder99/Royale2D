using Editor;
using Shared;
using System.Drawing;

namespace MapEditor;

public class Instance : StateComponentWithModel<InstanceModel>
{
    #region model section
    public string name { get => TrGetD(model.name); set => TrSetAll(value, [nameof(listDisplayName)], validationCallback: ValidateStringIsSet()); }
    public string properties { get => TrGetD(model.properties); set => TrSet(value); }
    public PointSC pos { get => TrGetD(new PointSC(context, model.pos)); set => TrSet(value); }
    public string instanceType { get => TrGetD(model.instanceType); set => TrSet(value); }
    public int layerIndex { get => TrGetD(model.layerIndex); set => TrSet(value); }
    
    public virtual InstanceModel ToModel()
    {
        return new InstanceModel(name, properties, pos, instanceType, layerIndex, null);
    }
    #endregion

    public InstanceType instanceTypeObj => InstanceTypes.GetInstanceTypeObj(instanceType);
    public bool isEntrance => instanceType == InstanceTypes.Entrance.name;

    public string listDisplayName => $"{name} - {instanceType}";
    public string propertyHelpText => instanceTypeObj.propertyHelpText.IsSet() ? $"Properties format: {instanceTypeObj.propertyHelpText}" : "";

    // Don't expose this publicly because we need ability to create Entrance subclass based on entranceData being set
    protected Instance(EditorContext context, InstanceModel model) : base(context, model)
    {
    }

    public static Instance New(EditorContext context, string name, string properties, MyPoint pos, string instanceType, int layerIndex, EntranceData? entranceData)
    {
        if (entranceData != null)
        {
            return new Entrance(context, new(name, properties, pos, instanceType, layerIndex, entranceData), entranceData);
        }
        return new Instance(context, new(name, properties, pos, instanceType, layerIndex, null));
    }

    public static Instance New(EditorContext context, InstanceModel model)
    {
        if (model.entranceData != null)
        {
            return new Entrance(context, model, model.entranceData);
        }
        return new Instance(context, model);
    }

    public MyRect GetPositionalRect()
    {
        Drawer iconDrawer = instanceTypeObj.drawer;
        int hw = iconDrawer.width / 2;
        int hh = iconDrawer.height / 2;
        return new MyRect(pos.x - hw, pos.y - hh, pos.x + hw, pos.y + hh);
    }

    public virtual void Draw(Drawer drawer)
    {
        Drawer iconDrawer = instanceTypeObj.drawer;
        int hw = iconDrawer.width / 2;
        int hh = iconDrawer.height / 2;
        drawer.DrawImage(iconDrawer, pos.x - hw, pos.y - hh);
    }
}

public class Entrance : Instance
{
    #region model section
    public string direction { get => TrGetD(""); set => TrSet(value); }
    public string width { get => TrGetD(""); set => TrSetV(value, ValidateWidthHeight); }
    public string height { get => TrGetD(""); set => TrSetV(value, ValidateWidthHeight); }
    public string GetEntranceId() => name;

    public override InstanceModel ToModel()
    {
        return new InstanceModel(name, properties, pos, instanceType, layerIndex, new EntranceData(direction, width, height));
    }
    #endregion

    // entranceData is repeated to gain compile time safety to ensure not null
    public Entrance(EditorContext context, InstanceModel model, EntranceData entranceData) : base(context, model)
    {
        direction = entranceData.direction;
        width = entranceData.width;
        height = entranceData.height;
    }

    public override void Draw(Drawer drawer)
    {
        base.Draw(drawer);

        Drawer iconDrawer = instanceTypeObj.drawer;
        int hw = iconDrawer.width / 2;
        int hh = iconDrawer.height / 2;

        var dirStr = "";
        if (direction == EntranceData.Up) dirStr = "↑";
        else if (direction == EntranceData.Down) dirStr = "↓";
        else if (direction == EntranceData.Left) dirStr = "←";
        else if (direction == EntranceData.Right) dirStr = "→";

        drawer.DrawText(GetEntranceId() + " " + dirStr, pos.x - hw, pos.y - hh - 1, Color.Yellow, Color.Black, 12);
    }

    public bool ValidateWidthHeight(string value)
    {
        int parsedVal;
        if (!value.IsSet()) return true;
        if (!int.TryParse(value, out parsedVal))
        {
            Prompt.ShowError("Width and height must be integers.");
            return false;
        }
        if (parsedVal < 1)
        {
            Prompt.ShowError("Width and height must be >= 1.");
            return false;
        }
        return true;
    }
}