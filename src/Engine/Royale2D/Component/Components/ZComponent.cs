namespace Royale2D
{
    public class ZComponent : Component
    {
        public static readonly Fd Gravity = -Fd.New(0, 10);

        public Fd z;  // Higher/positive z = higher, above ground
        public Fd zVel;
        public Fd zAcc;
        public bool useGravity;
        public bool bounce;
        public bool bouncedGround;
        int time;
        bool playLandSound;
        Fd? customGravity;
        public bool useZForWallCollisions;

        public ZComponent(Actor actor, Fd startZ = default, bool useGravity = false, bool bounce = false, Fd zVel = default, Fd zAcc = default, bool playLandSound = false, Fd? customGravity = null) : base(actor)
        {
            this.actor = actor;
            this.z = startZ;
            this.zVel = zVel;
            this.zAcc = zAcc;
            this.useGravity = useGravity;
            this.bounce = bounce;
            this.playLandSound = playLandSound;
            this.customGravity = customGravity;
        }

        public override void Update()
        {
            base.Update();

            // This covers cases like planting a bomb right into the water
            if (useGravity)
            {
                if (time == 3) CheckDestroyOnLand();
                time++;
            }

            MoveZAndGetDelta();
        }

        public override FdPoint GetRenderOffset()
        {
            return new FdPoint(0, -z);
        }

        public Fd MoveZAndGetDelta()
        {
            Fd prevZ = z;
            
            zVel += useGravity ? (customGravity ?? Gravity) : zAcc;
            z += zVel;
            var velComponent = actor.GetComponent<VelComponent>();

            if (useGravity)
            {
                // Fell and hit the ground
                if (prevZ > 0 && z <= 0)
                {
                    if (!CheckDestroyOnLand())
                    {
                        actor.OnLand();
                        if (playLandSound) actor.PlaySound("land");
                    }

                    if (bounce && !bouncedGround)
                    {
                        bouncedGround = true;
                        var wc = GetComponent<WadeComponent>();

                        // Landing on a wadable area, like tall grass or shallow water, should not bounce
                        if (!string.IsNullOrEmpty(wc?.GetWadeSpriteName()))
                        {
                            z = 0;
                            zVel = 0;
                            if (velComponent != null) velComponent.vel = FdPoint.Zero;
                        }
                        // This is the first bounce up
                        else
                        {
                            z = 1;
                            zVel /= -2;
                            if (velComponent != null) velComponent.vel *= Fd.New(0, 75);
                        }
                    }
                    // Don't bounce more than once, this is the last landing
                    else
                    {
                        z = 0;
                        zVel = 0;
                        if (velComponent != null) velComponent.vel = FdPoint.Zero;
                        useGravity = false;
                    }
                }
                // Was already on ground to begin with
                else if (prevZ == 0 && z <= 0)
                {
                    z = 0;
                    zVel = 0;
                    useGravity = false;
                    if (velComponent != null) velComponent.vel = FdPoint.Zero;
                }
            }

            return z - prevZ;
        }

        private bool CheckDestroyOnLand()
        {
            var cc = actor.GetComponent<ColliderComponent>();
            if (cc != null && actor is not Character)
            {
                if (cc.IsInTileWithTag(TileTag.Water))
                {
                    actor.PlaySound("walk water");
                    Anim splash = new Anim(actor, actor.pos, "splash_object");
                    actor.DestroySelf();
                    return true;
                }
                else if (cc.IsInTileWithTag(TileTag.Pit))
                {
                    actor.DestroySelf();
                    return true;
                }
            }
            return false;
        }

        public bool HasLanded()
        {
            return zVel <= 0 && z <= 0;
        }

        public void ResetBounce()
        {
            bouncedGround = false;
        }
    }
}
