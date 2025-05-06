using System.Runtime.Serialization;

namespace Royale2D
{
    public struct LongFd
    {
        public const long IntScale = 100;
        public long internalVal;

        public LongFd(long internalVal)
        {
            this.internalVal = internalVal;
        }

        public LongFd abs => new LongFd(Math.Abs(internalVal));
        public Fd fd => Fd.FromInternalVal((int)internalVal);

        public static LongFd operator *(LongFd fd1, int val)
        {
            return new LongFd(fd1.internalVal * val);
        }

        public static LongFd operator *(int val, LongFd fd1)
        {
            return new LongFd(fd1.internalVal * val);
        }

        public static LongFd operator *(LongFd fd1, LongFd fd2)
        {
            long product = (long)fd1.internalVal * fd2.internalVal;  // Use long to prevent overflow
            return new LongFd((long)(product / IntScale));  // Correct the scaling
        }

        public static LongFd operator /(LongFd fd1, long val)
        {
            return new LongFd(fd1.internalVal / val);
        }

        public static LongFd operator /(LongFd fd1, LongFd fd2)
        {
            if (fd2.internalVal == 0)
            {
                throw new DivideByZeroException("Cannot divide by zero.");
            }
            long temp = (long)fd1.internalVal * IntScale;  // Scale the dividend to preserve precision
            return new LongFd((long)(temp / fd2.internalVal));  // Perform the division
        }

        public static LongFd operator +(LongFd fd1, LongFd fd2)
        {
            return new LongFd(fd1.internalVal + fd2.internalVal);
        }

        public static LongFd operator -(LongFd fd1, LongFd fd2)
        {
            return new LongFd(fd1.internalVal - fd2.internalVal);
        }

        public static bool operator >(LongFd fd1, LongFd fd2)
        {
            return fd1.internalVal > fd2.internalVal;
        }

        public static bool operator <(LongFd fd1, LongFd fd2)
        {
            return fd1.internalVal < fd2.internalVal;
        }

        /*
        public static bool operator >=(LongFd fd1, LongFd fd2)
        {
            return fd1.internalVal >= fd2.internalVal;
        }

        public static bool operator <=(LongFd fd1, LongFd fd2)
        {
            return fd1.internalVal <= fd2.internalVal;
        }

        public static bool operator ==(LongFd fd1, LongFd fd2)
        {
            return fd1.internalVal == fd2.internalVal;
        }

        public static bool operator !=(LongFd fd1, LongFd fd2)
        {
            return fd1.internalVal != fd2.internalVal;
        }
        */
    }
}
