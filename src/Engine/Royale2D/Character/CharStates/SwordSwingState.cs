namespace Royale2D
{
    public class SwordSwingState : CharState
    {
        FdPoint clangVel = FdPoint.Zero;

        public SwordSwingState(Character character) : base(character)
        {
            baseSpriteName = "char_sword";
            damagerType = character.inventory.GetSwordDamagerType();
        }

        public override void Update()
        {
            base.Update();

            if (character.frameIndex == 4 && !once)
            {
                once = true;
                Point offset = Point.Zero;
                Point origin = Point.Zero;
                float chargeDist = 30;
                string sparkleSprite = "sword_swing_sparkle";
                if (character.dir == Direction.Up)
                {
                    origin = character.pos.ToFloatPoint();
                    offset.y = -chargeDist;
                }
                else if (character.dir == Direction.Down)
                {
                    origin = character.pos.ToFloatPoint();
                    offset.y = chargeDist;
                }
                else if (character.dir == Direction.Left)
                {
                    origin = character.pos.ToFloatPoint();
                    offset.x = -chargeDist;
                }
                else if (character.dir == Direction.Right)
                {
                    origin = character.pos.ToFloatPoint();
                    offset.x = chargeDist;
                }

                if (character.inventory.HasItem(ItemType.sword2))
                {
                    Anim sparkle = new Anim(character, (origin + offset).ToFdPoint(), sparkleSprite);
                    //sparkle.dirRotation = true;
                    //sparkle.dir = character.dir;
                    if (character.health.IsMax())
                    {
                        character.PlaySound("sword beam");
                        Projectile beam = Projectiles.CreateSwordBeamProj(character);
                    }
                }
            }

            if (character.IsAnimOver())
            {
                if (FeatureGate.spinAttack && input.IsHeld(Control.Attack))
                {
                    character.ChangeState(new SpinAttackChargeState(character));
                }
                else
                {
                    character.ChangeState(new IdleState(character));
                }
            }
        }

        public override void OnActorCollision(ActorCollision collision)
        {
            base.OnActorCollision(collision);

            Character? otherChr = collision.other.actor as Character;

            if (clangVel.IsZero() && otherChr != null && otherChr != character && collision.mine.collider.tags.Contains("sword"))
            {
                bool hitSword = collision.other.collider.tags.Contains("sword") &&
                    (otherChr.charState is SwordSwingState || otherChr.charState is SpinAttackChargeState || otherChr.charState is SpinAttackState || otherChr.charState is PokeState);
                bool hitFrozenEnemy = otherChr.charState is FrozenState;
                if (hitSword || hitFrozenEnemy)
                {
                    clangVel = collision.other.actor.pos.DirToNormalized(character.pos) * 2;
                    character.velComponent = character.ResetComponent(new VelComponent(character, clangVel));
                    FdPoint intersectPos = collision.GetIntersectCenter();
                    new Anim(character, intersectPos, "cling", new AnimOptions { soundName = "tink" });
                }
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            string sound = "";
            if (character.inventory.HasItem(ItemType.sword1)) sound = "sword1";
            else if (character.inventory.HasItem(ItemType.sword2)) sound = "sword2";
            else if (character.inventory.HasItem(ItemType.sword3)) sound = "sword3";
            else if (character.inventory.HasItem(ItemType.sword4)) sound = "sword4";
            if (sound != "") character.PlaySound(sound);
        }

        public override void OnExit()
        {
            base.OnExit();
            character.velComponent = character.ResetComponent(new VelComponent(character));
        }

        public override bool CanChangeFrom(CharState oldState)
        {
            return oldState is IdleState;
        }
    }
}
