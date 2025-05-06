using Editor;
using Shared;

namespace MapEditor;

public record TileShapeToDraw(MyRect? rect, MyShape? shape);

public static class TileExtensions
{
    public static int? GetAnimationId(this Tile tile, State state)
    {
        return state.tileAnimationSC.GetTileAnimationId(tile);
    }

    public static TileShapeToDraw? GetHitboxShapeToDraw(this Tile tile, int i, int j, Tileset tileset)
    {
        TileHitboxMode hitboxMode = tile.hitboxMode;
        int TS = tileset.TS;

        if (hitboxMode == TileHitboxMode.FullTile)
        {
            return new TileShapeToDraw(new GridCoords(i, j).GetRect(TS), null);
        }

        var x = j * TS;
        var y = i * TS;
        var x2 = (j + 1) * TS;
        var y2 = (i + 1) * TS;
        var topLeftPt = new MyPoint(x, y);
        var topRightPt = new MyPoint(x2, y);
        var botLeftPt = new MyPoint(x, y2);
        var botRightPt = new MyPoint(x2, y2);
        var xMid = x + (TS / 2);
        var yMid = y + (TS / 2);

        if (hitboxMode == TileHitboxMode.DiagBotLeft)
        {
            return new TileShapeToDraw(null, new MyShape([topLeftPt, botRightPt, botLeftPt]));
        }
        else if (hitboxMode == TileHitboxMode.DiagBotRight)
        {
            return new TileShapeToDraw(null, new MyShape([botLeftPt, botRightPt, topRightPt]));
        }
        else if (hitboxMode == TileHitboxMode.DiagTopLeft)
        {
            return new TileShapeToDraw(null, new MyShape([topLeftPt, topRightPt, botLeftPt]));
        }
        else if (hitboxMode == TileHitboxMode.DiagTopRight)
        {
            return new TileShapeToDraw(null, new MyShape([topLeftPt, topRightPt, botRightPt]));
        }
        else if (hitboxMode == TileHitboxMode.BoxTopLeft)
        {
            return new TileShapeToDraw(new MyRect(x, y, xMid, yMid), null);
        }
        else if (hitboxMode == TileHitboxMode.BoxTopRight)
        {
            return new TileShapeToDraw(new MyRect(xMid, y, x2, yMid), null);
        }
        else if (hitboxMode == TileHitboxMode.BoxBotLeft)
        {
            return new TileShapeToDraw(new MyRect(x, yMid, xMid, y2), null);
        }
        else if (hitboxMode == TileHitboxMode.BoxBotRight)
        {
            return new TileShapeToDraw(new MyRect(xMid, yMid, x2, y2), null);
        }
        else if (hitboxMode == TileHitboxMode.BoxTop)
        {
            return new TileShapeToDraw(new MyRect(x, y, x2, yMid), null);
        }
        else if (hitboxMode == TileHitboxMode.BoxBot)
        {
            return new TileShapeToDraw(new MyRect(x, yMid, x2, y2), null);
        }
        else if (hitboxMode == TileHitboxMode.BoxLeft)
        {
            return new TileShapeToDraw(new MyRect(x, y, xMid, y2), null);
        }
        else if (hitboxMode == TileHitboxMode.BoxRight)
        {
            return new TileShapeToDraw(new MyRect(xMid, y, x2, y2), null);
        }
        else if (hitboxMode == TileHitboxMode.SmallDiagTopLeft)
        {
            return new TileShapeToDraw(null, new MyShape([topLeftPt, topLeftPt.AddXY(TS / 2, 0), topLeftPt.AddXY(0, TS / 2)]));
        }
        else if (hitboxMode == TileHitboxMode.SmallDiagTopRight)
        {
            return new TileShapeToDraw(null, new MyShape([topRightPt, topRightPt.AddXY(-TS / 2, 0), topRightPt.AddXY(0, TS / 2)]));
        }
        else if (hitboxMode == TileHitboxMode.SmallDiagBotLeft)
        {
            return new TileShapeToDraw(null, new MyShape([botLeftPt, botLeftPt.AddXY(TS / 2, 0), botLeftPt.AddXY(0, -TS / 2)]));
        }
        else if (hitboxMode == TileHitboxMode.SmallDiagBotRight)
        {
            return new TileShapeToDraw(null, new MyShape([botRightPt, botRightPt.AddXY(-TS / 2, 0), botRightPt.AddXY(0, -TS / 2)]));
        }
        else if (hitboxMode == TileHitboxMode.LargeDiagTopLeft)
        {
            return new TileShapeToDraw(null, new MyShape([topLeftPt, topRightPt, botRightPt.AddXY(0, -TS / 2), botRightPt.AddXY(-TS / 2, 0), botLeftPt]));
        }
        else if (hitboxMode == TileHitboxMode.LargeDiagTopRight)
        {
            return new TileShapeToDraw(null, new MyShape([topLeftPt, topRightPt, botRightPt, botLeftPt.AddXY(TS / 2, 0), botLeftPt.AddXY(0, -TS / 2)]));
        }
        else if (hitboxMode == TileHitboxMode.LargeDiagBotLeft)
        {
            return new TileShapeToDraw(null, new MyShape([topLeftPt, topRightPt.AddXY(-TS / 2, 0), topRightPt.AddXY(0, TS / 2), botRightPt, botLeftPt]));
        }
        else if (hitboxMode == TileHitboxMode.LargeDiagBotRight)
        {
            return new TileShapeToDraw(null, new MyShape([topLeftPt.AddXY(TS / 2, 0), topRightPt, botRightPt, botLeftPt, topLeftPt.AddXY(0, TS / 2)]));
        }
        else
        {
            return null;
        }
    }
}
