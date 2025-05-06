namespace Royale2D
{
    public enum Direction
    {
        Down,
        Up,
        Left,
        Right
    }

    public enum ShadowType
    {
        None,
        Small,
        Large
    }

    public enum AlignX
    {
        Left,
        Center,
        Right
    }

    public enum AlignY
    {
        Top,
        Middle,
        Bottom
    }

    public enum SwordToDraw
    {
        None,
        Level1,
        Level2,
        Level3,
        Level4
    }

    public enum ShieldToDraw
    {
        None,
        Level1,
        Level2,
        Level3
    }

    public enum Axis
    {
        X,
        Y
    }

    public class InstanceType
    {
        public const string Entrance = "Entrance";
        public const string ChestSmall = "ChestSmall";
        public const string ChestBig = "ChestBig";
        public const string Pot = "Pot";
        public const string MasterSwordWoods = "MasterSwordWoods";
        public const string BigFairy = "BigFairy";
        public const string Fairy = "Fairy";
        public const string Npc = "Npc";
        public const string ShopItem = "ShopItem";
        public const string WorldNumber = "WorldNumber";
        public const string FightersSword = "FightersSword";
    }

    public class TileTag
    {
        public const string LedgeLeft = "ledgeleft";
        public const string LedgeRight = "ledgeright";
        public const string LedgeUp = "ledgeup";
        public const string LedgeDown = "ledgedown";
        public const string LedgeUpLeft = "ledgeul";
        public const string LedgeUpRight = "ledgeur";
        public const string LedgeDownLeft = "ledgedl";
        public const string LedgeDownRight = "ledgedr";
        public const string Water = "water";
        public const string ShallowWater = "swater";
        public const string TallGrass = "tallgrass";
        public const string Steps = "steps";
        public const string Stair = "stair";
        public const string StairUpper = "stairupper";
        public const string StairLower = "stairlower";
        public const string Pit = "pit";
    }
}