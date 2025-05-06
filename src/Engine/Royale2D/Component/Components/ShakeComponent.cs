namespace Royale2D
{
    public class ShakeComponent : Component
    {
        public FdPoint shakeAmount;

        public ShakeComponent(Actor actor) : base(actor)
        {
        }

        public override void Update()
        {
            base.Update();
            shakeAmount = FdPoint.FromXY(Helpers.RandomRange(-1, 1), Helpers.RandomRange(-1, 1));
        }

        public override FdPoint GetRenderOffset()
        {
            return shakeAmount;
        }
    }
}
