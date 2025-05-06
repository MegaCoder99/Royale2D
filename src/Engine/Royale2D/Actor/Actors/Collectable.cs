namespace Royale2D
{
    public class Collectable : Actor
    {
        public bool drift;
        public int driftState;
        public int driftTime;
        public FdPoint origPos;
        public string origSpriteName = "";
        public HeartDriftDevData devData;
        public ZComponent zComponent;
        public WadeComponent wadeComponent;

        public Collectable(
            WorldSection section,
            FdPoint pos,
            string spriteName,
            bool launchUp,
            FdPoint unitVelDir = default,
            bool showQuantity = false,
            bool shouldFade = true,
            bool drift = false,
            int healthGain = 0,
            int magicGain = 0,
            int rupeeGain = 0,
            int arrowGain = 0
        ) : base(section, pos, spriteName)
        {
            AddComponent(new ColliderComponent(this, false, false));
            AddComponent(new ShadowComponent(this, ShadowType.Small));

            wadeComponent = AddComponent(new WadeComponent(this));
            AddComponent(new CollectableComponent(
                this, 
                shouldFade, 
                delay: 0, 
                healthGain: healthGain, 
                magicGain: magicGain, 
                rupeeGain: rupeeGain,
                arrowGain: arrowGain
            ));
            
            int quantity = healthGain + magicGain + rupeeGain + arrowGain;
            if (showQuantity)
            {
                AddComponent(new QuantityComponent(this, quantity));
            }

            if (launchUp)
            {
                if (!drift)
                {
                    zComponent = AddComponent(new ZComponent(this, startZ: Fd.New(0, 10), zVel: 1, useGravity: true));
                }
                else
                {
                    zComponent = AddComponent(new ZComponent(this, startZ: Fd.New(0, 10), zVel: Fd.New(1, 50), useGravity: true));
                    origSpriteName = spriteName;
                    baseSpriteName = spriteName + "_drift";
                }
            }
            else
            {
                zComponent = AddComponent(new ZComponent(this));
            }

            if (unitVelDir.IsNonZero())
            {
                AddComponent(new VelComponent(this, unitVelDir));
            }

            this.drift = drift;
            this.origPos = pos;

            // devData = Helpers.LoadDevData<HeartDriftDevData>("heart_drift");
            devData = new HeartDriftDevData
            {
                amp = 10,
                tmod = 3,
                zVelDec = 15
            };
        }

        public override void Update()
        {
            base.Update();
            if (drift)
            {
                // Launch up
                if (driftState == 0)
                {
                    if (zComponent.zVel < 0)
                    {
                        driftState = 1;
                        zComponent.useGravity = false;
                        wadeComponent.disabled = true;
                        zComponent.zVel = -Fd.New(0, devData.zVelDec);
                    }
                }
                // Drifting down
                else if (driftState == 1)
                {
                    driftTime++;
                    Fd driftOffX = (devData.amp * NetcodeSafeMath.SinD(driftTime * devData.tmod) * NetcodeSafeMath.SinD(driftTime * devData.tmod));
                    xDir = deltaPos.x >= 0 ? 1 : -1;
                    pos.x = origPos.x + driftOffX;
                    if (zComponent.z <= 0)
                    {
                        zComponent.z = 0;
                        zComponent.zVel = 0;
                        driftState = 2;
                        wadeComponent.disabled = false;
                        baseSpriteName = origSpriteName;
                    }
                }
            }
        }
    }

    public class Collectables
    {
        public static Collectable CreateGreenRupee(WorldSection section, FdPoint pos, bool launchUp)
        {
            return new Collectable(section, pos, "pickup_rupee_green", launchUp, rupeeGain: 1);
        }

        public static Collectable CreateBlueRupee(WorldSection section, FdPoint pos, bool launchUp)
        {
            return new Collectable(section, pos, "pickup_rupee_blue", launchUp, rupeeGain: 5);
        }

        public static Collectable CreateRedRupee(WorldSection section, FdPoint pos, bool launchUp)
        {
            return new Collectable(section, pos, "pickup_rupee_red", launchUp, rupeeGain: 20);
        }

        public static Collectable CreateArrow(WorldSection section, FdPoint pos, bool launchUp, int amount)
        {
            return new Collectable(section, pos, "pickup_arrow", launchUp, showQuantity: true, arrowGain: amount);
        }

        public static Collectable CreateArrowLoot(WorldSection section, FdPoint pos, FdPoint unitVelDir, int amount)
        {
            return new Collectable(section, pos, "pickup_arrow", true, unitVelDir: unitVelDir, showQuantity: true, shouldFade: false, arrowGain: amount);
        }

        public static Collectable CreateRupeeLoot(WorldSection section, FdPoint pos, FdPoint unitVelDir, int amount)
        {
            var rupeeLoot = new Collectable(section, pos, "pickup_rupee_green", true, unitVelDir: unitVelDir, showQuantity: true, shouldFade: false, rupeeGain: amount);
            rupeeLoot.frameSpeed = 0;
            return rupeeLoot;
        }

        public static Collectable CreateHeart(WorldSection section, FdPoint pos, bool launchUp)
        {
            return new Collectable(section, pos, "pickup_heart", launchUp, healthGain: 4, drift: true);
        }

        public static Collectable CreateSmallMagic(WorldSection section, FdPoint pos, bool launchUp)
        {
            return new Collectable(section, pos, "pickup_magic_small", launchUp, magicGain: 4);
        }

        public static Collectable CreateLargeMagic(WorldSection section, FdPoint pos, bool launchUp)
        {
            return new Collectable(section, pos, "pickup_magic_large", launchUp, magicGain: -1);
        }
    }
}
