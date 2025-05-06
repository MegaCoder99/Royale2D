namespace Royale2D
{
    public struct ZIndex
    {
        // The 4 main hierarchial z-index levels (and pre-defined indices of these levels below each)

        // Level 1: current layer actor is in
        public int layerIndex;
        public const int LayerIndexFxGlobal = 100;
        public const int LayerIndexUIGlobal = 101;

        // Level 2: within each layer, there are several "sub-layers"
        public int layerOffset;
        public const int LayerOffsetTile = 0;
        // These are offset by 2 because by convention, the one before will be the shadow layer for that layer
        public const int LayerOffsetActorBelow = 2;
        public const int LayerOffsetActor = 4;
        public const int LayerOffsetActorAbove = 6;
        public const int LayerOffsetTileAbove = 8;
        public const int LayerOffsetEverythingAbove = 10;

        // Level 3: within each level 2 sub-layer, y-position determines z-index
        public int yPosition;

        // Level 4: within each level 3, different child spritess have different z indices (sword, shield, head, etc)
        public int childSpriteOffset;

        public const int ChildOffsetWade = 1;
        public const int ChildOffsetBurn = 11;
        public const int ChildOffsetPoof = 12;
        public const int ChildOffsetUI = 20;

        // Pre-made constants
        public static ZIndex FxGlobalBelow = new ZIndex(-1, 0, 0, 0);
        public static ZIndex FxGlobalAbove = new ZIndex(100, 0, 0, 0);
        public static ZIndex UIGlobal = new ZIndex(101, 0, 0, 0);

        public ZIndex(int layerIndex, int layerOffset, int yPosition, int childSpriteOffset)
        {
            this.layerIndex = layerIndex;
            this.layerOffset = layerOffset;
            this.yPosition = yPosition;
            this.childSpriteOffset = childSpriteOffset;
        }

        public static ZIndex FromLayerIndex(int layerIndex, int layerOffset)
        {
            return new ZIndex(layerIndex, layerOffset, 0, 0);
        }

        public int CompareTo(ZIndex zIndex)
        {
            if (layerIndex != zIndex.layerIndex)
            {
                return layerIndex - zIndex.layerIndex;
            }
            if (layerOffset != zIndex.layerOffset)
            {
                return layerOffset - zIndex.layerOffset;
            }
            if (yPosition != zIndex.yPosition)
            {
                return yPosition - zIndex.yPosition;
            }
            if (childSpriteOffset != zIndex.childSpriteOffset)
            {
                return childSpriteOffset - zIndex.childSpriteOffset;
            }
            return 0;
        }
    }

    public class ZIndexComparer : IComparer<ZIndex>
    {
        public int Compare(ZIndex x, ZIndex y)
        {
            return x.CompareTo(y);
        }
    }
}