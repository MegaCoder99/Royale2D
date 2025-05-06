namespace Royale2D
{
    public class HeartDriftDevData
    {
        public int amp;
        public int tmod;
        public int zVelDec;
    }

    public class FluteBirdDevData
    {
        public int startX;
        public int startZ;
        public float zAccFloat;
        public int startAlphaZ;
        public int alphaSpeed;

        public Fd zAcc => Fd.New(zAccFloat);

        public Fd GetInitXVel()
        {
            double val = (Math.Abs(startX * 2) * Math.Sqrt(zAccFloat)) / (2 * Math.Sqrt(startZ));
            return Fd.New((float)val);
        }

        public Fd GetInitZVel()
        {
            double val = -Math.Sqrt(startZ * zAccFloat);
            return Fd.New((float)val);
        }
    }
}