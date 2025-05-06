namespace SpriteEditor;

public interface ISelectable
{
    public bool selected { get; set; }
    public string border { get; }
    public void Move(int moveX, int moveY);

    /*
    // Due to C#'s lack of multiple inheritance, ISelectable has to be an interface and we have to copy paste these fields around in each implementer
    [JsonIgnore] public bool selected { get => TrGet<bool>(); set => TrSet(value, [nameof(border)]); }
    [JsonIgnore] public string border => selected ? "LightGreen" : "Transparent";
    */
}
