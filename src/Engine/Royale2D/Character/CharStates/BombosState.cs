namespace Royale2D
{
    public class BombosState : CharState
    {
        int flameAngle;
        int flameRadius;
        int bombosState;
        int bombsSpawned;
        int stateTime;

        public BombosState(Character character) : base(character)
        {
            baseSpriteName = "char_bombos";
            magicCost = 8;
            enterSound = "medallion";
        }

        public override void Update()
        {
            base.Update();

            stateTime++;

            if (bombosState == 0 && character.IsAnimOver())
            {
                stateTime = 0;
                bombosState = 1;
            }
            if (bombosState == 1)
            {
                character.PlaySound("fire", true);
                if (stateTime > 6)
                {
                    FdPoint basePos = character.pos.AddXY(0, 25);
                    stateTime = 0;
                    FdPoint flamePos = new FdPoint(NetcodeSafeMath.CosD(flameAngle) * flameRadius, NetcodeSafeMath.SinD(flameAngle) * flameRadius);
                    Anim bombosFlame = new Anim(character, basePos + flamePos, "bombos_flame");
                    // Damager damager = new Damager("bombos", 1, burnTime: 120);
                    flameRadius += 5;
                    flameAngle += 40;
                }
                if (flameRadius >= 80)
                {
                    bombosState = 2;
                    stateTime = 0;
                }
            }
            else if (bombosState == 2)
            {
                if (stateTime > 60)
                {
                    bombosState = 3;
                    stateTime = 0;
                }
            }
            else if (bombosState == 3)
            {
                if (stateTime > 3)
                {
                    int randX = NetcodeSafeRng.RandomRange(-128, 128);
                    int randY = NetcodeSafeRng.RandomRange(-112, 112);
                    FdPoint randPos = new FdPoint(randX, randY);
                    //Damager damager = new Damager(character, Item.bombos, 3);
                    //damager.burn = true;
                    Anim bombosBomb = new Anim(character, character.pos + randPos, "bombos_bomb");
                    randX = NetcodeSafeRng.RandomRange(-128, 128);
                    randY = NetcodeSafeRng.RandomRange(-112, 112);
                    bombosBomb = new Anim(character, character.pos + randPos, "bombos_bomb");
                    character.PlaySound("bomb explode", true);
                    stateTime = 0;
                    bombsSpawned++;
                }
                if (bombsSpawned > 25)
                {
                    ChangeState(new IdleState(character));
                }
            }
        }

        public override string GetChangeToError()
        {
            if (character.inventory.SwordLevel() == 0)
            {
                return "Bombos requires a sword";
            }
            return base.GetChangeToError();
        }

        public override bool CanChangeFrom(CharState oldState)
        {
            return oldState is IdleState;
        }
    }
}
