namespace Royale2D
{
    public class HookshotHook : Actor
    {
        public bool reverseDir;
        public bool returnedToUser;
        public FdPoint origin;
        public FdPoint origVel;
        public Character owner;
        public Actor? fetchedActor;
        public bool hooked;
        public Fd distTravelled;
        public Fd maxDistToTravel;
        public Fd maxDist;
        public int frames;

        public ColliderComponent colliderComponent;
        public DamagerComponent damagerComponent;
        public DirectionComponent directionComponent;
        public VelComponent velComponent;
        public ShieldableComponent shieldableComponent;

        public HookshotHook(Character owner, FdPoint pos) :
            base(owner, pos, DirectionComponent.GetSpriteName(owner.dir, "hookshot_hook", false))
        {
            directionComponent = AddComponent(new DirectionComponent(this, owner.dir));
            FdPoint vel = directionComponent.ForwardFdVec(4);
            velComponent = AddComponent(new VelComponent(this, vel));

            colliderComponent = AddComponent(new ColliderComponent(this, false, true));
            damagerComponent = AddComponent(new DamagerComponent(this, DamagerType.hookshot, owner));
            shieldableComponent = new ShieldableComponent(this, owner, false, velComponent);
            
            colliderComponent.ChangeMoveStrategy(new ProjMoveStrategy(colliderComponent));
            colliderComponent.useWallColliderForStairs = false;

            maxDist = 108;
            maxDistToTravel = maxDist;
            
            origVel = vel;
            origin = pos;
            this.owner = owner;
        }

        public override void Update()
        {
            base.Update();
            frames++;

            if (fetchedActor != null)
            {
                fetchedActor.pos = pos;
            }
            distTravelled += origVel.Magnitude();
            if (!reverseDir)
            {
                if (distTravelled > maxDistToTravel)
                {
                    DoReverseDir();
                }
            }
            else
            {
                if (hooked)
                {
                    origin += origVel;
                }
                if (distTravelled > maxDistToTravel)
                {
                    returnedToUser = true;
                }
            }
        }

        public void MoveOwner()
        {
            if (distTravelled + 16 >= maxDistToTravel)
            {
                List<TileCollision> collisions = owner.colliderComponent.GetMainTileCollisions(origVel);
                if (collisions.Count > 0)
                {
                    return;
                }
            }

            owner.pos += origVel;
        }

        public override void Render(Drawer drawer)
        {
            base.Render(drawer);

            int maxChains = 10; // Max number of chains at max length

            float xDist = Math.Abs((origin.x - pos.x).floatVal);
            float yDist = Math.Abs((origin.y - pos.y).floatVal);
            int numChainsX = (int)Math.Round(maxChains * (xDist / maxDist.floatVal));
            int numChainsY = (int)Math.Round(maxChains * (yDist / maxDist.floatVal));
            float xIncDist = numChainsX == 0 ? 0 : Math.Sign(origVel.x.floatVal) * xDist / numChainsX;
            float yIncDist = numChainsY == 0 ? 0 : Math.Sign(origVel.y.floatVal) * yDist / numChainsY;
            int numChains = Math.Max(numChainsX, numChainsY);

            for (int i = 0; i < numChains; i++)
            {
                float chainX = origin.x.floatVal + (xIncDist * i);
                float chainY = origin.y.floatVal + (yIncDist * i);

                ZIndex zIndex = GetRenderZIndex(overrideYPos: (int)chainY);
                // This if check below fixes a bug where if hookshotting down stairs, some chains of the hook are rendered in the bottom layer and cut out
                if (owner.layerIndex > layerIndex)
                {
                    zIndex = owner.GetRenderZIndex(overrideYPos: (int)chainY);
                }

                Assets.GetSprite("hookshot_chain").Render(drawer, chainX, chainY, 0, zIndex);
            }
        }

        public override void OnActorCollision(ActorCollision collision)
        {
            base.OnActorCollision(collision);
            if (reverseDir) return;

            Actor actor = collision.other.actor;
            // REFACTOR fetchable component?
            if (actor is Collectable || actor is FieldItem)
            {
                fetchedActor = actor;
                DoReverseDir();
                return;
            }
        }

        public override void OnTileCollision(TileCollision collision)
        {
            base.OnTileCollision(collision);
            if (reverseDir) return;
            
            if (collision.other.tileInstance.HasTileClumpTag(TileClumpTags.Hookable))
            {
                Hook();
            }
            else
            {
                Anim fade = new Anim(this, pos, "particle_hookshot");
                PlaySound("tink");
                DoReverseDir();
                return;
            }
        }

        public override void OnDamageDealt()
        {
            base.OnDamageDealt();
            if (reverseDir) return;

            DoReverseDir();

            // TODO if did no damage, like hitting a frozen enemy, run code below
            //Anim fade = new Anim(section, pos, "particle_hookshot");
            //doReverseDir();
        }

        public void DoReverseDir()
        {
            if (reverseDir) return;

            shieldableComponent.disabled = true;

            maxDistToTravel = distTravelled;
            distTravelled = 0;
            reverseDir = true;
            velComponent.vel *= -1;

            foreach (Collider c in colliderComponent.globalColliders)
            {
                c.isWallCollider = false;
            }
        }

        public void Hook()
        {
            DoReverseDir();
            hooked = true;
            owner.colliderComponent.disabled = true;
            // c.stateManager.actorState.isInvincible = true;
            velComponent.vel = FdPoint.Zero;
            maxDistToTravel += 12;
        }

        public void OnRemove()
        {
            if (fetchedActor != null)
            {
                fetchedActor.pos = owner.pos;
            }
        }

        public override void OnShieldBlock()
        {
            DoReverseDir();
        }
    }
}
