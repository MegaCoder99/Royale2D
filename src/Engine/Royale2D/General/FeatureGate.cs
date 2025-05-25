namespace Royale2D
{
    public class FeatureGate
    {
        public static bool swim => (Debug.main?.customAssets == true) || false;
        public static bool lift => (Debug.main?.customAssets == true) || false;
        public static bool spinAttack => (Debug.main?.customAssets == true) || false;
        public static bool pitEntrance => (Debug.main?.customAssets == true) || false;
        public static List<ItemType> allowedItems = Debug.main?.customAssets == true ? [] : 
        [
            ItemType.sword1,
            ItemType.bow,
            ItemType.arrows10,
            ItemType.heartPiece,
            ItemType.heartContainer
        ];
    }
}
