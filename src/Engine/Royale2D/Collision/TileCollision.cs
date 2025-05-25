using Shared;

namespace Royale2D
{
    public struct TileCollision
    {
        const int OverlapNudgeThreshold = 7;

        public ActorColliderInstance mine;
        public TileColliderInstance other;
        public long sqeuDistance;
        public string debugString;

        // REFACTOR if we need to add more than this, consider making OnTileCollision parameter higher level obj
        public bool didNudge;

        public TileCollision(ActorColliderInstance mine, TileColliderInstance other)
        {
            this.mine = mine;
            this.other = other;

            sqeuDistance = mine.collider.shape.SqeuDistanceTo(other.collider.shape);

            // This can change as time goes on due to references to actors. To make it static and represent the collision as it happened frozen in time, need to set the string in ctor
            debugString = "Mine: " + mine.GetWorldShape().ToString() + "\n" + "Theirs: " + other.GetWorldShape().ToString();
        }

        public override string ToString()
        {
            return debugString;
        }

        public bool HitIrtAgainstLeg(TileHitboxMode hitboxMode, FdPoint originalMoveAmount)
        {
            if (hitboxMode == TileHitboxMode.DiagTopLeft)
            {
                return originalMoveAmount.x.sign > 0 || originalMoveAmount.y.sign > 0;
            }
            else if (hitboxMode == TileHitboxMode.DiagTopRight)
            {
                return originalMoveAmount.x.sign < 0 || originalMoveAmount.y.sign > 0;
            }
            else if (hitboxMode == TileHitboxMode.DiagBotLeft)
            {
                return originalMoveAmount.x.sign > 0 || originalMoveAmount.y.sign < 0;
            }
            else if (hitboxMode == TileHitboxMode.DiagBotRight)
            {
                return originalMoveAmount.x.sign < 0 || originalMoveAmount.y.sign < 0;
            }
            else
            {
                return false;
            }
        }

        public NudgeData GetNudgeData(FdPoint originalMoveAmount)
        {
            IntPoint nudgeDir = IntPoint.Zero;
            bool splitXY = false;
            int nudgePriority = 0;
            bool useDiagSpeed = false;
            bool isDiagOnRect = false;
            bool isLedge = false;
            bool useSpeedOfOne = false;

            TileInstance tileInstance = other.tileInstance;
            if (Debug.main != null)
            {
                Debug.main.hitTileCoords = new GridCoords(tileInstance.i, tileInstance.j);
            }
            if (tileInstance.IsLedge()) isLedge = true;

            IntShape myShape = mine.GetWorldShape();
            IntShape otherShape = other.GetWorldShape();

            bool irtAsRect = false;
            // For all intents and purposes, if we hit an IRT towards the surface of its legs, treat it like a standard rect
            if (HitIrtAgainstLeg(tileInstance.hitboxMode, originalMoveAmount))
            {
                if (otherShape is IntIrt irt)
                {
                    otherShape = irt.GetBoundingRect();
                    irtAsRect = true;
                }
            }

            if (tileInstance.hitboxMode == TileHitboxMode.FullTile || irtAsRect)
            {
                // Diagonal collision on a rect tile will set splitXY to still move in the direction that works, i.e. if moving down right and hit a tile on the right, move down still
                if (originalMoveAmount.x != 0 && originalMoveAmount.y != 0)
                {
                    nudgeDir = new IntPoint(originalMoveAmount.x.sign, originalMoveAmount.y.sign);
                    splitXY = true;
                    isDiagOnRect = true;
                }
                // Close to tile edge when running into it: nudge player left/right/up/down for a smoother experience
                else
                {
                    int overlapX = myShape.GetOverlapX(otherShape);
                    int overlapY = myShape.GetOverlapY(otherShape);
                    if (overlapX >= 0)
                    {
                        if (overlapX <= OverlapNudgeThreshold && originalMoveAmount.x == 0)
                        {
                            if (myShape.Center().x < otherShape.Center().x && tileInstance.CanNudgeLeft(originalMoveAmount.y.sign))
                            {
                                nudgeDir.x = -1;
                                nudgePriority = 1;
                                useSpeedOfOne = true;
                            }
                            else if (myShape.Center().x > otherShape.Center().x && tileInstance.CanNudgeRight(originalMoveAmount.y.sign))
                            {
                                nudgeDir.x = 1;
                                nudgePriority = 1;
                                useSpeedOfOne = true;
                            }
                        }
                    }
                    if (overlapY >= 0)
                    {
                        if (overlapY <= OverlapNudgeThreshold && originalMoveAmount.y == 0)
                        {
                            if (myShape.Center().y < otherShape.Center().y && tileInstance.CanNudgeUp(originalMoveAmount.x.sign))
                            {
                                nudgeDir.y = -1;
                                nudgePriority = 1;
                                useSpeedOfOne = true;
                            }
                            else if (myShape.Center().y > otherShape.Center().y && tileInstance.CanNudgeDown(originalMoveAmount.x.sign))
                            {
                                nudgeDir.y = 1;
                                nudgePriority = 1;
                                useSpeedOfOne = true;
                            }
                        }
                    }
                }
            }
            else if (tileInstance.hitboxMode == TileHitboxMode.DiagTopLeft)
            {
                if (!(originalMoveAmount.x.sign == -1 && originalMoveAmount.y.sign == -1))
                {
                    if (originalMoveAmount.x.sign == -1)
                    {
                        nudgeDir = new IntPoint(-1, 1);
                        splitXY = originalMoveAmount.y.sign != 0;
                        useDiagSpeed = true;
                    }
                    else if (originalMoveAmount.y.sign == -1)
                    {
                        nudgeDir = new IntPoint(1, -1);
                        splitXY = originalMoveAmount.x.sign != 0;
                        useDiagSpeed = true;
                    }
                }
            }
            else if (tileInstance.hitboxMode == TileHitboxMode.DiagTopRight)
            {
                if (!(originalMoveAmount.x.sign == 1 && originalMoveAmount.y.sign == -1))
                {
                    if (originalMoveAmount.x.sign == 1)
                    {
                        nudgeDir = new IntPoint(1, 1);
                        splitXY = originalMoveAmount.y.sign != 0;
                        useDiagSpeed = true;
                    }
                    else if (originalMoveAmount.y.sign == -1)
                    {
                        nudgeDir = new IntPoint(-1, -1);
                        splitXY = originalMoveAmount.x.sign != 0;
                        useDiagSpeed = true;
                    }
                }
            }
            else if (tileInstance.hitboxMode == TileHitboxMode.DiagBotLeft)
            {
                if (!(originalMoveAmount.x.sign == -1 && originalMoveAmount.y.sign == 1))
                {
                    if (originalMoveAmount.x.sign == -1)
                    {
                        nudgeDir = new IntPoint(-1, -1);
                        splitXY = originalMoveAmount.y.sign != 0;
                        useDiagSpeed = true;
                    }
                    else if (originalMoveAmount.y.sign == 1)
                    {
                        nudgeDir = new IntPoint(1, 1);
                        splitXY = originalMoveAmount.x.sign != 0;
                        useDiagSpeed = true;
                    }
                }
            }
            else if (tileInstance.hitboxMode == TileHitboxMode.DiagBotRight)
            {
                if (!(originalMoveAmount.x.sign == 1 && originalMoveAmount.y.sign == 1))
                {
                    if (originalMoveAmount.x.sign == 1)
                    {
                        nudgeDir = new IntPoint(1, -1);
                        splitXY = originalMoveAmount.y.sign != 0;
                        useDiagSpeed = true;
                    }
                    else if (originalMoveAmount.y.sign == 1)
                    {
                        nudgeDir = new IntPoint(-1, 1);
                        splitXY = originalMoveAmount.x.sign != 0;
                        useDiagSpeed = true;
                    }
                }
            }

            return new NudgeData(nudgeDir, splitXY, nudgePriority, useDiagSpeed, isDiagOnRect, isLedge, useSpeedOfOne);
        }
    }
}
