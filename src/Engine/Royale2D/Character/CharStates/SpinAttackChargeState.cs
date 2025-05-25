using Shared;

namespace Royale2D
{
    public class SpinAttackChargeState : CharState
    {
        int chargeTime;
        int particleTime;
        bool particleAlt;

        public SpinAttackChargeState(Character character) : base(character)
        {
            baseSpriteName = "char_spin_charge";
            canLedgeJump = true;
            strafe = true;
        }

        public override void Update()
        {
            base.Update();

            chargeTime++;
            particleTime++;
            if (particleTime > 6)
            {
                particleTime = 0;
                particleAlt = !particleAlt;
                IntPoint offset = IntPoint.Zero;
                FdPoint origin = FdPoint.Zero;
                int clampedChargeTime = MyMath.ClampMax(chargeTime, 60);
                int chargeDist = 20;
                int shakeOffset = particleAlt ? 0 : 4;
                if (character.directionComponent.direction == Direction.Up)
                {
                    origin = character.pos;
                    offset.x = shakeOffset;
                    offset.y = -(clampedChargeTime * chargeDist) / 60;
                }
                else if (character.directionComponent.direction == Direction.Down)
                {
                    origin = character.pos;
                    offset.x = shakeOffset;
                    offset.y = (clampedChargeTime * chargeDist) / 60;
                }
                else if (character.directionComponent.direction == Direction.Left)
                {
                    origin = character.pos;
                    offset.x = -(clampedChargeTime * chargeDist) / 60;
                    offset.y = shakeOffset;
                }
                else if (character.directionComponent.direction == Direction.Right)
                {
                    origin = character.pos;
                    offset.x = (clampedChargeTime * chargeDist) / 60;
                    offset.y = shakeOffset;
                }
                new Anim(character, origin + offset.ToFdPoint(), "sword_sparkle", new AnimOptions { isParticle = true });
                if (chargeTime > 60 && !once)
                {
                    once = true;
                    character.PlaySound("spin attack charge");
                    new Anim(character, origin + offset.ToFdPoint(), "spin_particle_ready");
                }
            }

            if (!input.IsHeld(Control.Attack))
            {
                if (time > 60)
                {
                    character.ChangeState(new SpinAttackState(character));
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
            
            Character? hitEnemyChar = collision.other.actor as Character;
            if (collision.mine.collider.HasTag("sword") && hitEnemyChar != null && hitEnemyChar.damagableComponent.hurtInvulnTime == 0)
            {
                if (collision.other.collider.isDamagable)
                {
                    character.ChangeState(new PokeState(character, null, true));
                }
                else if (collision.other.collider.tags.Contains("sword") && (hitEnemyChar.charState is SwordSwingState || hitEnemyChar.charState is SpinAttackChargeState || hitEnemyChar.charState is SpinAttackState || hitEnemyChar.charState is PokeState))
                {
                    FdPoint clangVel = collision.other.actor.pos.DirToNormalized(character.pos);
                    FdPoint intersectPos = collision.GetIntersectCenter();
                    new Anim(character, intersectPos, "cling", new AnimOptions { soundName = "tink" });
                    character.ChangeState(new PokeState(character, this, false, clangVel));
                }
            }
        }

        public override void OnTileCollision(TileCollision collision)
        {
            base.OnTileCollision(collision);

            if (collision.mine.collider.isWallCollider && !collision.didNudge)
            {
                TileInstance tileInstance = collision.other.tileInstance;
                TileClumpInstance? tileClumpInstance = tileInstance.GetTileClumpInstanceFromTag(TileClumpTags.Bush);
                if (tileClumpInstance != null && character.IsFacingAndCloseToTileClump(tileClumpInstance.Value))
                {
                    character.ChangeState(new PokeState(character, this, false));
                    character.layer.TransformTileClumpWithAnim(tileClumpInstance.Value, character);
                }
            }
        }

        public override bool CanChangeFrom(CharState oldState)
        {
            return oldState is SwordSwingState || oldState is PokeState || oldState is LedgeJumpState;
        }

        public override InputMoveData? MoveCode()
        {
            return BasicWalkMoveCode();
        }
    }
}
