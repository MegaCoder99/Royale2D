
namespace Royale2D
{
    public class LedgeJumpState : CharState
    {
        public CharState prevState;
        IntPoint ledgeDir;
        int ledgeHeight;
        FdPoint startPos;
        public FdPoint destPos;
        int freezeFrameIndex;

        public LedgeJumpState(Character character, TileInstance tileInstance, CharState prevState) : base(character)
        {
            baseSpriteName = character.GetSpriteNameToUse();
            freezeFrameIndex = character.frameIndex == 0 ? 1 : character.frameIndex;
            this.prevState = prevState;
            enterSound = "fall";
            canEnterAsBunny = true;
            superArmor = true;

            ledgeDir = tileInstance.GetLedgeDir();
            ledgeHeight = tileInstance.GetTileLedgeHeight(ledgeDir.x, ledgeDir.y);
            startPos = character.pos;

            if (ledgeDir.x == 0 && ledgeDir.y > 0)
            {
                InitDownJump();
            }
            else if (ledgeDir.x != 0 && ledgeDir.y == 0)
            {
                InitSideJump(isDown: false);
            }
            else if (ledgeDir.x != 0 && ledgeDir.y > 0)
            {
                InitSideJump(isDown: true);
            }
            else if (ledgeDir.x == 0 && ledgeDir.y < 0)
            {
                InitUpJump(isSide: false);
            }
            else if (ledgeDir.x != 0 && ledgeDir.y < 0)
            {
                InitUpJump(isSide: true);
            }
        }

        public override void Update()
        {
            base.Update();

            if (character.zComponent.HasLanded())
            {
                OnLand();
            }
        }

        public void InitDownJump()
        {
            destPos = startPos + (ledgeDir.ToFdPoint() * (ledgeHeight + Fd.New(3, 50)) * 8);
        }

        public void OnEnterDownJump()
        {
            character.pos = destPos;
            character.zComponent.z = destPos.y - startPos.y;
            character.zComponent.zVel = Fd.New(0, 75);
            character.zComponent.useGravity = true;
        }

        public void InitSideJump(bool isDown)
        {
            destPos.x = startPos.x + (ledgeDir.x * (ledgeHeight + Fd.New(3, 50)) * 8);

            destPos.y = isDown ?
                startPos.y + (ledgeHeight + Fd.New(3, 50)) * 8 :
                startPos.y + ledgeHeight * 8;
        }

        public void OnEnterSideJump(bool isDown)
        {
            character.pos.y = destPos.y;
            character.zComponent.z = destPos.y - startPos.y;
            character.zComponent.zVel = 1;
            character.zComponent.useGravity = true;

            // We need to calculate the amount of time it will take for char to fall to ground to determine the right xVel to reach destPos.x at the exact right time
            Fd? timeToFall = TimeToGround(character.zComponent.z, character.zComponent.zVel, character.zComponent.GetZAcc());

            character.velComponent.vel.x = (destPos.x - startPos.x) / timeToFall!.Value;
        }

        public void InitUpJump(bool isSide)
        {
            destPos = !isSide ?
                startPos + (ledgeDir.ToFdPoint() * (ledgeHeight + Fd.New(3, 50)) * 8) :
                startPos + (ledgeDir.ToFdPoint() * (ledgeHeight + 3) * 8);
        }

        public void OnEnterUpJump(bool isSide)
        {
            character.zComponent.zVel = Fd.New(1, 75);
            character.zComponent.useGravity = true;

            Fd? timeToFall = TimeToGround(character.zComponent.z, character.zComponent.zVel, character.zComponent.GetZAcc());

            if (!isSide)
            {
                Fd moveVel = (destPos.y - startPos.y) / timeToFall!.Value;
                character.velComponent.vel.y = moveVel;
            }
            else
            {
                character.velComponent.vel = (destPos - startPos) / timeToFall!.Value;
            }
        }

        // Deterime the amount of time it will take to hit the ground given initial z position, initial z vel, and gravity, using physics kinematic equations
        public static Fd? TimeToGround(Fd initialZPos, Fd initialZVel, Fd gravity)
        {
            gravity *= -1;
            if (gravity <= 0)
            {
                return null;
            }
            Fd discriminant = (initialZVel * initialZVel) + (2 * gravity * initialZPos);
            if (discriminant < 0)
            {
                // No real solution — won't hit ground
                return null;
            }
            Fd time = (initialZVel + NetcodeSafeMath.Sqrt(discriminant.longFd)) / gravity;
            return time >= 0 ? time : null;
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
            character.frameSpeed = 0;
            character.ChangeFrameIndex(freezeFrameIndex);

            if (ledgeDir.x == 0 && ledgeDir.y > 0)
            {
                OnEnterDownJump();
            }
            else if (ledgeDir.x != 0 && ledgeDir.y == 0)
            {
                OnEnterSideJump(isDown: false);
            }
            else if (ledgeDir.x != 0 && ledgeDir.y > 0)
            {
                OnEnterSideJump(isDown: true);
            }
            else if (ledgeDir.x == 0 && ledgeDir.y < 0)
            {
                OnEnterUpJump(isSide: false);
            }
            else if (ledgeDir.x != 0 && ledgeDir.y < 0)
            {
                OnEnterUpJump(isSide: true);
            }
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
