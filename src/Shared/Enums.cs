namespace Shared;

// Once tile data is being used, only append new ones, do not remove/reorder old ones
public enum TileHitboxMode
{
    None,
    FullTile,
    DiagTopLeft,
    DiagTopRight,
    DiagBotLeft,
    DiagBotRight,
    BoxTop,
    BoxBot,
    BoxLeft,
    BoxRight,
    BoxTopLeft,
    BoxTopRight,
    BoxBotLeft,
    BoxBotRight,
    SmallDiagTopLeft,
    SmallDiagTopRight,
    SmallDiagBotLeft,
    SmallDiagBotRight,
    LargeDiagTopLeft,
    LargeDiagTopRight,
    LargeDiagBotLeft,
    LargeDiagBotRight,
    Custom, // Currently not implemented
}

public class WrapMode
{
    public const string Once = "once";
    public const string Loop = "loop";
}

public class Alignment
{
    public const string TopLeft = "topleft";
    public const string TopMid = "topmid";
    public const string TopRight = "topright";
    public const string MidLeft = "midleft";
    public const string Center = "center";
    public const string MidRight = "midright";
    public const string BotLeft = "botleft";
    public const string BotMid = "botmid";
    public const string BotRight = "botright";

    public static (int x, int y) GetAlignmentOrigin(string alignment, int w, int h)
    {
        switch (alignment)
        {
            case TopLeft: return (0, 0);
            case TopMid: return (w / 2, 0);
            case TopRight: return (w, 0);
            case MidLeft: return (0, h / 2);
            case Center: return (w / 2, h / 2);
            case MidRight: return (w, h / 2);
            case BotLeft: return (0, h);
            case BotMid: return (w / 2, h);
            case BotRight: return (w, h);
            default: throw new ArgumentException("Invalid alignment value");
        }
    }

    public static (float x, float y) GetAlignmentOriginFloat(string alignment, float w, float h)
    {
        switch (alignment)
        {
            case TopLeft: return (0, 0);
            case TopMid: return (MyMath.Floor(w / 2), 0);
            case TopRight: return (w, 0);
            case MidLeft: return (0, MyMath.Floor(h / 2));
            case Center: return (MyMath.Floor(w / 2), MyMath.Floor(h / 2));
            case MidRight: return (w, MyMath.Floor(h / 2));
            case BotLeft: return (0, h);
            case BotMid: return (MyMath.Floor(w / 2), h);
            case BotRight: return (w, h);
            default: throw new ArgumentException("Invalid alignment value");
        }
    }
}