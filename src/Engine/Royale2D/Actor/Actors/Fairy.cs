namespace Royale2D
{
    public class Fairy : Actor
    {
        int time;
        public ColliderComponent colliderComponent;
        public VelComponent velComponent;
        public WanderComponent wanderComponent;

        public Fairy(WorldSection section, FdPoint pos, bool shouldFade, Direction? initDir = null) : base(section, pos, "fairy")
        {
            colliderComponent = AddComponent(new ColliderComponent(this), true);
            AddComponent(new ShadowComponent(this, ShadowType.Small));
            AddComponent(new CollectableComponent(this, shouldFade, healthGain: 7));
            AddComponent(new ZComponent(this, 6));
            velComponent = AddComponent(new VelComponent(this));
            wanderComponent = AddComponent(new WanderComponent(this, colliderComponent, maxStrayDist: 20, initDir: initDir));
        }

        public override void Update()
        {
            base.Update();
            if (time > 10)
            {
                colliderComponent.disabled = false;
            }
            time++;
        }

        public static void OnUseBottledFairy(Character character)
        {
            FdPoint pos = character.GetCenterPos() + character.directionComponent.ForwardFdVec(12);
            new Fairy(character.section, pos, true, initDir: character.dir);
        }
    }
}
