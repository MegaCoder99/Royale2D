namespace Royale2D
{
    public static class Tips
    {
        public static readonly Dictionary<string, List<string>> tileClumpKeyToTip = new Dictionary<string, List<string>>();

        public static List<string> GetTip(TileClumpInstance tci)
        {
            string key = tci.GetKey();
            if (!tileClumpKeyToTip.ContainsKey(key))
            {
                tileClumpKeyToTip[key] = tips.GetRandomElement();
            }
            return tileClumpKeyToTip[key];
        }

        public static readonly List<List<string>> tips = 
        [
            [ "Tip: The Magic Cape\nmakes you invisible,\nbut not invincible.", "The Cane of Bryana does the\nopposite, making you\ninvincible but not invisible." ],
            [ "Tip: The mirror shield will\nreflect all energy-based\nprojectiles back at the user!", "The user must face\ntheir opponent for\nthis to work." ],
            [ "Tip: A bottled fairy cannot\nrevive you while swimming." ],
            [ "Tip: Use the Magic Powder to\nbriefly transform foes\ninto a helpless form." ],
            [ "Tip: The Quake Medallion\nwill stun nearby\nopponents." ],
            [ "Tip: The Moon Pearl will protect\nyou from becoming helpless\nfrom the Twilight." ],
            [ "Tip: Unlike the Red and Mirror\nShield, the Blue Shield can't\nblock energy-based projectiles." ],
            [ "Tip: Flippers are required\nin your inventory to\nswim in bodies of water." ],
            [ "Tip: A frozen enemy cannot\nbe damaged by swords.", "Use blunt-force weapons\nlike the hammer or bombs." ],
            [ "Tip: Catch a fairy with the\nBug Catching Net to\nrevive on death.", "You will need an\nempty bottle and the net\nin your inventory." ],
            [ "Tip: Skins provide no\nadvantage in gameplay\nand are purely cosmetic." ],
            [ "Tip: Use rupees in-game to\nbuy items from shops." ],
            [ "Tip: The storm deals damage\nover time, and also turns\nyou into a helpless form." ],
            [ "Tip: The lamp can burn enemies,\nbut deals little damage." ],
            [ "Tip: You can set enemies on\nfire with the Lamp,\nFire Rod and Bombos.", "Enemies on fire take\ndamage over time." ],
            [ "Tip: When on fire,\njump in the water\nto put it out!" ],
            [ "Tip: You can thaw a frozen\nenemy or ally with the\nFire Rod, Lamp or Bombos." ],
            [ "Tip: Houses and caves\ncontain the most chests." ],
            [ "Tip: Kakariko Village has\nthe most chests\nof any landing zone.", "But beware: many opponents\nwill also land here\nfor the same reason." ],
            [ "Tip: The Moon Pearl will\nreduce damage taken\nin the Twilight." ],
            [ "Tip: Collect 4 pieces of heart\nin your inventory to\nget an extra heart container." ],
        ];
    }
}
