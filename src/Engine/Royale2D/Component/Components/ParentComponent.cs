namespace Royale2D
{
    public class ParentComponent : Component
    {
        // You may ask why this isn't a list of ChildComponent instead. (Same for ChildComponent.parent not being ParentComponent).
        // It could be, and that would in fact give more compile-time safety. i.e. no need for child.GetComponent<ChildComponent>()?
        // However it makes the code MUCH more complex/verbose and difficult to read and understand. IMO the compile-time safety ain't worth it here
        public List<Actor> children = [];

        public ParentComponent(Actor actor) : base(actor)
        {
        }

        public override void PostUpdate()
        {
            base.PostUpdate();
            foreach (Actor child in children.ToList())
            {
                if (child.isDestroyed)
                {
                    children.Remove(child);
                }
                else
                {
                    child.pos = GetChildPosition(child);
                    child.layerIndex = actor.layerIndex;
                }
            }
        }

        public void AddChild(Actor child, ParentPosition parentPosition, string childSpriteTag = "")
        {
            if (children.Contains(child))
            {
                throw new Exception("Can't add same child twice!");
            }
            child.ResetComponent(new ChildComponent(child, actor, parentPosition, childSpriteTag));
            children.Add(child);
        }

        public void RemoveChild(Actor child, bool destroy)
        {
            children.Remove(child);
            child.RemoveComponent<ChildComponent>();
            if (destroy) child.DestroySelf();
        }

        public FdPoint GetChildPosition(Actor child)
        {
            if (child.GetComponent<ChildComponent>()?.parentPositionType == ParentPosition.Center)
            {
                return actor.GetRenderPos();
            }
            else if (child.GetComponent<ChildComponent>()?.parentPositionType == ParentPosition.FirstPOI)
            {
                return actor.GetFirstPOI(useRenderOffset: true);
            }
            return actor.GetRenderPos();
        }

        public override void OnDestroy()
        {
            foreach (Actor child in children.ToList())
            {
                child.DestroySelf();
                children.Remove(child);
            }
        }
    }
}
