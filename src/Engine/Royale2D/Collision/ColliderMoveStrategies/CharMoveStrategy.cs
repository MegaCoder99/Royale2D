namespace Royale2D
{
    // This move strategy handles character-specific movement logic, like tile nudging (both for diagonal tiles and square tile edges) and ledge jumping
    public class CharMoveStrategy : ColliderMoveStrategy
    {
        public const int LedgeJumpFrameRequirement = 30;

        public Character character;
        TileInstance? lastLedgeHit;
        int lastLedgeHitFrames;

        public CharMoveStrategy(ColliderComponent colliderComponent, Character character) : base(colliderComponent) 
        {
            this.character = character;
        }

        public override void ApplyMove(FdPoint moveAmount)
        {
            Fd nudgeSpeed = character.lastMoveData.nudgeSpeed;
            Fd diagonalNudgeSpeed = character.lastMoveData.diagonalNudgeSpeed;
            FdPoint inputMoveAmount = character.lastMoveData.inputMoveAmount;

            (List<TileCollision> tileCollisions, List<ActorCollision> actorCollisions) = MoveIncremental(moveAmount, true);

            bool didNudge = false;

            // Nudge check
            if (tileCollisions.Count > 0)
            {
                bool checkNudge = nudgeSpeed != 0 || diagonalNudgeSpeed != 0;
                NudgeData? nullableNudgeData = checkNudge ? GetBestNudgeData(tileCollisions, inputMoveAmount) : null;
                FdPoint posBeforeNudge = actor.pos;
                if (nullableNudgeData != null)
                {
                    NudgeData nudgeData = nullableNudgeData.Value;
                    IntPoint nudgeDir = nudgeData.dir;

                    FdPoint nudgeDirFd = new FdPoint(nudgeDir.x * nudgeSpeed.abs, nudgeDir.y * nudgeSpeed.abs);
                    if (nudgeData.useSpeedOfOne) nudgeDirFd = new FdPoint(nudgeDir.x, nudgeDir.y);
                    if (nudgeData.useDiagSpeed) nudgeDirFd = new FdPoint(nudgeDir.x * diagonalNudgeSpeed.intVal, nudgeDir.y * diagonalNudgeSpeed.intVal);

                    // FYI we may need to fire events for these MoveIncremental collisions too, otherwise we could miss some
                    // Once we do that make sure there are no dup collision events raised
                    if (!nudgeData.splitXY)
                    {
                        MoveIncremental(nudgeDirFd, false);
                    }
                    else
                    {
                        MoveIncremental(new FdPoint(nudgeDirFd.x, 0), false);
                        MoveIncremental(new FdPoint(0, nudgeDirFd.y), false);
                    }

                    if (posBeforeNudge != actor.pos)
                    {
                        didNudge = true;
                    }
                }
            }

            // Ledge jump check
            TileInstance? oldLastLedgeHit = lastLedgeHit;
            lastLedgeHit = GetLastLedgeHit(tileCollisions);
            if (character.charState.canLedgeJump && oldLastLedgeHit != null && lastLedgeHit != null && oldLastLedgeHit.Equals(lastLedgeHit))
            {
                lastLedgeHitFrames++;
                if (lastLedgeHitFrames >= LedgeJumpFrameRequirement)
                {
                    if (FeatureGate.ledgeJump)
                    {
                        character.ChangeState(new LedgeJumpState(character, lastLedgeHit!.Value, character.charState));
                    }
                    lastLedgeHitFrames = 0;
                    lastLedgeHit = null;
                }
            }
            else
            {
                lastLedgeHitFrames = 0;
            }

            foreach (TileCollision collision in tileCollisions)
            {
                TileCollision newCollision = collision;
                newCollision.didNudge = didNudge;
                actor.OnTileCollision(newCollision);
            }

            colliderComponent.SortAndFilterActorCollisions(actorCollisions);
            foreach (ActorCollision actorCollision in actorCollisions)
            {
                actor.OnActorCollision(actorCollision);

                // Soft collision with other characters
                if (actorCollision.other.actor is Character otherChar && actorCollision.mine.collider.isWallCollider && actorCollision.other.collider.isWallCollider)
                {
                    if (moveAmount.IsZero())
                    {
                        Fd dist = otherChar.GetCenterPos().DistanceTo(character.GetCenterPos());
                        Fd modifier = Fd.Clamp(1 - (dist / 16), 0, 1);
                        FdPoint pushAmount = otherChar.GetCenterPos().DirTo(character.GetCenterPos()).Normalized() * Fd.New(0, 50) * modifier;
                        // FYI we may need to fire events for these MoveIncremental collisions too, otherwise we could miss some
                        // Once we do that make sure there are no dup collision events raised
                        MoveIncremental(new FdPoint(pushAmount.x, 0), false);
                        MoveIncremental(new FdPoint(0, pushAmount.y), false);
                    }
                }
            }
        }

        private NudgeData? GetBestNudgeData(List<TileCollision> tileCollisions, FdPoint moveAmount)
        {
            NudgeData? bestNudgeData = null;
            for (int i = 0; i < tileCollisions.Count; i++)
            {
                TileCollision tileCollision = tileCollisions[i];
                NudgeData nudgeData = tileCollision.GetNudgeData(moveAmount);

                if (nudgeData.dir.IsNonZero() && !nudgeData.isLedge && (bestNudgeData == null || nudgeData.priority > bestNudgeData.Value.priority))
                {
                    bestNudgeData = nudgeData;
                }
            }

            return bestNudgeData;
        }

        public TileInstance? GetLastLedgeHit(List<TileCollision> tileCollisions)
        {
            TileInstance? tileInstanceToUse = null;

            // If multiple ledges are hit, use the closest one
            Fd bestDist = Fd.MaxValue;
            foreach (TileCollision tileCollision in tileCollisions)
            {
                TileInstance tileInstance = tileCollision.other.tileInstance;
                if (!tileInstance.IsLedge()) continue;
                Fd dist = tileInstance.GetWorldCenterPos().ToFdPoint().DistanceTo(actor.pos);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    tileInstanceToUse = tileInstance;
                }
            }

            return tileInstanceToUse;
        }
    }
}
