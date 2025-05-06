using SFML.Graphics;
using Shared;

namespace Royale2D
{
    public class Sprite
    {
        // SHARED FIELDS
        public List<Frame> frames = new List<Frame>();
        public List<Hitbox> hitboxes = new List<Hitbox>();
        public int loopStartFrame = 0;
        public string alignment = Alignment.Center;
        public string wrapMode = WrapMode.Once;
        public string spritesheetName;
        // END SHARED FIELDS

        public Texture? texture;
        public string name = "";

        public Sprite(List<Frame> frames, string spritesheetName)
        {
            this.frames = frames;
            this.spritesheetName = spritesheetName;
        }

        public void Init(string name)
        {
            this.name = name;
            if (spritesheetName.IsSet())
            {
                texture = Assets.textures[Path.GetFileNameWithoutExtension(spritesheetName)];
            }
        }

        public int GetAnimDuration()
        {
            int duration = 0;
            foreach (Frame frame in frames)
            {
                duration += frame.duration;
            }
            return duration;
        }

        // Only use Render() on sprite directly if it doesn't matter that everyone shares the same instance for the sprite, otherwise use a local SpriteInstance
        public SpriteInstance? spriteInstance;
        public void Render(Drawer drawer, float x, float y, int frameIndex, ZIndex zIndex = default, float overrideXScale = 1, float overrideYScale = 1)
        {
            if (spriteInstance == null)
            {
                spriteInstance = new SpriteInstance(name);
            }
            spriteInstance.ChangeFrameIndex(frameIndex);
            spriteInstance.Render(drawer, x, y, zIndex, xScale: overrideXScale, yScale: overrideYScale);
        }

        public void Render(Drawer drawer, Point pos, int frameIndex, ZIndex zIndex = default)
        {
            Render(drawer, pos.x, pos.y, frameIndex, zIndex);
        }
    }
}
