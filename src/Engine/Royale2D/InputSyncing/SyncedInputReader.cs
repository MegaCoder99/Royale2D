namespace Royale2D
{
    public class SyncedInputReader : IInputReader
    {
        FrameInput current;
        FrameInput? previous;
        public SyncedInputReader(FrameInput current, FrameInput? previous)
        {
            this.current = current;
            this.previous = previous;
        }

        public bool IsHeld(string inputName)
        {
            return IsPressedInternal(current, inputName);
        }

        public bool IsPressed(string inputName)
        {
            return IsPressedInternal(current, inputName) && (previous == null || !IsPressedInternal(previous, inputName));
        }

        bool IsPressedInternal(FrameInput frameInput, string inputName)
        {
            string pressedInputBitString = Convert.ToString(frameInput.bits, 2).PadLeft(16, '0');

            bool heldUp = pressedInputBitString[0] == '1' ? true : false;
            bool heldDown = pressedInputBitString[1] == '1' ? true : false;
            bool heldLeft = pressedInputBitString[2] == '1' ? true : false;
            bool heldRight = pressedInputBitString[3] == '1' ? true : false;
            bool heldAttack = pressedInputBitString[4] == '1' ? true : false;
            bool heldAction = pressedInputBitString[5] == '1' ? true : false;
            bool heldItem = pressedInputBitString[6] == '1' ? true : false;
            bool heldToss = pressedInputBitString[7] == '1' ? true : false;
            bool heldItemLeft = pressedInputBitString[8] == '1' ? true : false;
            bool heldItemRight = pressedInputBitString[9] == '1' ? true : false;

            if (inputName == Control.Up && heldUp) return true;
            if (inputName == Control.Down && heldDown) return true;
            if (inputName == Control.Left && heldLeft) return true;
            if (inputName == Control.Right && heldRight) return true;
            if (inputName == Control.Attack && heldAttack) return true;
            if (inputName == Control.Action && heldAction) return true;
            if (inputName == Control.Item && heldItem) return true;
            if (inputName == Control.Toss && heldToss) return true;
            if (inputName == Control.ItemLeft && heldItemLeft) return true;
            if (inputName == Control.ItemRight && heldItemRight) return true;

            return false;
        }
    }
}
