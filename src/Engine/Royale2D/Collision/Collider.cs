namespace Royale2D
{
    // As much as possible, put collider properties in this actual class instead of relying on Actor class type checks
    public class Collider
    {
        public IntShape shape;
        public string tags = "";
        public bool isMultiSprite;  // Set to true, for example, for player's main collider, since player has multiple sprites he changes between
        public bool isDamager;
        public bool isDamagable;
        public bool isWallCollider;
        // If adding a new property flag, you MUST add it to EqualTo as well

        public Collider(IntShape shape, string tags = "", bool isMultiSprite = false, bool isDamager = false, bool isDamagable = false, bool isWallCollider = false)
        {
            this.shape = shape;
            this.tags = tags;
            this.isMultiSprite = isMultiSprite;
            this.isDamager = isDamager;
            this.isDamagable = isDamagable;
            this.isWallCollider = isWallCollider;
        }

        public bool EqualTo(Collider other)
        {
            return shape.EqualTo(other.shape) &&
                tags == other.tags &&
                isMultiSprite == other.isMultiSprite &&
                isDamager == other.isDamager &&
                isDamagable == other.isDamagable &&
                isWallCollider == other.isWallCollider;
        }

        public string GetDebugString()
        {
            List<string> pieces = [];
            if (isMultiSprite) pieces.Add("isMultiSprite");
            if (isDamager) pieces.Add("isDamager");
            if (isDamagable) pieces.Add("isDamagable");
            if (isWallCollider) pieces.Add("isWallCollider");
            return string.Join(',', pieces);
        }

        public IntShape GetActorWorldShape(Actor actor, FdPoint? moveAmount = null)
        {
            FdPoint nonNullMoveAmount = moveAmount ?? FdPoint.Zero;
            FdPoint offset = actor.pos + nonNullMoveAmount;
            IntShape newShape = shape;

            if (!isMultiSprite)
            {
                if (actor.xDir == -1)
                {
                    newShape = newShape.FlipX();
                }
                offset += actor.spriteOffset.ToFdPoint();
            }

            if (actor.GetComponent<ZComponent>() is ZComponent zComponent && (!isWallCollider || zComponent.useZForWallCollisions))
            {
                offset.y -= zComponent.z;
            }

            return newShape.Clone(offset.x.intVal, offset.y.intVal);
        }

        public IntShape GetTileInstanceWorldShape(TileInstance tileInstance)
        {
            return shape.Clone(tileInstance.j * 8, tileInstance.i * 8);
        }

        public bool HasTag(string tag)
        {
            return tags.Contains(tag);
        }
    }
}
