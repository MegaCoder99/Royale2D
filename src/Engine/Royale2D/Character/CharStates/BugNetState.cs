namespace Royale2D
{
    public class BugNetState : CharState
    {
        int startFrameIndex;

        public BugNetState(Character character) : base(character)
        {
            baseSpriteName = "char_bug_net";
        }

        public override void Update()
        {
            base.Update();
            if (character.frameIndex == startFrameIndex + 1 && time > 30)
            {
                ChangeState(new IdleState(character));
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if (character.dir == Direction.Down)
            {
                startFrameIndex = 2;
                character.ChangeFrameIndex(startFrameIndex);
            }
            else if (character.dir == Direction.Up)
            {
                startFrameIndex = 6;
                character.ChangeFrameIndex(startFrameIndex);
            }
        }

        public override void OnActorCollision(ActorCollision collision)
        {
            base.OnActorCollision(collision);
            if (collision.mine.collider.HasTag("net"))
            {
                if (collision.other.actor is Fairy fairy && !fairy.isDestroyed && character.inventory.TransformFirstItem(ItemType.emptyBottle, ItemType.bottledFairy))
                {
                    fairy.DestroySelf();
                }
                else if (collision.other.actor is Bee bee && !bee.isDestroyed && character.inventory.TransformFirstItem(ItemType.emptyBottle, ItemType.bottledBee))
                {
                    bee.DestroySelf();
                }
            }
        }

        public override bool CanChangeFrom(CharState oldState)
        {
            return oldState is IdleState;
        }
    }
}
