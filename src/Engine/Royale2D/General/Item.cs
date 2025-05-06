namespace Royale2D
{
    public class Item
    {
        public ItemType itemType;
        public int spriteIndex;
        public int shopSpriteIndex;
        public bool usesQuantity;
        public int maxQuantity = 1;
        public int spawnOddsWeight = 100;
        public int spawnOddsOverride;  // If not set to 0 will use this instead, but AI and rarity ranking still use the above
        public bool immediate;
        public string name = "";
        public Action<Character>? useAction;
        public ItemType? itemToBecome;

        public Item(ItemType itemType, int spriteIndex, string name, int spawnOddsWeight, bool usesQuantity = false, int maxQuantity = 1, bool immediate = false, Action<Character>? useAction = null, ItemType? itemToBecome = null, int? shopSpriteIndex = null)
        {
            this.itemType = itemType;
            this.spriteIndex = spriteIndex - 1;
            this.name = name;
            this.usesQuantity = usesQuantity;
            this.maxQuantity = maxQuantity;
            this.spawnOddsWeight = spawnOddsWeight;
            this.immediate = immediate;
            this.useAction = useAction;
            this.itemToBecome = itemToBecome;
            this.shopSpriteIndex = shopSpriteIndex ?? spriteIndex;
        }
    }
}
