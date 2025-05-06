using static SFML.Window.Keyboard;

namespace Royale2D
{
    public class GlobalInputState : InputState
    {
        // Keyboard only, to be used for internal debug code only. Any official binding should get a control string added
        protected Dictionary<Key, bool> keysHeld = new Dictionary<Key, bool>();
        protected Dictionary<Key, bool> keysPressed = new Dictionary<Key, bool>();

        public GlobalInputState()
        {
        }

        // Called before every frame starts
        public override void Update()
        {
            // TODO move this to use events, do this once controller axes is being implemented
            foreach (Key key in KeyMaps.Keys)
            {
                if (SFML.Window.Keyboard.IsKeyPressed(key) && Game.HasFocus())
                {
                    keysPressed[key] = !keysHeld.GetValueOrDefault(key);
                    keysHeld[key] = true;
                }
                else
                {
                    keysPressed[key] = false;
                    keysHeld[key] = false;
                }
            }

            base.Update();
        }

        public bool IsKeyHeld(Key key)
        {
            return keysHeld.ContainsKey(key) && keysHeld[key];
        }

        public bool IsKeyPressed(Key key)
        {
            return keysPressed.ContainsKey(key) && keysPressed[key];
        }

        public char? GetKeyCharPressed()
        {
            foreach (var kvp in KeyMaps.KeyToChar)
            {
                if (IsKeyPressed(kvp.Key))
                {
#if WINDOWS
                    if (System.Windows.Forms.Control.IsKeyLocked(Keys.CapsLock))
                    {
                        if (KeyMaps.CapsLockMap.ContainsKey(kvp.Key)) return KeyMaps.CapsLockMap[kvp.Key];
                    }
#endif
                    if (IsKeyHeld(Key.LShift))
                    {
                        if (KeyMaps.KeyToCharShift.ContainsKey(kvp.Key)) return KeyMaps.KeyToCharShift[kvp.Key];
                        else return null;
                    }
                    return KeyMaps.KeyToChar[kvp.Key];
                }
            }
            return null;
        }
    }
}
