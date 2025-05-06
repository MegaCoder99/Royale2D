using Shared;

namespace Royale2D
{
    public enum ProjDestroyType
    {
        HitEnemy,
        HitWall,
        Blocked
    }

    public class Projectile : Actor
    {
        public Character owner;
        string fadeSprite;
        string fadeSound;
        bool destroyOnHit = true;
        bool destroyOnAnimOver;
        public int range;
        public ColliderComponent colliderComponent;
        public DamagerComponent damagerComponent;
        public VelComponent velComponent;
        FdPoint totalDistTravelled;

        public Projectile(
            Character owner,
            string spriteName,
            string fadeSprite,
            string fadeSound,
            Fd speed,
            int range,
            DamagerType damagerType,
            bool directional = false,
            FdPoint? overridePos = null,
            Direction? overrideDir = null,
            bool destroyOnHit = true,
            bool destroyOnAnimOver = false,
            bool energyBased = false
        ) : base(owner, overridePos ?? owner.GetFirstPOI(), spriteName)
        {
            if (directional)
            {
                AddComponent(new DirectionComponent(this, owner.dir));
            }
            RefreshSprite();
            colliderComponent = AddComponent(new ColliderComponent(this, true, true));
            colliderComponent.ChangeMoveStrategy(new ProjMoveStrategy(colliderComponent));
            damagerComponent = AddComponent(new DamagerComponent(this, damagerType, owner));
            
            FdPoint vel = overrideDir != null ? Helpers.DirToFdVec(overrideDir.Value) * speed : owner.directionComponent.ForwardFdVec(speed);
            velComponent = AddComponent(new VelComponent(this, vel: vel));

            AddComponent(new ShieldableComponent(this, owner, energyBased, velComponent));

            this.owner = owner;
            this.fadeSprite = fadeSprite;
            this.fadeSound = fadeSound;
            this.destroyOnHit = destroyOnHit;
            this.destroyOnAnimOver = destroyOnAnimOver;
            this.range = range;
        }

        public override void Update()
        {
            base.Update();

            totalDistTravelled += deltaPos;
            if (totalDistTravelled.Magnitude() > range)
            {
                DestroySelf();
                return;
            }

            if (destroyOnAnimOver && spriteInstance.sprite.wrapMode == WrapMode.Once && spriteInstance.IsAnimOver())
            {
                DestroySelf();
                return;
            }
        }

        public override void OnDamageDealt()
        {
            base.OnDamageDealt();
            if (destroyOnHit)
            {
                DestroySelfHelper(ProjDestroyType.HitEnemy);
            }
        }

        public override void OnTileCollision(TileCollision collision)
        {
            base.OnTileCollision(collision);
            TileInstance tileInstance = collision.other.tileInstance;

            if (!tileInstance.IsLedge() && destroyOnHit)
            {
                List<TileCollision> collisions = colliderComponent.GetMainTileCollisions(FdPoint.Zero);
                if (collisions.Count == 0)
                {
                    pos += velComponent.vel.Normalized() * 3;
                }

                DestroySelfHelper(ProjDestroyType.HitWall);
            }
        }

        public void DestroySelfHelper(ProjDestroyType destroyType)
        {
            if (sprite.name.Contains("arrow"))
            {
                if (destroyType == ProjDestroyType.HitEnemy)
                {
                    DestroySelf();
                }
                else if (destroyType == ProjDestroyType.HitWall)
                {
                    DestroySelf("arrow_stuck", "arrow_hit_wall");
                }
                else if (destroyType == ProjDestroyType.Blocked)
                {
                    DestroySelf(() =>
                    {
                        // TODO bounce needs to support anim fade + movement
                        // new Anim(this, GetRenderPos(), GetDirFadeSprite("arrow_bounce"), "tink");
                    });
                }
            }
            else
            {
                DestroySelf(fadeSprite, fadeSound);
            }
        }

        public override void OnShieldBlock()
        {
            DestroySelfHelper(ProjDestroyType.Blocked);
        }
    }

    public class Projectiles
    {
        public const int DefaultRange = 128; // 192;

        public static Projectile CreateArrowProj(Character character) => 
            new Projectile(character, "arrow", "", "", speed: 3, range: DefaultRange, DamagerType.arrow, directional: true);

        public static Projectile CreateFireRodProj(Character character) => 
            new Projectile(character, "fire_rod_proj", "flame_burn", "fire", speed: 4, range: DefaultRange, DamagerType.fireRod, energyBased: true);

        public static Projectile CreateIceRodProj(Character character) => 
            new Projectile(character, "ice_rod_proj", "ice_rod_hit", "", speed: 2, range: DefaultRange, DamagerType.iceRod, energyBased: true);

        public static Projectile CreateLampProj(Character character, FdPoint overridePos) => 
            new Projectile(character, "lamp_flame", "", "fire", speed: 0, range: DefaultRange, DamagerType.lamp, destroyOnHit: false, destroyOnAnimOver: true, overridePos: overridePos);

        public static Projectile CreatePowderProj(Character character) => 
            new Projectile(character, "magic_powder_spray", "", "", speed: 0, range: DefaultRange, DamagerType.magicPowder, destroyOnHit: false, destroyOnAnimOver: true);

        public static Projectile CreateSwordBeamProj(Character character) => 
            new Projectile(character, "sword_beam", "sword_beam_break", "", speed: 4, range: DefaultRange, DamagerType.swordBeam, energyBased: true);

        public static Projectile CreateCaneBlockProj(Character character, FdPoint overridePos, Direction overrideDir) => 
            new Projectile(character, "somaria_proj", "sword_beam_break", "", speed: 4, range: DefaultRange, DamagerType.caneBlockProj, energyBased: true, overridePos: overridePos, overrideDir: overrideDir);
    }
}
