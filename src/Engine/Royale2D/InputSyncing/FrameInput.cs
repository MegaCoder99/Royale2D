using ProtoBuf;

namespace Royale2D
{
    [ProtoContract]
    public class FrameInput
    {
        [ProtoMember(1)] public ushort bits;

        public FrameInput()
        {
        }

        public FrameInput(ushort bits)
        {
            this.bits = bits;
        }

        public override string ToString()
        {
            return bits.ToString();
        }

        public bool EqualTo(FrameInput other)
        {
            return bits == other.bits;
        }
    }
}
