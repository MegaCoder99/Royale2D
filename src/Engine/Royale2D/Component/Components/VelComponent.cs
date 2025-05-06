namespace Royale2D
{
    public class VelComponent : Component
    {
        public FdPoint vel;
        public FdPoint acc;
        public FdPoint distTravelled;

        public VelComponent(Actor actor, FdPoint? vel = null, FdPoint? acc = null) : base(actor)
        {
            this.vel = vel ?? FdPoint.Zero;
            this.acc = acc ?? FdPoint.Zero;
        }

        public override void Update()
        {
            base.Update();
            actor.IncPos(vel);
            vel += acc;
            distTravelled += vel;
        }
    }
}
