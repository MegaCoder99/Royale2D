namespace Royale2D
{
    public struct Damager
    {
        public DamagerType damagerType;
        public string name;
        public int damage;
        public bool flinch;
        public bool selfDamage;
        public bool stun;
        public bool freeze;
        public int burnTime;
        public int bunnyTime;
        public ItemType? itemType;
        public string killFeedSpriteName;   // Only used if itemType is null, since that's a convenient wrapper
        public string killFeedSuffix;       // If set, overrides killFeedSpriteName
        public int damageCooldown;
        public bool hitFrozen;

        public Damager(
            DamagerType damagerType,
            string name,
            // Internally damage is an int and each quarter heart is 1 damage. But to make damage values easier to visualize in code, damageStr is in decimal, i.e. "1.5" = 1 and a half hearts
            // (We're not using decimal because of the risk of netcode safety issues with that data type)
            string damageStr,
            bool selfDamage = false,
            ItemType? item = null,
            string killFeedSpriteName = "",
            bool flinch = true,
            bool stun = false,
            bool freeze = false,
            int burnTime = 0,
            int bunnyTime = 0,
            int damageCooldown = 0,
            bool hitFrozen = false,
            string killFeedSuffix = "")
        {
            this.damagerType = damagerType;
            this.name = name;
            this.damage = GetDamageFromStr(damageStr);
            this.selfDamage = selfDamage;
            this.flinch = flinch;
            this.stun = stun;
            this.freeze = freeze;
            this.burnTime = burnTime;
            this.bunnyTime = bunnyTime;
            this.itemType = item;
            this.killFeedSpriteName = killFeedSpriteName;
            this.damageCooldown = damageCooldown;
            this.hitFrozen = hitFrozen;
            this.killFeedSuffix = killFeedSuffix;
        }

        public static int GetDamageFromStr(string damageStr)
        {
            string[] damagePieces = damageStr.Split('.');
            int wholeDamagePart = int.Parse(damagePieces[0]);
            string decimalDamagePart = damagePieces.Length > 1 ? damagePieces[1] : "";
            int damage = (wholeDamagePart * 4);
            if (decimalDamagePart == "25") damage += 1;
            else if (decimalDamagePart == "5" || decimalDamagePart == "50") damage += 2;
            else if (decimalDamagePart == "75") damage += 3;
            else if (decimalDamagePart == "" || decimalDamagePart == "0") damage += 0;
            else throw new Exception("Invalid damage string: " + damageStr);
            return damage;
        }

        public (string, int) GetKillfeedSpriteAndIndex()
        {
            if (itemType != null)
            {
                return ("hud_item", Items.items[itemType.Value].spriteIndex);
            }
            return (killFeedSpriteName, 0);
        }
    }
}
