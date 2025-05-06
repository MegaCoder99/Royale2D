namespace Royale2D
{
    public class DashState : CharState
    {
        int particleTime;

        public DashState(Character character) : base(character)
        {
            baseSpriteName = "char_dash";
            damagerType = character.inventory.GetSwordDamagerType();
            canLedgeJump = true;
        }

        public override void Update()
        {
            base.Update();

            particleTime++;
            if (particleTime > 6)
            {
                particleTime = 0;
                int yPos = 10;
                if (character.dir == Direction.Down) yPos = -10;
                new Anim(character, character.pos.AddXY(0, yPos), "dust", new AnimOptions { isParticle = true });
            }
            
            character.PlaySound("pegasus boots", true);

            if (input.IsHeld(Control.Left) || input.IsHeld(Control.Right) || input.IsHeld(Control.Up) || input.IsHeld(Control.Down))
            {
                character.ChangeState(new IdleState(character));
            }
        }

        public override void OnTileCollision(TileCollision collision)
        {
            base.OnTileCollision(collision);
            if (!collision.other.tileInstance.IsLedge() && !collision.didNudge)
            {
                character.ChangeState(new BonkState(character, character.directionComponent.ForwardFdVec(1)));
            }
        }

        public override InputMoveData? MoveCode()
        {
            int dashSpeed = 4;

            if (character.colliderComponent.IsInTileWithTagAnyLayer(TileTag.Steps, TileTag.Stair, TileTag.StairUpper, TileTag.StairLower))
            {
                dashSpeed = 2;
            }

            FdPoint moveAmount = Helpers.DirToFdVec(character.dir) * dashSpeed;
            return new InputMoveData(moveAmount, moveAmount, 2, 3);
        }

        public override bool CanChangeFrom(CharState oldState)
        {
            return oldState is DashChargeState || oldState is LedgeJumpState;
        }
    }
}
