namespace Royale2D
{
    public class ActorCollision
    {
        public ActorColliderInstance mine;
        public ActorColliderInstance other;
        public long sqeuDistance;
        public string debugString;
        public bool filtered;

        public ActorCollision(ActorColliderInstance mine, ActorColliderInstance other)
        {
            this.mine = mine;
            this.other = other;

            sqeuDistance = mine.collider.shape.SqeuDistanceTo(other.collider.shape);

            // This can change as time goes on due to references to actors. To make it static and represent the collision as it happened frozen in time, need to set the string in ctor
            // debugString = "Mine: " + mine.GetWorldShape().ToString() + "\n" + "Theirs: " + other.GetWorldShape().ToString();

            debugString = "Mine: " + mine.debugString + ",Theirs:" + other.debugString;
        }

        public FdPoint GetIntersectCenter()
        {
            IntShape myShape = mine.collider.GetActorWorldShape(mine.actor);
            IntShape otherShape = other.collider.GetActorWorldShape(other.actor);

            if (myShape is IntRect myRect && otherShape is IntRect theirRect)
            {
                return MySpatial.GetRectIntersectCenter(myRect, theirRect).ToFdPoint();
            }
            return FdPoint.Zero;
        }

        public override string ToString()
        {
            return debugString;
        }

        public bool BlockMovement()
        {
            if (mine.actor is Character && mine.collider.isWallCollider && 
               (other.actor is Npc || other.actor is ShopItem || other.actor is CaneBlock))
            {
                return true;
            }

            if (mine.actor.GetComponent<ShieldableComponent>() is ShieldableComponent sc)
            {
                if (sc.ShieldBlockType(this) > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public int CompareToSort(ActorCollision otherCollision)
        {
            // If two damagers connect at once on the same enemy, such as master sword swing + the sword beam, the one with the higher damage wins
            if (other.actor == otherCollision.other.actor)
            {
                if (mine.actor.GetComponent<DamagerComponent>() is DamagerComponent myDc && otherCollision.mine.actor.GetComponent<DamagerComponent>() is DamagerComponent otherDc)
                {
                    // We actually don't filter here. We want the second collision to still connect (but deal no damage)
                    return myDc.damager.damage.CompareTo(otherDc.damager.damage);
                }
            }
            return 0;
        }

        public void CompareToFilter(ActorCollision otherCollision)
        {
            // This scenario addresses a corner case where the same damager collision can hit the enemy's shield and hurtbox at the same time (i.e. if you fire a projectile "inside" of them).
            // We want to prioritize the hurtbox collision and ignore the shield hit. Otherwise, a "ding" sound would play but the enemy would get hit.
            // We could have done the reverse, and prioritized the shield hit, but from a gameplay perspective it makes less sense because you'd expect point blank projectile shots to bypass the shield
            if (mine.actor == otherCollision.mine.actor && other.actor == otherCollision.other.actor)
            {
                int myShieldableType = mine.actor.GetComponent<ShieldableComponent>() is ShieldableComponent mySc ? mySc.ShieldBlockType(this) : 0;
                if (myShieldableType != 0)
                {
                    if (otherCollision.other.collider.isDamagable)
                    {
                        filtered = true;
                    }
                }
            }
        }

        public bool EqualTo(ActorCollision otherActorCollision)
        {
            return mine.actor == otherActorCollision.mine.actor && 
                   other.actor == otherActorCollision.other.actor &&
                   mine.collider.EqualTo(otherActorCollision.mine.collider) &&
                   other.collider.EqualTo(otherActorCollision.other.collider);
        }
    }
}
