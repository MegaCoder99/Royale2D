using System.Runtime.Serialization;

namespace Royale2D
{
    public struct FdPoint3d
    {
        public Fd x;
        public Fd y;
        public Fd z;

        public FdPoint3d(Fd x, Fd y, Fd z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}
