namespace Royale2D
{
    public enum ParentPosition
    {
        Center,
        FirstPOI
    }

    public class ChildComponent : Component
    {
        public Actor parent;
        public ParentPosition parentPositionType;
        public string childSpriteTag;   // optional parent child sprite tag to match z-index of

        public ChildComponent(Actor actor, Actor parent, ParentPosition parentPositionType, string childSpriteTag = "") : base(actor)
        {
            this.parent = parent;
            this.parentPositionType = parentPositionType;
            this.childSpriteTag = childSpriteTag;
        }
    }
}
