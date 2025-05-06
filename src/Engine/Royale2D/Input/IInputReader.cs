namespace Royale2D
{
    public interface IInputReader
    {
        public bool IsHeld(string inputName);
        public bool IsPressed(string inputName);
    }
}
