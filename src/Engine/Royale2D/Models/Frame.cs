using SFML.Graphics;
using Shared;

namespace Royale2D
{
    public class Frame
    {
        public IntRect rect { get; set; }
        public int duration { get; set; }
        public IntPoint offset { get; set; }
        public List<Hitbox> hitboxes { get; set; }
        public List<Drawbox> drawboxes { get; set; }
        public List<POI> POIs { get; set; }
        public int zIndex { get; set; }
        public string tags { get; set; } = "";
        public string spritesheetName { get; set; } = "";
        public Texture? texture;

        public Frame(IntRect rect, int duration, IntPoint offset, string spritesheetName)
        {
            this.rect = rect;
            this.duration = duration;
            this.offset = offset;
            this.spritesheetName = spritesheetName;
            this.hitboxes = new List<Hitbox>();
            this.POIs = new List<POI>();
            this.drawboxes = new List<Drawbox>();
            if (spritesheetName.IsSet())
            {
                this.texture = Assets.textures[Path.GetFileNameWithoutExtension(spritesheetName)];
            }
        }
    }
}
