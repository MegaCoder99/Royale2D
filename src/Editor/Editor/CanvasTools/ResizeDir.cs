using System.Windows.Input;

namespace Editor;

public class ResizeDirs
{
    public static ResizeDir TopLeft = new ResizeDir(Cursors.SizeNWSE, -1, -1);
    public static ResizeDir Top = new ResizeDir(Cursors.SizeNS, 0, -1);
    public static ResizeDir TopRight = new ResizeDir(Cursors.SizeNESW, 1, -1);
    public static ResizeDir Left = new ResizeDir(Cursors.SizeWE, -1, 0);
    public static ResizeDir Right = new ResizeDir(Cursors.SizeWE, 1, 0);
    public static ResizeDir BottomLeft = new ResizeDir(Cursors.SizeNESW, -1, 1);
    public static ResizeDir Bottom = new ResizeDir(Cursors.SizeNS, 0, 1);
    public static ResizeDir BottomRight = new ResizeDir(Cursors.SizeNWSE, 1, 1);
}

public class ResizeDir
{
    public Cursor cursor;
    public int xDir;
    public int yDir;
    public ResizeDir(Cursor cursor, int xDir, int yDir)
    {
        this.cursor = cursor;
        this.xDir = xDir;
        this.yDir = yDir;
    }
}
