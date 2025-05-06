namespace Royale2D
{
    public class InventoryItem
    {
        public ItemType itemType;
        public int quantity = 1;

        public Item item => Items.items[itemType];

        public InventoryItem(ItemType itemType, int count = 1)
        {
            this.itemType = itemType;
            this.quantity = count;
            /*
            if (item == Items.bombs)
            {
                int rand = Helpers.RandomRange(0, 3);
                if (rand == 0) this.quantity = 15;
                else this.quantity = 5;
            }
            */
        }

        public bool IsMaxed()
        {
            return quantity >= item.maxQuantity;
        }
    }
}
