namespace Royale2D
{
    public class LedgeJumpState : CharState
    {
        public CharState prevState;
        IntPoint ledgeDir;
        int ledgeHeight;
        FdPoint startPos;
        int freezeFrameIndex;
        List<FdPoint3d> jumpPath;

        public LedgeJumpState(Character character, TileInstance tileInstance, CharState prevState) : base(character)
        {
            baseSpriteName = character.GetSpriteNameToUse();
            freezeFrameIndex = character.frameIndex == 0 ? 1 : character.frameIndex;
            this.prevState = prevState;
            enterSound = "fall";
            startPos = character.pos;
            canEnterAsBunny = true;
            superArmor = true;

            ledgeDir = tileInstance.GetLedgeDir();
            ledgeHeight = tileInstance.GetTileLedgeHeight(ledgeDir.x, ledgeDir.y);

            jumpPath = GetJumpPath(ledgeDir, ledgeHeight);
        }

        public override void Update()
        {
            base.Update();
            character.frameSpeed = 0;
            character.ChangeFrameIndex(freezeFrameIndex);

            if (time >= jumpPath.Count)
            {
                OnLand();
            }
            else
            {
                character.pos.x += jumpPath[time].x;
                character.pos.y += jumpPath[time].y;
                character.zComponent.z += jumpPath[time].z;
            }
        }

        // PERF pre-computing is done all at once in one frame and may spike the CPU for a moment
        public List<FdPoint3d> GetJumpPath(IntPoint ledgeDir, int height)
        {
            int xDir = ledgeDir.x;
            int yDir = ledgeDir.y;

            Fd maxDist;
            FdPoint destPoint;
            bool teleportDown = false;

            Fd tilesLeftOrRight = ledgeDir.x * height;
            Fd tilesUpOrDown = ledgeDir.y * height;

            bool upLeft = false;
            Fd gravity = Fd.New(0, 10);

            Fd zVel = 0;
            Fd z = 0;
            FdPoint vel = FdPoint.Zero;

            //Up left/right
            if ((xDir == -1 && yDir == -1) || (xDir == 1 && yDir == -1))
            {
                upLeft = true;
            }
            //Up
            else if (xDir == 0 && yDir == -1)
            {
                tilesUpOrDown *= Fd.New(1, 13);
            }
            //down
            else if (xDir == 0 && yDir == 1)
            {
                teleportDown = true;
                zVel = 1;
            }
            //Sides
            else if ((xDir == -1 && yDir == 0) || (xDir == 1 && yDir == 0))
            {
                teleportDown = true;
                zVel = 1;
                tilesLeftOrRight *= Fd.New(1, 25);
                tilesUpOrDown = (tilesLeftOrRight.abs * Fd.New(0, 63));
            }
            //down left / right
            else
            {
                teleportDown = true;
                zVel = 1;
                tilesLeftOrRight *= Fd.New(1, 13);
                tilesUpOrDown = tilesLeftOrRight.abs;
            }

            FdPoint pos = FdPoint.Zero;
            FdPoint offset = new FdPoint(tilesLeftOrRight * 8, tilesUpOrDown * 8);
            destPoint = pos + offset.IncMag(8);

            if (teleportDown)
            {
                z = destPoint.y - pos.y;
                pos.y = destPoint.y;
                Fd v = zVel;
                Fd g = gravity;
                Fd d = z;
                LongFd valToSqrt = (v.longFd * v.longFd) + (2 * g.longFd * d.longFd);
                Fd t = (v + NetcodeSafeMath.Sqrt(valToSqrt)) / g;
                Fd velX = (pos.x - destPoint.x).abs / t;
                vel.x = velX * tilesLeftOrRight.sign;
                maxDist = 0;
            }
            else
            {
                Fd speed = 1;
                if (upLeft) speed = Fd.OnePoint5;
                Fd dist = pos.DistanceTo(destPoint);
                Fd t = dist / speed;
                zVel = Fd.Point5 * gravity * t;
                z = Fd.Point1;
                vel = pos.DirTo(destPoint) * speed;
                maxDist = pos.DistanceTo(destPoint);
            }

            var points = new List<FdPoint3d>();
            points.Add(new FdPoint3d(pos.x, pos.y, z));

            Fd distTravelled = 0;
            bool once = false;

            while (true)
            {
                Fd MoveZAndGetDelta()
                {
                    Fd prevZ = z;
                    z += zVel;
                    zVel -= gravity;
                    if (z < 0) z = 0;
                    return z - prevZ;
                };

                MoveZAndGetDelta();
                FdPoint moveAmount = vel;
                pos += moveAmount;

                points.Add(new FdPoint3d(pos.x, pos.y, z));

                Fd inc = vel.Magnitude();
                distTravelled += inc;
                if (!once && (distTravelled > maxDist - inc || maxDist == 0))
                {
                    once = true;
                    if (!teleportDown) vel = FdPoint.Zero;
                    if (!teleportDown) pos = destPoint;
                }
                if (once && z == 0)
                {
                    break;
                }
            }

            var diffPoints = new List<FdPoint3d>();
            diffPoints.Add(points[0]);
            for (int i = 1; i < points.Count; i++)
            {
                var oldPoint = points[i - 1];
                var newPoint = points[i];
                diffPoints.Add(new FdPoint3d(newPoint.x - oldPoint.x, newPoint.y - oldPoint.y, newPoint.z - oldPoint.z));
            }

            return diffPoints;
        }

        public void OnLand()
        {
            character.zComponent.z = 0;
            if (character.layerIndex > 0) character.layerIndex--;
            character.colliderComponent.disableWallCollider = false;

            Entrance? entrance = character.world.entranceSystem.GetCollidingEntrance(character);
            if (entrance != null && entrance.fall)
            {
                ChangeState(new FallState(character, entrance));
            }

            if (character.colliderComponent.IsInTileWithTag(TileTag.Water))
            {
                if (character.CanSwim())
                {
                    ChangeState(new SwimState(character));
                }
                else
                {
                    ChangeState(new SwimJumpState(character, startPos, true));
                }
            }
            else
            {
                character.PlaySound("land");
                character.ChangeState(prevState);
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            character.zComponent = character.ResetComponent(new ZComponent(character));
            character.wadeComponent.disabled = true;
            character.colliderComponent.disableWallCollider = true;
        }

        public override void OnExit()
        {
            base.OnExit();
            character.frameSpeed = 1;
            character.wadeComponent.disabled = false;
            character.colliderComponent.disableWallCollider = false;
        }

        public override bool CanChangeFrom(CharState oldState)
        {
            return oldState is IdleState || oldState is DashState || oldState is SpinAttackChargeState || oldState is LiftState;
        }
    }
}
