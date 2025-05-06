namespace Royale2D
{
    public class PlayerInputState : InputState, IInputReader
    {
        // up, down, left, right, attack, action, item, toss, itemLeft, itemRight
        public ushort GetInputBits()
        {
            string bitString = "";

            bitString += IsHeld(Control.Up) ? "1" : "0";
            bitString += IsHeld(Control.Down) ? "1" : "0";
            bitString += IsHeld(Control.Left) ? "1" : "0";
            bitString += IsHeld(Control.Right) ? "1" : "0";
            bitString += IsHeld(Control.Attack) ? "1" : "0";
            bitString += IsHeld(Control.Action) ? "1" : "0";
            bitString += IsHeld(Control.Item) ? "1" : "0";
            bitString += IsHeld(Control.Toss) ? "1" : "0";
            bitString += IsHeld(Control.ItemLeft) ? "1" : "0";
            bitString += IsHeld(Control.ItemRight) ? "1" : "0";

            bitString += "000000";
            // Debug.debugString3 = bitString;
            return Convert.ToUInt16(bitString, 2);
        }

        public FrameInput GetInput()
        {
            return new FrameInput(GetInputBits());
        }
    }
}
