namespace Royale2D
{
    public class NetcodeSafeMath
    {
        public static Fd Sqrt(LongFd n)
        {
            if (n.internalVal < 0) throw new ArgumentOutOfRangeException("Cannot compute square root of a negative number.");
            if (n.internalVal == 0) return 0;

            LongFd x = n;
            LongFd root;
            LongFd epsilon = new LongFd(2);

            while (true)
            {
                root = new LongFd(50) * (x + n / x);

                // Check for convergence within some small delta epsilon
                if ((root - x).abs.internalVal < epsilon.internalVal)
                {
                    break;
                }

                x = root;
            }

            return root.fd;
        }

        public static Fd SinD(Fd degrees)
        {
            int intDegrees = degrees.Rounded();
            while (intDegrees < 0) intDegrees += 360;
            while (intDegrees >= 360) intDegrees -= 360;
            return LookupTables.main.sin[intDegrees];
        }

        public static Fd CosD(Fd degrees)
        {
            int intDegrees = degrees.Rounded();
            while (intDegrees < 0) intDegrees += 360;
            while (intDegrees >= 360) intDegrees -= 360;
            return LookupTables.main.cos[intDegrees];
        }

        public static Fd ArcTanD(Fd y, Fd x)
        {
            // Check if x is zero to handle vertical lines
            if (x == 0)
            {
                // Return 90 or 270 degrees based on the sign of y
                if (y > 0) return 90;  // Directly upwards
                if (y < 0) return 270; // Directly downwards
                // Undefined behavior when both x and y are zero
                return 0;
            }

            // Get the raw arctan value from the lookup table
            Fd rawValue = ArcTanDRaw(y.abs, x.abs);

            // Determine the quadrant and adjust the angle accordingly
            if (x > 0)
            {
                // Right half (Quadrants I and IV)
                if (y >= 0) return rawValue;  // Quadrant I: angle is as calculated
                else return 360 - rawValue;   // Quadrant IV: angle = 360 - angle
            }
            else
            {
                // Left half (Quadrants II and III)
                if (y >= 0) return 180 - rawValue;  // Quadrant II: angle = 180 - angle
                else return 180 + rawValue;         // Quadrant III: angle = 180 + angle
            }
        }

        private static Fd ArcTanDRaw(Fd y, Fd x)
        {
            int internalVal = (y / x).abs.GetInternalVal();
            if (internalVal > 20000) internalVal = 20000;
            Fd rawValue = LookupTables.main.atan[internalVal];
            return rawValue;
        }
    }
}
