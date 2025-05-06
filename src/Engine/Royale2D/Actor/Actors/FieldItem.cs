namespace Royale2D
{
    public class FieldItem : Actor
    {
        public InventoryItem inventoryItem;
        public ZComponent zComponent;
        public ColliderComponent colliderComponent;

        public FieldItem(WorldSection section, FdPoint pos, InventoryItem inventoryItem, FdPoint velDir, bool launchUp) 
            : base(section, pos, "field_item")
        {
            colliderComponent =AddComponent(new ColliderComponent(this, false, false));
            AddComponent(new ShadowComponent(this, ShadowType.Small));
            AddComponent(new WadeComponent(this));
            if (inventoryItem.quantity > 1)
            {
                AddComponent(new QuantityComponent(this, inventoryItem.quantity));
            }

            if (launchUp || velDir.IsNonZero())
            {
                // zComponent.SetParabolicMotion(velDir, Fd.New(1), Fd.New(1), Fd.New(0, 10));
                // zComponent.SetLaunchUpMotion(Fd.New(1), Fd.New(10));
                zComponent = AddComponent(new ZComponent(this, startZ: Fd.New(0, 10), zVel: 1, useGravity: true, playLandSound: false));
                AddComponent(new VelComponent(this, velDir));
            }
            else
            {
                zComponent = AddComponent(new ZComponent(this, playLandSound: false));

                // Draw field items below other actors always, reducing visual clutter
                // Don't do this while it's being tossed, only after landing
                zLayerOffset = ZIndex.LayerOffsetActorBelow;
            }

            this.inventoryItem = inventoryItem;
            frameSpeed = 0;
            ChangeFrameIndex(inventoryItem.item.spriteIndex);
        }

        public FieldItem(Actor creator, FdPoint pos, InventoryItem inventoryItem, FdPoint velDir, bool launchUp) 
            : this(creator.section, pos, inventoryItem, velDir, launchUp)
        {
        }

        public override void OnLand()
        {
            base.OnLand();
            zLayerOffset = ZIndex.LayerOffsetActorBelow;
        }
    }
}
