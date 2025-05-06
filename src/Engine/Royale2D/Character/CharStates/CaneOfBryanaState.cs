namespace Royale2D
{
    public class CaneOfBryanaState : CharState
    {
        Actor? bryanaStart;

        public CaneOfBryanaState(Character character) : base(character)
        {
            baseSpriteName = "char_cane_bryana";
            enterSound = "cane";
            magicCost = 1;
        }

        public override void Update()
        {
            base.Update();

            if (bryanaStart == null)
            {
                bryanaStart = new Anim(character, character.pos, "bryana_start");
            }
            else if (character.sprite.frames.Count > 0 && character.spriteInstance.GetCurrentFrame().POIs.Count > 0)
            {
                FdPoint offset = character.GetFirstPOI();
                if (character.xDir == -1)
                {
                    offset.x *= -1;
                    offset.x += 14;
                }
                bryanaStart.pos = character.pos + /*this.character.getOffset(true) +*/ offset;
                if (bryanaStart.frameIndex == 3)
                {
                    bryanaStart.ChangeFrameIndex(1);
                }
            }

            if (character.IsAnimOver())
            {
                BryanaRing bryanaRing = new BryanaRing(character, character.pos);
                character.parentComponent.AddChild(bryanaRing, ParentPosition.Center);
                ChangeState(new IdleState(character));
            }
        }

        public override bool CanChangeFrom(CharState oldState)
        {
            return oldState is IdleState;
        }

        public static void OnUse(Character character)
        {
            if (character.bryanaRing == null)
            {
                character.ChangeState(new CaneOfBryanaState(character));
            }
            else
            {
                character.parentComponent.RemoveChild(character.bryanaRing, true);
            }
        }
    }
}
