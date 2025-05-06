
namespace Shared;

public struct GridCoords
{
    public int i;
    public int j;

    public GridCoords(int i, int j)
    {
        this.i = i;
        this.j = j;
    }

    public MyRect GetRect(int TS)
    {
        return new MyRect(j * TS, i * TS, (j + 1) * TS, (i + 1) * TS);
    }

    public override string ToString()
    {
        return i.ToString() + "," + j.ToString();
    }

    public static string ToString(int i, int j)
    {
        return new GridCoords(i, j).ToString();
    }

    public static GridCoords Parse(string s)
    {
        return new GridCoords(int.Parse(s.Split(',')[0]), int.Parse(s.Split(',')[1]));
    }

    // i and j must be ushorts (0 to 65535)
    public static int GetHashCode(int i, int j)
    {
        return i << 16 | j;
    }

    public GridCoords AddIJ(int i, int j)
    {
        return new GridCoords(this.i + i, this.j + j);
    }

    public override int GetHashCode()
    {
        return GetHashCode(i, j);
    }
}