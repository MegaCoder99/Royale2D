using Shared;

namespace Royale2D
{
    public class CameraShakeComponent : Component
    {
        // 1 = full power, 0 = no shake
        public Point shakePower;
        public int shakeTime;

        public CameraShakeComponent(Actor actor) : base(actor)
        {
        }

        public override void Update()
        {
            base.Update();
            if (shakeTime > 0)
            {
                shakeTime--;
                if (shakeTime == 0)
                {
                    shakePower = Point.Zero;
                }
            }
        }

        public void Shake(float shakeX, float shakeY, int shakeTime = 15)
        {
            shakePower = new Point(shakeX, shakeY);
            this.shakeTime = shakeTime;
        }

        public Point GetShakePowerAtPos(Point pos)
        {
            float distance = actor.pos.DistanceTo(pos.ToFdPoint()).floatVal;
            float percent = (1 - (distance / 256));
            percent = MyMath.Clamp(percent, 0, 1);
            return new Point(shakePower.x * percent, shakePower.y * percent);
        }

        public static Point GetCamOffset(World world, Point shakePower)
        {
            Point offset = Point.Zero;
            int shakeMag = 4;
            if (shakePower.x > 0)
            {
                offset.x = world.frameNum % 2 == 0 ? shakePower.x * shakeMag : -shakePower.x * shakeMag;
            }
            if (shakePower.y > 0)
            {
                offset.y = world.frameNum % 2 == 0 ? shakePower.y * shakeMag : -shakePower.y * shakeMag;
            }
            return offset;
        }
    }
}
