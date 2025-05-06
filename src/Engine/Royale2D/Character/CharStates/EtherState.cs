namespace Royale2D
{
    public class EtherState : CharState
    {
        Actor? etherLightning;
        Actor? etherLightning2;
        int ballAngle;
        int ballRadius = 48;
        List<Actor> etherBalls = new List<Actor>();
        int etherState;
        int stateTime;

        public EtherState(Character character) : base(character)
        {
            baseSpriteName = "char_ether";
            magicCost = 8;
            enterSound = "medallion";
        }

        public override void Update()
        {
            base.Update();

            stateTime++;

            if (character.frameIndex >= 12 && etherState == 0)
            {
                etherState = 1;
                character.PlaySound("ether");
                etherLightning = new Anim(character, character.pos.AddXY(0, 12), "ether_lightning");
            }
            if (etherState > 0)
            {
                if (stateTime > 2)
                {
                    stateTime = 0;
                    /*
                    if (character.shader == null)
                    {
                        character.shader = Global.shaders["etherFlash"];
                    }
                    else
                    {
                        character.shader = null;
                    }
                    */
                }
            }
            if (etherState == 1 && etherLightning != null && etherLightning.IsAnimOver())
            {
                //Damager etherDamager = new Damager(actor, Item.ether, 1);
                //etherDamager.freeze = true;
                character.section.RemoveActor(etherLightning);
                etherLightning = null;
                etherLightning2 = new Anim(character, character.pos, "ether_lightning_2");
                etherState = 2;
            }
            if (etherState == 2 && etherLightning2.IsAnimOver())
            {
                character.section.RemoveActor(etherLightning2);
                etherLightning2 = null;
                etherState = 3;
            }
            if (etherState == 3)
            {
                for (int i = 0; i < 8; i++)
                {
                    //Damager etherDamager = new Damager(actor, Item.ether, 1);
                    //etherDamager.freeze = true;
                    FdPoint ballPos = new FdPoint(NetcodeSafeMath.CosD(i * 45) * ballRadius, NetcodeSafeMath.SinD(i * 45) * ballRadius);
                    Actor ball = new Anim(character, character.pos + ballPos, "ether_ball");
                    etherBalls.Add(ball);
                }
                etherState = 4;
            }
            if (etherState == 4)
            {
                ballAngle += 7;
                for (int i = 0; i < 8; i++)
                {
                    etherBalls[i].pos = character.pos + new FdPoint(NetcodeSafeMath.CosD(ballAngle + i * 45) * ballRadius, NetcodeSafeMath.SinD(ballAngle + i * 45) * ballRadius);
                }
                if (ballAngle > 450) etherState = 5;
            }
            if (etherState == 5)
            {
                ballRadius += 5;
                for (int i = 0; i < 8; i++)
                {
                    etherBalls[i].pos = character.pos + new FdPoint(NetcodeSafeMath.CosD(ballAngle + i * 45) * ballRadius, NetcodeSafeMath.SinD(ballAngle + i * 45) * ballRadius);
                }
                if (ballRadius > 200)
                {
                    etherState = 6;
                }
            }
            if (etherState == 6)
            {
                ChangeState(new IdleState(character));
            }
        }

        public override void OnExit()
        {
            // character.shader = null;
            foreach (Actor etherBall in etherBalls)
            {
                etherBall.DestroySelf();
            }
            if (etherLightning != null) etherLightning.DestroySelf();
            if (etherLightning2 != null) etherLightning2.DestroySelf();
        }

        public override bool CanChangeFrom(CharState oldState)
        {
            return oldState is IdleState;
        }
    }
}
