namespace Royale2D
{
    public class LiftState : CharState
    {
        bool isHeavy;
        bool thrown;
        public LiftableComponent liftedActorComponent;

        public Actor liftedActor => liftedActorComponent.actor;

        public LiftState(Character character, LiftableComponent liftedActorComponent) : base(character)
        {
            this.liftedActorComponent = liftedActorComponent;
            isHeavy = liftedActorComponent.isHeavy;
            canLedgeJump = true;
            
            if (!isHeavy)
            {
                baseSpriteName = "char_lift";
                enterSound = "lift";
            }
            else
            {
                baseSpriteName = "char_lift_heavy";
            }
        }

        public override void Update()
        {
            base.Update();

            if (liftedActor.isDestroyed)
            {
                character.charState = new IdleState(character);
                return;
            }

            if (baseSpriteName == "char_lift" || baseSpriteName == "char_lift_heavy")
            {
                if (character.IsAnimOver())
                {
                    baseSpriteName = "char_carry";
                    liftedActorComponent.OnLiftAnimDone();
                }
            }
            else if (baseSpriteName == "char_carry")
            {
                if (input.IsPressed(Control.Action))
                {
                    baseSpriteName = "char_throw";
                }
            }
            else if (baseSpriteName == "char_throw")
            {
                if (!thrown)
                {
                    thrown = true;
                    liftedActorComponent.Throw(character, character.directionComponent.ForwardUnitIntVec());
                }

                if (character.IsAnimOver())
                {
                    character.ChangeState(new IdleState(character));
                    return;
                }
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            character.parentComponent.AddChild(liftedActor, ParentPosition.FirstPOI, "bush");
            liftedActorComponent.Lift();
        }

        public override void OnExit()
        {
            base.OnExit();
            if (!thrown && newState is not LedgeJumpState)
            {
                thrown = true;
                liftedActorComponent.Throw(character, character.directionComponent.ForwardUnitIntVec());
            }
        }

        public override bool CanChangeFrom(CharState oldState)
        {
            return oldState is IdleState || oldState is LedgeJumpState;
        }

        public override InputMoveData? MoveCode()
        {
            if (baseSpriteName == "char_carry")
            {
                return BasicWalkMoveCode(isHeavy ? 0 : 1);
            }
            return null;
        }
    }
}
