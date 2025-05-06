namespace Royale2D
{
    public class CaneOfSomariaState : CharState
    {
        bool makeBlock;

        public CaneOfSomariaState(Character character) : base(character)
        {
            baseSpriteName = "char_cane";
            idleOnAnimEnd = true;
            makeBlock = character.caneBlock == null || character.caneBlock.isDestroyed;
            if (makeBlock)
            {
                enterSound = "cane";
                magicCost = 4;
            }
            else
            {
                character.PlaySound("sword beam");
            }
        }

        public override void Update()
        {
            base.Update();

            FdPoint? poi = character.GetFirstPOIOrNull();
            if (poi != null && !once)
            {
                once = true;
                if (makeBlock)
                {
                    CaneBlock caneBlock = new CaneBlock(character, poi.Value);
                    character.caneBlock = caneBlock;
                }
                else if (character.caneBlock != null && !character.caneBlock.isDestroyed)
                {
                    character.caneBlock.Split();
                    character.caneBlock = null;
                }
            }
        }
        
        public override bool CanChangeFrom(CharState oldState)
        {
            return oldState is IdleState;
        }
    }
}
