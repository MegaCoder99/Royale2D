namespace Royale2D
{
    public class FluteBirdState : CharState
    {
        FluteBird fluteBirdPickUp;
        FluteBird? fluteBirdDropOff;
        int state;

        public FluteBirdState(Character character, FluteBird fluteBird) : base(character)
        {
            baseSpriteName = "char_idle";
            this.fluteBirdPickUp = fluteBird;
            visible = false;
        }

        public override void Update()
        {
            base.Update();

            // Carried by bird animation
            if (state == 0)
            {
                if (fluteBirdPickUp.isDestroyed)
                {
                    state = 1;
                    character.EnterFluteScreen();
                }
            }
            // In the flute menu
            else if (state == 1)
            {
                if (!character.fluteScreenData.isInFluteScreen)
                {
                    character.pos.x = character.fluteScreenData.dropJ * 8;
                    character.pos.y = character.fluteScreenData.dropI * 8;
                    fluteBirdDropOff = FluteBird.New(character, true);
                    state = 2;
                }
            }
            // Dropped off by bird animation
            else if (state == 2)
            {
                if (fluteBirdDropOff!.shouldDrop)
                {
                    character.ChangeState(new IdleState(character));
                }
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            character.zComponent = character.ResetComponent(new ZComponent(character));
            character.colliderComponent.disabled = true;
            character.directionComponent.Change(Direction.Right);
        }

        public override void OnExit()
        {
            base.OnExit();
            character.colliderComponent.disabled = false;
        }
    }
}
