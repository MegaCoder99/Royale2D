namespace Royale2D
{
    public class ActorColliderInstance
    {
        public Collider collider;
        public ColliderComponent colliderComponent;
        public FdPoint moveAmount;
        public Actor actor => colliderComponent.actor;
        public string debugString;

        public ActorColliderInstance(Collider collider, ColliderComponent colliderComponent, FdPoint? moveAmount = null)
        {
            this.collider = collider;
            this.colliderComponent = colliderComponent;
            this.moveAmount = moveAmount ?? FdPoint.Zero;

            debugString = collider.tags;
            if (debugString == "") debugString = actor.spriteName;
            if (collider.GetDebugString() != "") debugString += "(" + collider.GetDebugString() + ")";
        }

        public IntShape GetWorldShape()
        {
            return collider.GetActorWorldShape(actor, moveAmount);
        }

        public ActorCollision? CheckActorCollision(ActorColliderInstance other)
        {
            if (colliderComponent.disabled || other.colliderComponent.disabled) return null;
            
            // Actors on different layers can't collide...
            if (actor.layerIndex != other.actor.layerIndex)
            {
                // unless one of them is on stairs, (to make sword swing collisions from above/below stairs more realistic)
                if (!colliderComponent.isOnStairs && !other.colliderComponent.isOnStairs)
                {
                    return null;
                }
            }

            IntShape myShape = GetWorldShape();
            IntShape theirShape = other.GetWorldShape();

            if (MySpatial.ShapesIntersect(myShape, theirShape))
            {
                return new ActorCollision(this, other);
            }

            return null;
        }

        public TileCollision? CheckTileCollision(TileColliderInstance other)
        {
            if (colliderComponent.disabled == true) return null;
            if (colliderComponent.tileClumpTagsToIgnore.Length > 0 && other.tileInstance.HasTileClumpTag(colliderComponent.tileClumpTagsToIgnore))
            {
                return null;
            }

            IntShape myShape = GetWorldShape();
            IntShape theirShape = other.GetWorldShape();

            if (MySpatial.ShapesIntersect(myShape, theirShape))
            {
                return new TileCollision(this, other);
            }

            return null;
        }
    }
}