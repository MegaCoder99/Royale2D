namespace Royale2D
{
    public class BryanaRing : Actor
    {
        public Actor owner;

        public BryanaRing(Character creator, FdPoint pos) :
            base(creator, pos, "bryana_ring")
        {
            this.owner = creator;
            AddComponent(new ColliderComponent(this));
        }

        public override void Update()
        {
            base.Update();

            PlaySound("zol", true);

            /*
            angle += Global.spf * 1000;
            shaderSwapTime += Global.spf;
            if (shaderSwapTime > 0.1)
            {
                shaderSwapTime = 0;
                if (shader == null) shader = Global.shaders["replaceColorSpin"];
                else shader = null; ;
            }
            */
        }
    }
}
