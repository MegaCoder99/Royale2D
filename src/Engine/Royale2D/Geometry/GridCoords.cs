namespace Royale2D
{
    public struct GridCoords
    {
        public int i;
        public int j;

        public GridCoords(int i, int j)
        {
            this.i = i;
            this.j = j;
        }

        public Point ToFloatPoint()
        {
            return new Point((j * 8) + 4, (i * 8) + 4);
        }

        public FdPoint ToFdPoint()
        {
            return new FdPoint((j * 8) + 4, (i * 8) + 4);
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

        public override int GetHashCode()
        {
            return Helpers.GetIJHashCode(i, j);
        }

        public override bool Equals(object? obj)
        {
            if (obj is GridCoords other)
            {
                return i == other.i && j == other.j;
            }

            return false;
        }
    }
}
