namespace Royale2D
{
    public class FairyDieState : CharState
    {
        int state;
        int stateTime;
        int zDir = 1;
        int fairySoundTime;
        Direction lastDir;
        Actor? fairy;
        FdPoint fairyDestPos1;

        public FairyDieState(Character character) : base(character)
        {
            baseSpriteName = "char_die";
            enterSound = "char dies";
            intangible = true;
        }

        public override void Update()
        {
            base.Update();
            stateTime++;

            if (state == 0)
            {
                if (stateTime > 150)
                {
                    state = 1;
                    fairy = new Actor(character, character.pos, "fairy");
                    fairy.AddComponent(new VelComponent(fairy));
                    fairyDestPos1 = character.pos.AddXY(0, -20);
                    character.inventory.TransformFirstItem(ItemType.bottledFairy, ItemType.emptyBottle);
                }
            }
            if (state == 1)
            {
                if (fairy.MoveToPos(fairyDestPos1, Fd.Point5))
                {
                    state = 2;
                    baseSpriteName = "fairy_heal";
                }
            }
            else if (state == 2)
            {
                fairy.pos.y = fairyDestPos1.y + NetcodeSafeMath.SinD((stateTime * Fd.OnePoint5).intVal);
                if (fairySoundTime == 0)
                {
                    character.PlaySound("fairy");
                    fairySoundTime++;
                }
                else
                {
                    fairySoundTime++;
                    if (fairySoundTime > 75)
                    {
                        fairySoundTime = 0;
                    }
                }
                if (fairy.loopCount >= 2)
                {
                    state = 3;
                    baseSpriteName = "fairy";
                    if (fairy.GetComponent<VelComponent>() is VelComponent vc)
                    {
                        vc.acc = new FdPoint(Fd.New(0, 2), -Fd.New(0, 2));
                    }
                    character.health.AddOverTime(7);
                }
            }
            else if (state == 3)
            {
                fairy.alpha -= Game.spfConst;

                Fd zInc = Fd.New(0, 20) * zDir;
                character.zComponent.z += zInc;
                if (zInc > 12)
                {
                    zInc = 0;
                    zDir = -1;
                }

                if (!character.health.IsChanging())
                {
                    fairy.DestroySelf();
                    character.directionComponent.Change(lastDir);
                    character.zComponent.z = 0;
                    // character.invulnComponent.AddTransitionInvulnTime(60);
                    // Global.game.killFeed.Add(new KillFeedEntry(character.playerName + " was revived"));
                    ChangeState(new IdleState(character));
                }
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            lastDir = character.dir;
            character.directionComponent.Change(Direction.Right);
            //stateManager.hurtTime = 1;
        }
    }
}
