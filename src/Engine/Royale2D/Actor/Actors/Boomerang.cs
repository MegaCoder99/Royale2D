namespace Royale2D
{
    public class Boomerang : Actor
    {
        public VelComponent velComponent;
        public Character owner;
        public bool isMagical;
        public FdPoint returnDir;
        public bool returnToThrower;
        public bool reversedDir = false;
        public Actor? fetchedActor;

        public ColliderComponent colliderComponent;
        public DamagerComponent damagerComponent;

        public Boomerang(Character owner, FdPoint unitVel, bool isMagical) :
            base(owner, owner.pos, "boomerang_throw")
        {
            this.owner = owner;
            this.isMagical = isMagical;

            int speed = 3;
            if (isMagical)
            {
                speed = 4;
            }
            else
            {
                // shader = Global.shaders["replaceRedBlue"];
            }

            FdPoint vel = unitVel * speed;
            returnDir = (vel * -1).Normalized();

            colliderComponent = AddComponent(new ColliderComponent(this, false, true));
            colliderComponent.ChangeMoveStrategy(new ProjMoveStrategy(colliderComponent));
            damagerComponent = AddComponent(new DamagerComponent(this, DamagerType.boomerang, owner));
            velComponent = AddComponent(new VelComponent(this, vel));
            AddComponent(new ShieldableComponent(this, owner, false, velComponent));

            colliderComponent.useWallColliderForStairs = false;

        }

        public override void Update()
        {
            base.Update();
            if (fetchedActor != null)
            {
                fetchedActor.pos = pos;
            }

            if (reversedDir && pos.DistanceTo(owner.pos) < 5)
            {
                OnReturn();
                return;
            }
            velComponent.vel += returnDir * Fd.New(0.075f);
            if (velComponent.vel.Magnitude() < Fd.New(0, 50))
            {
                Reverse();
            }
            if (reversedDir)
            {
                velComponent.vel = pos.DirToNormalized(owner.pos) * velComponent.vel.Magnitude();
            }

            PlaySound("boomerang", true);
        }

        public override void OnDamageDealt()
        {
            base.OnDamageDealt();
            Reverse();
        }

        public override void OnActorCollision(ActorCollision collision)
        {
            base.OnActorCollision(collision);

            Actor actor = collision.other.actor;
            if (actor is Character && actor == owner && reversedDir)
            {
                OnReturn();
            }
            // REFACTOR fetchable component?
            else if (!reversedDir && (actor is Collectable || actor is FieldItem))
            {
                Reverse();
                fetchedActor = actor;
            }
        }

        public override void OnTileCollision(TileCollision collision)
        {
            base.OnTileCollision(collision);
            if (!reversedDir && !collision.other.tileInstance.IsLedge())
            {
                Reverse();
                new Anim(this, pos, "particle_hookshot");
                PlaySound("tink");
            }
        }

        public void Reverse()
        {
            reversedDir = true;
            foreach (Collider c in colliderComponent.globalColliders)
            {
                c.isWallCollider = false;
            }
        }

        public override void OnShieldBlock()
        {
            if (!reversedDir)
            {
                Reverse();
            }
            else
            {
                owner.boomerang = null;
                DestroySelf();
            }
        }

        public void OnReturn()
        {
            owner.boomerang = null;
            if (fetchedActor != null)
            {
                fetchedActor.pos = owner.pos;
            }
            DestroySelf();
        }
    }
}
