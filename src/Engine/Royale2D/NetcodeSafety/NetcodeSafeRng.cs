using System.Diagnostics;

namespace Royale2D
{
    public class NetcodeSafeRng
    {
        private long seed;
        private const long a = 48271;  // Multiplier
        private const long c = 0;      // Increment
        private const long m = 2147483647; // Modulus (2^31 - 1, a prime number)

        public NetcodeSafeRng(int seed)
        {
            if (seed == 0) seed = 1;
            this.seed = seed;
        }

        private int Next()
        {
            seed = (a * seed + c) % m;
            return (int)seed;
        }

        // Inclusive
        private int RandomRangeInternal(int min, int max)
        {
            // Ensure range is positive
            int range = max - min + 1;
            return (int)(Next() % range) + min;
        }

        public static int RandomRange(int start, int end)
        {
            if (Match.current == null)
            {
                Helpers.Assert(false, "Match.current was null in NetcodeSafeRng.RandomRange()");
            }
            if (Match.current is OnlineMatch om)
            {
                return om.netcodeSafeRng.RandomRangeInternal(start, end);
            }
            return Helpers.RandomRange(start, end);
        }
    }
}
