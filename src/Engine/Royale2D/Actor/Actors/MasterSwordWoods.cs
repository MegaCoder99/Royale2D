namespace Royale2D
{
    public class MasterSwordWoods : Actor
    {
        public bool isPulling;
        public bool isPulled;

        public MasterSwordWoods(WorldSection section, FdPoint pos) : base(section, pos, "master_sword_woods")
        {
            components.Add(new ColliderComponent(this));
        }

        public void Pull()
        {
            isPulled = true;
            visible = false;
            foreach (WorldSection section in world.sections)
            {
                if (section.mapSection.IsWoods())
                {
                    WoodsFogFxLayer? fogFxLayer = section.fxLayers.FirstOrDefault(fxLayer => fxLayer is WoodsFogFxLayer) as WoodsFogFxLayer;
                    if (fogFxLayer != null)
                    {
                        fogFxLayer.fadeOut = true;
                    }
                    if (section.mapSection.name != "woods_grove")
                    {
                        section.fxLayers.Add(new WoodsShadowFxLayer());
                    }
                }
            }
        }
    }
}
