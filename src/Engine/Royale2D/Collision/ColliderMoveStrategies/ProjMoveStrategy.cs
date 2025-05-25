namespace Royale2D
{
    // Despite the name this is not actually just for projectiles but anything with projectile-like movement (field items, pickups, etc)
    // This can include things with gravity (thrown bombs, liftables, etc) and things without gravity (arrows, boomerangs, etc)
    // and handles common behavior like flying off ledges without colliding into tiles thereafter, bouncing off walls, etc.
    public class ProjMoveStrategy : ColliderMoveStrategy
    {
        // A special flag that should only be set to true on demand when you want to signify this collider to stop registering any tile collisions
        // until it ends up in a position where it's not colliding with any tiles
        bool hitWallLastFrame;
        int hitDist;
        bool bouncedWall;

        public ProjMoveStrategy(ColliderComponent colliderComponent) : base(colliderComponent)
        {
        }

        public override void ApplyMove(FdPoint moveAmount)
        {
            (List<TileCollision> tileCollisions, List<ActorCollision> actorCollisions) = MoveIncremental(moveAmount, false);

            foreach (TileCollision tileCollision in tileCollisions)
            {
                actor.OnTileCollision(tileCollision);
            }

            if (actorCollisions.Count > 0)
            {
                colliderComponent.SortAndFilterActorCollisions(actorCollisions);
                foreach (ActorCollision actorCollision in actorCollisions)
                {
                    actor.OnActorCollision(actorCollision);
                }
            }
        }

        public void CheckHitWallLastFrame()
        {
            hitWallLastFrame = colliderComponent.GetMainTileCollisions(FdPoint.Zero).Any(c => !c.other.tileInstance.IsLedge());
        }

        public override List<TileCollision> GetIncrementalTileCollisions(FdPoint moveAmount)
        {
            List<TileCollision> tileCollisions = colliderComponent.GetMainTileCollisions(moveAmount);

            // Going off a ledge: ignore all collisions this frame and set hitWallLastFrame=true to simulate the collider flying off the ledge
            bool goingOffLedge = tileCollisions.RemoveAll(c => c.other.tileInstance.tileData.LedgeMatchesMoveDir(moveAmount)) > 0;
            if (goingOffLedge)
            {
                hitWallLastFrame = true;
            }

            VelComponent? velComponent = actor.GetComponent<VelComponent>();
            ZComponent? zComponent = actor.GetComponent<ZComponent>();

            // Bouncing off walls behavior for things with both z- and vel-components
            if (!hitWallLastFrame && !bouncedWall && tileCollisions.Count > 0 && velComponent != null && velComponent.vel.IsNonZero() && zComponent != null && zComponent.useGravity)
            {
                hitDist++;  // This will be incremented once per pixel of movement
                int hitDistBeforeBounce = 6;
                if (velComponent.vel.x != 0 && velComponent.vel.y == 0) hitDistBeforeBounce = 8;
                if (velComponent.vel.x == 0 && velComponent.vel.y > 0) hitDistBeforeBounce = 2;
                if (velComponent.vel.x == 0 && velComponent.vel.y < 0) hitDistBeforeBounce = 16;

                if (hitDist > hitDistBeforeBounce)
                {
                    hitWallLastFrame = true;
                    BounceBack();
                    // We immediately return results here because we need to return to the callers of this function that we in fact hit something to get it to stop this very frame.
                    // Otherwise the position would increment an extra frame resulting in some inaccuracy in the "hitDistBeforeBounce" system above
                    return tileCollisions;
                }
                else
                {
                    return [];
                }
            }

            // For as long as hitWallLastFrame=true, collisions are ignored UNTIL we are no longer colliding with any tile, this enforces that UNTIL
            if (tileCollisions.Count == 0 && !goingOffLedge) hitWallLastFrame = false;

            // And this ignores any collisions if hitWallLastFrame is still true
            if (hitWallLastFrame) return [];

            return tileCollisions;
        }

        public void BounceBack()
        {
            if (bouncedWall) return;
            bouncedWall = true;
            if (actor.GetComponent<VelComponent>() is VelComponent velComponent)
            {
                velComponent.vel *= Fd.New(0, -50);
            }
        }
    }
}
