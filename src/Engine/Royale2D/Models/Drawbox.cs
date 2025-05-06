using SFML.Graphics;

namespace Royale2D
{
    public class Drawbox
    {
        public Point pos;
        public string spritesheetName;
        public IntRect rect;
        public string tags;
        public int zIndex;

        public Texture texture;

        public Drawbox(Point pos, string spritesheetName, IntRect rect, string tags)
        {
            this.pos = pos;
            this.spritesheetName = spritesheetName;
            this.rect = rect;
            this.tags = tags;
            texture = Assets.textures[Path.GetFileNameWithoutExtension(spritesheetName)];
        }
    }
}
