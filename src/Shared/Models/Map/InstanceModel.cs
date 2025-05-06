namespace Shared;

public record InstanceModel(
    string name,
    string properties,
    MyPoint pos,
    string instanceType,
    int layerIndex,
    EntranceData? entranceData
);

// At the model level, we compose EntranceData instead of having a separate Entrance model that inherits from Instance to avoid complexities
// with inheritance in System.Text.Json serialization. The Instance state component will use inheritance when mapping from the model.
public record EntranceData(
    string direction,
    string width,
    string height)
{
    public const string Up = "up";
    public const string Down = "down";
    public const string Left = "left";
    public const string Right = "right";
    public const string Fall = "fall";
    public const string Land = "land";
    public static readonly List<string> directions = [ Up, Down, Left, Right, Fall, Land ];
}