namespace Royale2D
{
    public class TileColliderInstance
    {
        public Collider collider;
        public TileInstance tileInstance;
        public FdPoint moveAmount;

        public TileColliderInstance(Collider collider, TileInstance tileInstance, FdPoint? moveAmount = null)
        {
            this.collider = collider;
            this.tileInstance = tileInstance;
            this.moveAmount = moveAmount ?? FdPoint.Zero;
        }

        public TileClumpInstance? GetTileClumpInstance(params string[] tileClumpTags)
        {
            TileClumpInstance? tileClumpInstance = tileInstance.GetTileClumpInstanceFromTag(tileClumpTags);
            return tileClumpInstance;
        }

        public IntShape GetWorldShape()
        {
            return collider.GetTileInstanceWorldShape(tileInstance);
        }
    }
}
