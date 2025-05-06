namespace Royale2D
{
    // FYI you must NEVER be able to get "knocked out of" this state somehow
    public class FallState : CharState
    {
        public Entrance entrance;

        public FallState(Character character, Entrance entrance) : base(character)
        {
            baseSpriteName = "char_fall";
            this.entrance = entrance;
            idleOnAnimEnd = false;
            canEnterAsBunny = true;
        }

        public override void Update()
        {
            base.Update();
            if (character.IsAnimOver())
            {
                character.shadowComponent.disabled = true;
                character.ChangeState(new LandState(character));
                entrance.Enter(character);
            }
        }

        public override void OnEnter()
        {
            character.colliderComponent.disabled = true;
            character.pos = entrance.pos;
            character.directionComponent.Change(Direction.Right);
            
            character.shadowComponent.disabled = true;
            character.PlaySound("char falls");
        }

        public override void OnExit()
        {
            character.shadowComponent.disabled = false;
        }

        /*
        public override bool CanChangeFrom(CharState oldState)
        {
            return oldState is IdleState;
        }
        */
    }
}
