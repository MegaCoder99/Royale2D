namespace Royale2D
{
    public class PokeState : CharState
    {
        SpinAttackChargeState? lastSpinAttackChargeState;
        bool canMove;
        FdPoint clangVel;

        public PokeState(Character character, SpinAttackChargeState? lastSpinAttackChargeState, bool canMove, FdPoint clangVel = default) : base(character)
        {
            this.lastSpinAttackChargeState = lastSpinAttackChargeState;
            this.canMove = canMove;
            this.clangVel = clangVel;
            baseSpriteName = "char_poke";
            idleOnAnimEnd = lastSpinAttackChargeState == null;
            damagerType = character.inventory.GetSwordDamagerType();
            strafe = true;
        }

        public override void Update()
        {
            base.Update();

            if (lastSpinAttackChargeState != null && character.IsAnimOver())
            {
                character.ChangeState(lastSpinAttackChargeState);
            }
        }

        public override bool CanChangeFrom(CharState oldState)
        {
            return oldState is SpinAttackChargeState;
        }

        public override InputMoveData? MoveCode()
        {
            if (!canMove) return null;
            return BasicWalkMoveCode();
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if (clangVel.IsNonZero())
            {
                character.velComponent = character.ResetComponent(new VelComponent(character, clangVel));
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            character.velComponent = character.ResetComponent(new VelComponent(character));
        }
    }
}
