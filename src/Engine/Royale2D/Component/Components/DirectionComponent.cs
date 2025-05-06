namespace Royale2D
{
    public class DirectionComponent : Component
    {
        public Direction direction { get; private set; }
        bool disableFlipX;  // Some sprites are not flipped on X axis but have standalone left sprite (like spin attack) because flipping is overcomplex/broken in these cases

        public DirectionComponent(Actor actor, Direction direction) : base(actor)
        {
            Change(direction);
        }

        public void SetDisableFlipX(bool disableFlipX)
        {
            this.disableFlipX = disableFlipX;
            if (disableFlipX) actor.xDir = 1;
            else if (direction == Direction.Left) actor.xDir = -1;
            else actor.xDir = 1;
        }

        public void Change(Direction newDirection)
        {
            if (newDirection == Direction.Left && !disableFlipX) actor.xDir = -1;
            else actor.xDir = 1;
            direction = newDirection;
        }

        public static string GetSpriteName(Direction direction, string baseSpriteName, bool disableFlipX)
        {
            if (direction == Direction.Left && !disableFlipX) return baseSpriteName + "_right";
            if (direction == Direction.Left && disableFlipX) return baseSpriteName + "_left";
            if (direction == Direction.Right) return baseSpriteName + "_right";
            if (direction == Direction.Up) return baseSpriteName + "_up";
            return baseSpriteName + "_down";
        }

        public string GetSpriteName(string baseSpriteName)
        {
            return GetSpriteName(direction, baseSpriteName, disableFlipX);
        }

        public IntPoint ForwardUnitIntVec()
        {
            return Helpers.DirToVec(direction);
        }

        public FdPoint ForwardFdVec(int amount)
        {
            return Helpers.DirToFdVec(direction) * amount;
        }

        public FdPoint ForwardFdVec(Fd amount)
        {
            return Helpers.DirToFdVec(direction) * amount;
        }

        public int GetXDir()
        {
            if (direction == Direction.Left) return -1;
            return 1;
        }
    }
}
