namespace Royale2D
{
    public class SwimState : CharState
    {
        int swimCooldown;
        int swimStrokeTime;
        public FdPoint swimVel;
        int lastPressActionTime;

        public SwimState(Character character) : base(character)
        {
            baseSpriteName = "char_swim";
        }

        public override void Update()
        {
            base.Update();
            
            if (character.input.IsPressed(Control.Action))
            {
                lastPressActionTime = 20;
            }
            else if (lastPressActionTime > 0)
            {
                lastPressActionTime--;
            }

            if (swimStrokeTime == 0 && swimCooldown == 0 && lastPressActionTime > 0)
            {
                character.PlaySound("swim");
                swimStrokeTime = 1;
                character.ChangeFrameIndex(3);
                character.spriteInstance.currentFrameTime = 0;
                character.childFrameTagsToHide.Remove("swimstroke");
            }
            if (swimStrokeTime > 0)
            {
                swimStrokeTime++;
                if (swimStrokeTime >= 30)
                {
                    swimCooldown = 1;
                    swimStrokeTime = 0;
                    character.childFrameTagsToHide.Add("swimstroke");
                }
            }
            if (swimCooldown > 0)
            {
                swimCooldown++;
                if (swimCooldown > 15)
                {
                    swimCooldown = 0;
                }
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();

            new Anim(character, character.pos, "splash_swim", new AnimOptions { soundName = "water" });

            if (!character.CanSwim())
            {
                character.colliderComponent.disabled = true;
                return;
            }

            character.childFrameTagsToHide.Add("swimstroke");
            character.shadowComponent.disabled = true;
            character.wadeComponent.disabled = true;
        }

        public override void OnExit()
        {
            base.OnExit();
            character.childFrameTagsToHide.Remove("swimstroke");
            character.shadowComponent.disabled = false;
            character.wadeComponent.disabled = false;
            character.colliderComponent.disabled = false;
        }

        public override void OnTileCollision(TileCollision collision)
        {
            base.OnTileCollision(collision);
            if (character.deltaPos.x != 0) swimVel.x = 0;
            if (character.deltaPos.y != 0) swimVel.y = 0;
        }

        public override InputMoveData? MoveCode()
        {
            FdPoint moveAmount = FdPoint.Zero;
            if (input.IsHeld(Control.Left))
            {
                moveAmount.x = -character.speed;
            }
            else if (input.IsHeld(Control.Right))
            {
                moveAmount.x = character.speed;
            }

            if (input.IsHeld(Control.Down))
            {
                moveAmount.y = character.speed;
            }
            else if (input.IsHeld(Control.Up))
            {
                moveAmount.y = -character.speed;
            }

            if (moveAmount.IsZero())
            {
                DampenSwimVel();
            }
            else
            {
                FdPoint with = swimVel.Project(moveAmount);
                FdPoint without = swimVel.WithoutComponent(moveAmount);
                without *= Fd.New(0, 95);
                swimVel = with + without;

                Fd maxSwimSpeed = Fd.New(0, 50);
                if (swimStrokeTime > 0) maxSwimSpeed = 1;

                if (with.Magnitude() < maxSwimSpeed)
                {
                    swimVel += moveAmount * Fd.New(0, 3);
                }
                else
                {
                    DampenSwimVel();
                }
            }

            Fd threshold = Fd.New(0, 25);
            if (moveAmount.x < 0 && swimVel.x < threshold) character.directionComponent.Change(Direction.Left);
            if (moveAmount.x > 0 && swimVel.x > -threshold) character.directionComponent.Change(Direction.Right);
            if (moveAmount.y < 0 && swimVel.y < threshold) character.directionComponent.Change(Direction.Up);
            if (moveAmount.y > 0 && swimVel.y > -threshold) character.directionComponent.Change(Direction.Down);

            return new InputMoveData(swimVel, moveAmount, /*Fd.New(moveAmount.Magnitude())*/ Fd.New(0, 50), 0);
        }

        public void DampenSwimVel()
        {
            swimVel *= Fd.New(0, 95);
            if (swimVel.Magnitude() < Fd.New(0, 10)) swimVel = FdPoint.Zero;
        }

        public override bool CanChangeFrom(CharState oldState)
        {
            return oldState is not SwimState && oldState is not SwimJumpState;
        }
    }
}
