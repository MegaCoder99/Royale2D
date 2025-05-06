
namespace Royale2D
{
    // IMPORTANT: GridRects are designed to be INCLUSIVE for BOTH the top left and bot right grid coords.
    // That is to say, a grid rect of (0, 0) and (0, 0) is actually a grid rect of size 1 and is a rect representing just the top left cell.
    // A grid rect of (0, 0) and (1, 1) would represent a grid rect of size 2 and is a 2x2 rect
    public struct GridRect
    {
        public int i1;
        public int j1;
        public int i2;
        public int j2;

        public GridCoords topLeftGridCoords => new GridCoords(i1, j1);
        public GridCoords botRightGridCoords => new GridCoords(i2, j2);

        public GridRect(int i1, int j1, int i2, int j2)
        {
            this.i1 = i1;
            this.j1 = j1;
            this.i2 = i2;
            this.j2 = j2;
        }

        public Rect GetFloatRect()
        {
            return new Rect(j1 * 8, i1 * 8, (j2 + 1) * 8, (i2 + 1) * 8);
        }

        public IntRect GetIntRect()
        {
            return new IntRect(j1 * 8, i1 * 8, (j2 + 1) * 8, (i2 + 1) * 8);
        }

        public GridRect Clone(int offI, int offJ)
        {
            return new GridRect(i1 + offI, j1 + offJ, i2 + offI, j2 + offJ);
        }

        /*
        public string toString()
        {
            return (topLeftGridCoords.i).ToString() + "_" + (topLeftGridCoords.j).ToString() + "_" + (botRightGridCoords.i).ToString() + "_" + (botRightGridCoords.j).ToString();
        }
        
        public bool equals(GridRect other)
        {
            return topLeftGridCoords.equals(other.topLeftGridCoords) && botRightGridCoords.equals(other.botRightGridCoords);
        }
        */
    }
}
