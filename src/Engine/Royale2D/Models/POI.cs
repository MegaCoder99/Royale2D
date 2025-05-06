namespace Royale2D
{
    public class POI
    {
        public string tags { get; set; }
        public int x { get; set; }
        public int y { get; set; }

        public POI(string tags, int x, int y) 
        {
            this.tags = tags;
            this.x = x;
            this.y = y;
        }

        public IntRect getRect
        {
            get
            {
                return new IntRect(x - 2, y - 2, x + 2, y + 2);
            }
        }
    }
}
