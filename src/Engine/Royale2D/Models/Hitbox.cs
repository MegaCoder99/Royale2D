

namespace Royale2D
{
    public struct Hitbox
    {
        public string tags = ""; // This can come from two places: the hitbox tags in editor, or if this is generated from child sprite at run-time, the child sprite tag
        public IntRect rect;
        
        public Hitbox()
        {
        }

        public Collider ToCollider()
        {
            var collider = new Collider(rect, tags);

            if (tags.Contains("sword") || tags.Contains("hammer"))
            {
                collider.isDamager = true;
            }

            return collider;
        }
    }
}
