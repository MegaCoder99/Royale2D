namespace Royale2D
{
    public class LiftableComponent : Component
    {
        public Fd throwSpeedForward;
        public Fd throwSpeedUp;    // "Up" as in the z direction, not the y direction
        public bool isHeavy;
        bool destroyOnContact;
        string fadeSpriteName = "";
        string fadeSoundName = "";
        public bool isLifted;
        public bool isThrown;
        public FdPoint? customThrowVel;
        public Character? lastThrower;
        ZComponent zComponent;
        VelComponent velComponent;
        ColliderComponent colliderComponent;

        public LiftableComponent(
            Actor actor, 
            ZComponent zComponent, 
            VelComponent velComponent,
            ColliderComponent colliderComponent,
            Fd throwSpeed, 
            Fd throwSpeedUp,
            bool destroyOnContact,
            bool isHeavy,
            string fadeSpriteName = "",
            string fadeSoundName = "") : base(actor)
        {
            this.zComponent = zComponent;
            this.velComponent = velComponent;
            this.colliderComponent = colliderComponent;
            this.throwSpeedForward = throwSpeed;
            this.throwSpeedUp = throwSpeedUp;
            this.destroyOnContact = destroyOnContact;
            this.isHeavy = isHeavy;
            this.fadeSpriteName = fadeSpriteName;
            this.fadeSoundName = fadeSoundName;
        }

        public override void Update()
        {
            base.Update();
            if (isThrown && zComponent.HasLanded())
            {
                isThrown = false;
            }
        }

        public bool CanBeLifted()
        {
            return FeatureGate.lift && !isLifted && !isThrown;
        }

        public void Lift()
        {
            isLifted = true;

            zComponent.useGravity = false;
            zComponent.ResetBounce();
            colliderComponent.disabled = true;

            actor.DisableComponent<WadeComponent>();
            actor.DisableComponent<ShadowComponent>();
        }

        public void Throw(Character character, IntPoint unitDir)
        {
            isLifted = false;
            isThrown = true;

            lastThrower = character;
            actor.PlaySound("throw");
            character.parentComponent.RemoveChild(actor, false);
            colliderComponent.disabled = false;
            var projMoveStrategy = new ProjMoveStrategy(colliderComponent);
            colliderComponent.ChangeMoveStrategy(projMoveStrategy);

            zComponent.useGravity = true;      
            zComponent.zVel = throwSpeedUp;
            zComponent.z = character.pos.y - actor.pos.y;

            actor.pos.y += zComponent.z;

            bool throwingUpOrDown = unitDir.y != 0;
            if (throwingUpOrDown)
            {
                zComponent.useZForWallCollisions = true;
                // When throwing down, the lifted object could be touching a wall if your back is to it. This allows it to not be destroyed immediately
                if (unitDir.y > 0)
                {
                    projMoveStrategy.CheckHitWallLastFrame();
                }
            }
            else
            {
                zComponent.useZForWallCollisions = false;
            }

            velComponent.vel = new FdPoint(throwSpeedForward * unitDir.x, throwSpeedForward * unitDir.y);

            actor.EnableComponent<WadeComponent>();
            actor.EnableComponent<ShadowComponent>();
        }

        public void OnLiftAnimDone()
        {
            actor.zLayerOffset = ZIndex.LayerOffsetActorAbove;
        }

        public override void OnLand()
        {
            actor.zLayerOffset = ZIndex.LayerOffsetActor;
            CheckDestroy();
        }

        public override void OnDamageDealt()
        {
            CheckDestroy();
        }

        public override void OnTileCollision(TileCollision collision)
        {
            CheckDestroy();
        }

        public void CheckDestroy()
        {
            if (destroyOnContact)
            {
                actor.DestroySelf(fadeSpriteName, fadeSoundName);
            }
        }
    }
}
