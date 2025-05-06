namespace Royale2D
{
    // Fd is short for FastDecimal.
    // This is used instead of floats for netcode safety. Unfortunately float math behaves differently across CPU's leading to nondeterministic netcode and desyncs.
    // We could have used c# decimal but that's orders of magnitude slower than int division and might still not be netcode safe across platforms.
    public struct Fd : IComparable
    {
        public const int IntScale = 100;

        // This represents the internal value with respect to IntScale, "simulating" a fixed float with lower precision
        // We should not need to expose the internal value. All Update() code should use intVal, while all Render() code should use floatVal
        // Really, the only (but important) reason we have the 100's precision is for float-like movement, precision and smoothness
        private int internalVal;

        public int GetInternalVal() => internalVal;
        public int intVal => internalVal / IntScale;
        public int fractalVal => internalVal % IntScale;
        public float floatVal => internalVal / (float)IntScale;
        public decimal decimalVal => internalVal / (decimal)IntScale;
        public Fd wholeValueFd => New(intVal, 0);
        public LongFd longFd => new LongFd(internalVal);

        private Fd(int internalVal)
        {
            this.internalVal = internalVal;
        }

        // For int to Fd conversion especially, ChatGPT suggests using explicit
        //public static implicit operator int(Fd fd) => fd.intVal;      // Fd to int
        public static implicit operator Fd(int value) => New(value);  // int to Fd
        //public static explicit operator int(Fd fd) => fd.intVal;      // Fd to int
        //public static explicit operator Fd(int value) => New(value);  // int to Fd

        public static Fd FromInternalVal(int internalVal)
        {
            return new Fd(internalVal);
        }

        public static Fd New(int intPart, int fractalPart = 0)
        {
            return new Fd((intPart * IntScale) + fractalPart);
        }

        public static Fd New(decimal decVal)
        {
            int sign = Math.Sign(decVal);
            decimal absDecVal = Math.Abs(decVal);
            int intPart = (int)absDecVal;
            int fractalPart = (int)((absDecVal - intPart) * IntScale);
            if (sign < 0) return New(-intPart, -fractalPart);
            return New(intPart, fractalPart);
        }

        public static Fd New(float floatVal)
        {
            float absFloatVal = Math.Abs(floatVal);
            int wholePart = (int)absFloatVal;
            int decimalPart = (int)Math.Round((absFloatVal - wholePart) * IntScale);
            if (floatVal < 0) return New(-wholePart, -decimalPart);
            return New(wholePart, decimalPart);
        }

        public int sign => Math.Sign(internalVal);
        public Fd abs => new Fd(Math.Abs(internalVal));

        public static Fd OnePoint5 => Fd.New(1, 50);
        public static Fd Point5 => Fd.New(0, 50);
        public static Fd Point25 => Fd.New(0, 25);
        public static Fd Point1 => Fd.New(0, 10);
        public static Fd Point01 => Fd.New(0, 1);
        public static Fd MaxValue => new Fd(int.MaxValue);

        public Fd InDir(Fd val)
        {
            if (internalVal < 0) return -val;
            return val;
        }

        public override string ToString()
        {
            return floatVal.ToString();
        }

        public bool IsWhole()
        {
            return internalVal % 100 == 0;
        }

        public void RoundInDir(int dir)
        {
            int remainder = internalVal % 100;
            if (remainder != 0)
            {
                if (dir < 0)
                {
                    internalVal -= remainder;
                }
                else if (dir > 0)
                {
                    internalVal += (100 - remainder);
                }
            }
        }

        public int Rounded()
        {
            if (internalVal >= 0)
            {
                int wholePart = internalVal / 100;
                int remainder = internalVal % 100;
                if (remainder >= 50)
                {
                    return wholePart + 1;
                }
                else
                {
                    return wholePart;
                }
            }
            else
            {
                Fd positiveCopy = -this;
                int positiveRounded = positiveCopy.Rounded();
                return -positiveRounded;
            }
        }

        public static Fd Max(Fd fd1, Fd fd2)
        {
            return fd1.internalVal > fd2.internalVal ? fd1 : fd2;
        }

        public static Fd Min(Fd fd1, Fd fd2)
        {
            return fd1.internalVal < fd2.internalVal ? fd1 : fd2;
        }

        public static Fd operator *(Fd fd1, int val)
        {
            return new Fd(fd1.internalVal * val);
        }

        public static Fd operator *(int val, Fd fd1)
        {
            return new Fd(fd1.internalVal * val);
        }

        public static Fd operator *(Fd fd1, Fd fd2)
        {
            long product = (long)fd1.internalVal * fd2.internalVal;  // Use long to prevent overflow
            return new Fd((int)(product / IntScale));  // Correct the scaling
        }

        public static Fd operator /(Fd fd1, int val)
        {
            return new Fd(fd1.internalVal / val);
        }

        public static Fd operator /(Fd fd1, Fd fd2)
        {
            if (fd2.internalVal == 0)
            {
                throw new DivideByZeroException("Cannot divide by zero.");
            }
            long temp = (long)fd1.internalVal * IntScale;  // Scale the dividend to preserve precision
            return new Fd((int)(temp / fd2.internalVal));  // Perform the division
        }

        public static Fd operator -(Fd fd1)
        {
            return new Fd(-fd1.internalVal);
        }

        public static Fd operator +(Fd fd1, Fd fd2)
        {
            return new Fd(fd1.internalVal + fd2.internalVal);
        }

        public static Fd operator -(Fd fd1, Fd fd2)
        {
            return new Fd(fd1.internalVal - fd2.internalVal);
        }

        public static bool operator ==(Fd fd1, Fd fd2)
        {
            return fd1.internalVal == fd2.internalVal;
        }

        public static bool operator !=(Fd fd1, Fd fd2)
        {
            return fd1.internalVal != fd2.internalVal;
        }

        public static bool operator <(Fd fd1, Fd fd2)
        {
            return fd1.internalVal < fd2.internalVal;
        }

        public static bool operator >(Fd fd1, Fd fd2)
        {
            return fd1.internalVal > fd2.internalVal;
        }

        public static bool operator <=(Fd fd1, Fd fd2)
        {
            return fd1.internalVal <= fd2.internalVal;
        }

        public static bool operator >=(Fd fd1, Fd fd2)
        {
            return fd1.internalVal >= fd2.internalVal;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Fd other = (Fd)obj;
            return internalVal.Equals(other.internalVal);
        }

        public override int GetHashCode()
        {
            // Use a combination of the hash codes of the individual components
            return internalVal.GetHashCode();
        }

        public static Fd Clamp(Fd fd, Fd v1, Fd v2)
        {
            if (fd < v1) return v1;
            if (fd > v2) return v2;
            return fd;
        }

        public static Fd ClampMin0(Fd fd)
        {
            if (fd < 0) return 0;
            return fd;
        }

        public int CompareTo(object? obj)
        {
            if (obj is Fd other)
            {
                return internalVal.CompareTo(other.internalVal);
            }
            return -1;
        }
    }
}
