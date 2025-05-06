namespace Royale2D
{
    public class WanderComponent : Component
    {
        ColliderComponent colliderComponent;
        public FdPoint dest;
        public FdPoint origin;
        public int maxStrayDist;
        public int pauseTime;
        public int timeToPause;
        public bool isPaused;
        public int moveDist;
        public Fd speed;

        public FdPoint moveAmount => (dest - actor.pos).Normalized() * speed;

        public WanderComponent(Actor actor, ColliderComponent colliderComponent, Fd? speed = null, int moveDist = 20, int timeToPause = 0, int maxStrayDist = 10000, Direction? initDir = null) : base(actor)
        {
            this.moveDist = moveDist;
            this.timeToPause = timeToPause;
            this.maxStrayDist = maxStrayDist;
            this.speed = speed ?? 1;
            this.colliderComponent = colliderComponent;

            if (initDir == null)
            {
                SetNextMovePos();
            }
            else
            {
                if (initDir.Value == Direction.Left) actor.xDir = -1;
                //if (initDir.Value == Direction.Left) SetNextMovePos(-4, -2, -4, 4);
                //else if (initDir.Value == Direction.Right) SetNextMovePos(2, 4, -4, 4);
                //else if (initDir.Value == Direction.Down) SetNextMovePos(-4, 4, 2, 4);
                //else if (initDir.Value == Direction.Up) SetNextMovePos(-4, 4, -4, -2);
            }

            origin = actor.pos;
        }

        public override void Update()
        {
            base.Update();

            if (!isPaused)
            {
                actor.IncPos(moveAmount);
                bool reachedDest = actor.pos.DistanceTo(dest) < speed * 2;
                if (reachedDest)
                {
                    OnDestReached();
                }
            }
            else if (isPaused)
            {
                pauseTime++;
                if (pauseTime >= timeToPause)
                {
                    pauseTime = 0;
                    isPaused = false;
                    SetNextMovePosHelper();
                }
            }
        }

        public override void OnTileCollision(TileCollision collision)
        {
            base.OnTileCollision(collision);
            OnDestReached();
        }

        public void OnDestReached()
        {
            if (timeToPause > 0)
            {
                isPaused = true;
            }
            else
            {
                SetNextMovePosHelper();
            }
        }

        public void SetNextMovePosHelper()
        {
            int loop = 0;
            while (true)
            {
                loop++; 
                if (loop > 1000)
                {
                    break;
                }
                SetNextMovePos();
                    
                if (colliderComponent.GetMainTileCollisions(moveAmount).Count == 0)
                {
                    break;
                }
            }
        }

        public void SetNextMovePos()
        {
            int startX = -moveDist;
            int endX = moveDist;
            int startY = -moveDist;
            int endY = moveDist;

            if (actor.pos.x - origin.x > maxStrayDist) endX = 0;
            else if (actor.pos.x - origin.x < -maxStrayDist) startX = 0;
            if (actor.pos.y - origin.y > maxStrayDist) endY = 0;
            else if (actor.pos.y - origin.y < -maxStrayDist) startY = 0;

            //int randX = (startX + endX) / 2;
            //int randY = (startY + endY) / 2;

            int randX = NetcodeSafeRng.RandomRange(startX, endX);
            int randY = NetcodeSafeRng.RandomRange(startY, endY);

            dest.x = actor.pos.x + randX;
            dest.y = actor.pos.y + randY;

            if (dest.x < actor.pos.x) actor.xDir = -1;
            else if (dest.x >= actor.pos.x) actor.xDir = 1;
        }
    }
}
