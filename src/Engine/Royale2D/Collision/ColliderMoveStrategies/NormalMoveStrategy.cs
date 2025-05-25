namespace Royale2D
{
    public class NormalMoveStrategy : ColliderMoveStrategy
    {
        public NormalMoveStrategy(ColliderComponent colliderComponent) : base(colliderComponent)
        {
        }

        // REFACTOR consider renaming to "ApplyQueuedMoveAndCheckCollision"
        public override void ApplyMove(FdPoint moveAmount)
        {
            (List<TileCollision> tileCollisions, List<ActorCollision> actorCollisions) = MoveIncremental(moveAmount, false);

            colliderComponent.SortAndFilterActorCollisions(actorCollisions);
            foreach (ActorCollision actorCollision in actorCollisions)
            {
                actor.OnActorCollision(actorCollision);
            }
            foreach (TileCollision tileCollision in tileCollisions)
            {
                actor.OnTileCollision(tileCollision);
            }
        }
    }
}
