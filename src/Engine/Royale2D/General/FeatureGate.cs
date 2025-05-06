namespace Royale2D
{
    public class FeatureGate
    {
        public static bool swim => Debug.customAssets || false;
        public static bool lift => Debug.customAssets || false;
        public static bool spinAttack => Debug.customAssets || false;
        public static bool pitEntrance => Debug.customAssets || false;
        public static bool attack => Debug.customAssets || false;
        public static bool menu => Debug.customAssets || false;
        public static bool battlebus => Debug.customAssets || false;
        public static bool hud => Debug.customAssets || false;
        public static bool storm => Debug.customAssets || false;
    }
}
