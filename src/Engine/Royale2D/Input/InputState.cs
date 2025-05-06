using static SFML.Window.Keyboard;

namespace Royale2D
{
    public abstract class InputState
    {
        protected Dictionary<string, bool> controlsHeld = new Dictionary<string, bool>();
        protected Dictionary<string, bool> controlsPressed = new Dictionary<string, bool>();

        public bool IsHeld(string inputName)
        {
            return controlsHeld.ContainsKey(inputName) && controlsHeld[inputName];
        }

        public bool IsPressed(string inputName)
        {
            return controlsPressed.ContainsKey(inputName) && controlsPressed[inputName];
        }

        public virtual void Update()
        {
            foreach (var binding in Bindings.main.keyboardBinding)
            {
                string control = binding.Key;
                Key key = binding.Value;

                if (IsKeyPressed(key) && Game.HasFocus())
                {
                    controlsPressed[control] = !controlsHeld.GetValueOrDefault(control);
                    controlsHeld[control] = true;
                }
                else
                {
                    controlsPressed[control] = false;
                    controlsHeld[control] = false;
                }
            }
        }
    }
}