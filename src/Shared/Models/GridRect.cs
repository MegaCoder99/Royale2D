namespace Shared;

public struct GridRect
{
    public int i1 { get; set; }
    public int j1 { get; set; }
    public int i2 { get; set; }
    public int j2 { get; set; }

    public int rows => i2 - i1 + 1;
    public int cols => j2 - j1 + 1;

    public GridRect()
    {
    }

    public GridRect(int i1, int j1, int i2, int j2)
    {
        this.i1 = i1;
        this.j1 = j1;
        this.i2 = i2;
        this.j2 = j2;
    }

    public MyRect GetRect(int TS)
    {
        return new MyRect(j1 * TS, i1 * TS, (j2 + 1) * TS, (i2 + 1) * TS);
    }

    public static GridRect CreateFromWH(int i1, int j1, int w, int h)
    {
        return new GridRect(i1, j1, i1 + w, j1 + h);
    }
}