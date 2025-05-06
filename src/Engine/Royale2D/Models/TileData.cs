using SFML.Graphics;
using Shared;
using System.Text.Json.Serialization;

namespace Royale2D
{
    public class TileData
    {
        // SHARED FIELDS
        public int id;
        public TileHitboxMode hitboxMode;
        public string tags = "";
        public IntPoint imageTopLeftPos;
        public string imageFileName;
        
        // END SHARED FIELDS

        [JsonIgnore] public Collider? collider;
        [JsonIgnore] public TileAnimation? tileAnimation;
        [JsonIgnore] public List<string> tagsList => tags.Split(',').ToList();

        public Texture texture;

        public TileData(TileHitboxMode hitboxMode, string tags)
        {
            this.hitboxMode = hitboxMode;
            collider = GetCollider(hitboxMode);
            this.tags = tags ?? "";
        }

        public static Collider? GetCollider(TileHitboxMode hitboxMode)
        {
            int x1 = 0;
            int y1 = 0;
            int x2 = 8;
            int y2 = 8;
            if (hitboxMode == TileHitboxMode.FullTile)
            {
                IntRect colliderRect = new IntRect(x1, y1, x2, y2);
                return new Collider(colliderRect);
            }
            else if (hitboxMode == TileHitboxMode.DiagBotLeft)
            {
                IntIrt colliderIrt = new IntIrt(new IntPoint(x1, y2), 8, IrtDir.BottomLeft);
                return new Collider(colliderIrt);
            }
            else if (hitboxMode == TileHitboxMode.DiagBotRight)
            {
                IntIrt colliderIrt = new IntIrt(new IntPoint(x2, y2), 8, IrtDir.BottomRight);
                return new Collider(colliderIrt);
            }
            else if (hitboxMode == TileHitboxMode.DiagTopLeft)
            {
                IntIrt colliderIrt = new IntIrt(new IntPoint(x1, y1), 8, IrtDir.TopLeft);
                return new Collider(colliderIrt);
            }
            else if (hitboxMode == TileHitboxMode.DiagTopRight)
            {
                IntIrt colliderIrt = new IntIrt(new IntPoint(x2, y1), 8, IrtDir.TopRight);
                return new Collider(colliderIrt);
            }
            else if (hitboxMode == TileHitboxMode.BoxTop)
            {
                IntRect colliderRect = new IntRect(x1, y1, x1 + 8, y1 + 4);
                return new Collider(colliderRect);
            }
            else if (hitboxMode == TileHitboxMode.BoxBot)
            {
                IntRect colliderRect = new IntRect(x1, y1 + 4, x1 + 8, y1 + 8);
                return new Collider(colliderRect);
            }
            else if (hitboxMode == TileHitboxMode.BoxLeft)
            {
                IntRect colliderRect = new IntRect(x1, y1, x1 + 4, y1 + 8);
                return new Collider(colliderRect);
            }
            else if (hitboxMode == TileHitboxMode.BoxRight)
            {
                IntRect colliderRect = new IntRect(x1 + 4, y1, x1 + 8, y1 + 8);
                return new Collider(colliderRect);
            }
            else if (hitboxMode == TileHitboxMode.BoxTopLeft)
            {
                IntRect colliderRect = new IntRect(x1, y1, x1 + 4, y1 + 4);
                return new Collider(colliderRect);
            }
            else if (hitboxMode == TileHitboxMode.BoxTopRight)
            {
                IntRect colliderRect = new IntRect(x1 + 4, y1, x1 + 8, y1 + 4);
                return new Collider(colliderRect);
            }
            else if (hitboxMode == TileHitboxMode.BoxBotLeft)
            {
                IntRect colliderRect = new IntRect(x1, y1 + 4, x1 + 4, y1 + 8);
                return new Collider(colliderRect);
            }
            else if (hitboxMode == TileHitboxMode.BoxBotRight)
            {
                IntRect colliderRect = new IntRect(x1 + 4, y1 + 4, x1 + 8, y1 + 8);
                return new Collider(colliderRect);
            }

            return null;
        }

        public bool HasTag(string tag)
        {
            return tagsList.Contains(tag);
        }

        public bool HasLedgeTag()
        {
            return tagsList.Any(tag => tag.StartsWith("ledge"));
        }

        public bool LedgeMatchesMoveDir(FdPoint moveAmount)
        {
            if (tags.Contains(TileTag.LedgeLeft)) return moveAmount.x < 0;
            if (tags.Contains(TileTag.LedgeRight)) return moveAmount.x > 0;
            if (tags.Contains(TileTag.LedgeUp)) return moveAmount.y < 0;
            if (tags.Contains(TileTag.LedgeDown)) return moveAmount.y > 0;
            if (tags.Contains(TileTag.LedgeUpLeft)) return moveAmount.x < 0 || moveAmount.y < 0;
            if (tags.Contains(TileTag.LedgeUpRight)) return moveAmount.x > 0 || moveAmount.y < 0;
            if (tags.Contains(TileTag.LedgeDownLeft)) return moveAmount.y < 0 || moveAmount.y > 0;
            if (tags.Contains(TileTag.LedgeDownRight)) return moveAmount.y > 0 || moveAmount.y > 0;
            return false;
        }

        public bool HasAnyTag(List<string> tagsList)
        {
            return tagsList.Any(t => HasTag(t));
        }

        public bool CanLand()
        {
            return hitboxMode == TileHitboxMode.None && !HasTag("water");
        }

        public bool IsDiagonalHitbox()
        {
            return hitboxMode == TileHitboxMode.DiagBotLeft || hitboxMode == TileHitboxMode.DiagBotRight || hitboxMode == TileHitboxMode.DiagTopLeft || hitboxMode == TileHitboxMode.DiagTopRight;
        }
    }
}
