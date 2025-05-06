namespace Royale2D
{
    public class WorldNumber : Actor
    {
        public int number;

        public WorldNumber(WorldSection section, FdPoint pos, int number) : base(section, pos, "empty")
        {
            this.number = number;
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Render(Drawer drawer)
        {
            base.Render(drawer);
            drawer.DrawText(number.ToString(), pos.x.floatVal, pos.y.floatVal, AlignX.Center, AlignY.Middle, FontType.NumberHUD);
        }
    }
}
