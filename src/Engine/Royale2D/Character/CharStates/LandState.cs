namespace Royale2D
{
    public class LandState : CharState
    {
        int turnTime;

        public LandState(Character character) : base(character)
        {
            baseSpriteName = "char_idle";
            idleOnAnimEnd = true;
            canEnterAsBunny = true;
        }

        public override void Update()
        {
            base.Update();
            turnTime++;
            character.spriteOffset += new IntPoint(0, 3);
            if (turnTime > 4)
            {
                turnTime = 0;
                if (character.dir == Direction.Down) character.directionComponent.Change(Direction.Left);
                else if (character.dir == Direction.Left) character.directionComponent.Change(Direction.Up);
                else if (character.dir == Direction.Up) character.directionComponent.Change(Direction.Right);
                else if (character.dir == Direction.Right) character.directionComponent.Change(Direction.Down);
            }
            if (character.spriteOffset.y >= 0)
            {
                character.wadeComponent.disabled = false;
                character.colliderComponent.disabled = false;
                character.PlaySound("land");
                character.ChangeState(new IdleState(character));
            }
        }

        public override void OnEnter()
        {
            character.spriteOffset -= new IntPoint(0, 130);
            character.wadeComponent.disabled = true;
            character.colliderComponent.disabled = true;
            character.zLayerOffset = ZIndex.LayerOffsetEverythingAbove;
        }

        public override void OnExit()
        {
            base.OnExit();
            character.spriteOffset = new IntPoint(0, 0);
            character.zLayerOffset = ZIndex.LayerOffsetActor;
        }
    }
}
