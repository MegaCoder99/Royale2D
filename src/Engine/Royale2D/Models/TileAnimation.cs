using SFML.Graphics;
using Shared;

namespace Royale2D
{
    public class TileAnimation
    {
        public int duration;
        public int frameIndex;
        public int frameNum;
        public List<TileData> tiles;

        public Texture texture => tiles[frameIndex].texture;

        public TileAnimation(int tileId, Dictionary<int, TileData> allTiles, TileAnimationModel model)
        {
            tiles = model.tileIds.SelectList(id => allTiles[id]);
            duration = model.duration ?? 10;
            frameIndex = tiles.FindIndex(tile => tile.id == tileId);
        }

        public void Update()
        {
            frameNum++;
            if (frameNum >= duration)
            {
                frameNum = 0;
                frameIndex++;
                if (frameIndex >= tiles.Count)
                {
                    frameIndex = 0;
                }
            }
        }

        public SFML.Graphics.IntRect GetTextureRect()
        {
            IntPoint topLeftPos = tiles[frameIndex].imageTopLeftPos;
            return new SFML.Graphics.IntRect(topLeftPos.x, topLeftPos.y, 8, 8);
        }
    }
}
