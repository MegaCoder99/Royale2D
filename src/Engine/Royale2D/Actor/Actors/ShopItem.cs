namespace Royale2D
{
    public class ShopItem : Actor
    {
        public ItemType itemType;
        public int price;

        public Item item => Items.items[itemType];

        public ShopItem(WorldSection section, FdPoint pos, ItemType shopItemType, int price) : base(section, pos, "field_item")
        {
            this.itemType = shopItemType;
            this.price = price;

            AddComponent(new ColliderComponent(this));
            ChangeFrameIndex(item.shopSpriteIndex);
            frameSpeed = 0;
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Render(Drawer drawer)
        {
            base.Render(drawer);
            drawer.DrawText(price.ToString(), pos.x.floatVal, pos.y.floatVal + 16, AlignX.Center, AlignY.Middle, FontType.NumberHUD);
        }
    }
}
