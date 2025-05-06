namespace Royale2D
{
    public abstract class ColliderMoveStrategy
    {
        public ColliderComponent colliderComponent;

        public Actor actor => colliderComponent.actor;

        public ColliderMoveStrategy(ColliderComponent colliderComponent)
        {
            this.colliderComponent = colliderComponent;
        }

        public virtual void ApplyMove(FdPoint moveAmount)
        {
        }

        public virtual List<TileCollision> GetIncrementalTileCollisions(FdPoint moveAmount)
        {
            return colliderComponent.GetTileCollisions(moveAmount, colliderComponent.GetWallColliders());
        }

        // COLLISION double check negative x/y values actually work
        // Moves the actor one pixel at a time, preventing "teleportation past collisions" issues, and allowing us to detect order of hits (like shields first)
        public (List<TileCollision> tileCollisions, List<ActorCollision> actorCollisions) MoveIncremental(FdPoint moveAmount, bool allowEscapingWall)
        {
            List<TileCollision> tileCollisions = [];
            List<ActorCollision> actorCollisions = [];

            if (moveAmount == FdPoint.Zero)
            {
                actorCollisions = colliderComponent.GetAllActorCollisions(FdPoint.Zero);
                return (tileCollisions, actorCollisions);
            }

            List<TileCollision> beforeMoveTileCollisions = [];
            List<ActorCollision> beforeMoveActorCollisions = [];
            if (allowEscapingWall)
            {
                beforeMoveTileCollisions = GetIncrementalTileCollisions(FdPoint.Zero);
                beforeMoveActorCollisions = colliderComponent.GetAllActorCollisions(FdPoint.Zero);
            }

            List<FdPoint> incMoveAmounts = GetIncMoveAmounts(moveAmount);
            foreach (FdPoint incMoveAmount in incMoveAmounts)
            {
                tileCollisions = GetIncrementalTileCollisions(incMoveAmount);
                if (tileCollisions.Count > 0)
                {
                    // If already stuck in tiles (colliding with em before) move slowly so player can get out of them
                    if (allowEscapingWall && beforeMoveTileCollisions.Count > 0)
                    {
                        actor.pos += incMoveAmount * Fd.New(0, 10);
                    }
                    break;
                }

                // IMPORTANT: the order of actorCollisions returned is important, the first hits come first, this tells OnCollision event system to do the first one first
                // so that if say an arrow hits two players, it only hits the closer one it hits first, destroys itself, and doesn't hit two players at once
                List<ActorCollision> currentActorCollisions = colliderComponent.GetAllActorCollisions(incMoveAmount);
                foreach (ActorCollision currentActorCollision in currentActorCollisions)
                {
                    if (!actorCollisions.Any(ac => ac.EqualTo(currentActorCollision)))
                    {
                        actorCollisions.Add(currentActorCollision);
                    }
                }

                // PERF nested for loop here?
                // Allow actor to move if already colliding with it in the first place (like if inside somaria block)
                if (actorCollisions.Any(ac => ac.BlockMovement() && (!allowEscapingWall || !beforeMoveActorCollisions.Any(bmac => bmac.EqualTo(ac)))))
                {
                    break;
                }

                actor.pos += incMoveAmount;
            }
            return (tileCollisions, actorCollisions);
        }

        protected List<FdPoint> GetIncMoveAmounts(FdPoint moveAmount)
        {
            int xSign = moveAmount.x.sign;
            int ySign = moveAmount.y.sign;
            List<FdPoint> positiveIncMoveAmounts = GetIncMoveAmountsPositive(new FdPoint(moveAmount.x.abs, moveAmount.y.abs));
            for (int i = 0; i < positiveIncMoveAmounts.Count; i++)
            {
                positiveIncMoveAmounts[i] = new FdPoint(positiveIncMoveAmounts[i].x * xSign, positiveIncMoveAmounts[i].y * ySign);
            }
            return positiveIncMoveAmounts;
        }

        protected List<FdPoint> GetIncMoveAmountsPositive(FdPoint moveAmount)
        {
            List<FdPoint> results = new List<FdPoint>();
            Fd x = moveAmount.x;
            Fd y = moveAmount.y;

            if (x <= 1 && y <= 1)
            {
                results.Add(new FdPoint(x, y));
                return results;
            }

            // Determine the ratio and max component
            Fd ratio = Fd.Min(x, y) / Fd.Max(x, y);
            bool xIsLarger = x > y;

            // The size of the steps to take
            Fd stepSizeX = xIsLarger ? 1 : ratio;
            Fd stepSizeY = xIsLarger ? ratio : 1;

            int infinteLoopGuard = 0;
            while (x > 0 || y > 0)
            {
                Fd nextStepX = Fd.Min(stepSizeX, x);
                Fd nextStepY = Fd.Min(stepSizeY, y);

                if (x > 0 && nextStepX <= 0) nextStepX = Fd.New(0, 1);
                if (y > 0 && nextStepY <= 0) nextStepY = Fd.New(0, 1);

                results.Add(new FdPoint(nextStepX, nextStepY));
                x -= nextStepX;
                y -= nextStepY;

                infinteLoopGuard++; if (infinteLoopGuard > 10000) throw new Exception("GetIncMoveAmountsPositive: infinite loop");
            }

            return results;
        }
    }
}
