using static SFML.Window.Keyboard;

namespace Royale2D
{
    public class Bindings
    {
        public Dictionary<string, Key> keyboardBinding;

        public static Bindings main = new Bindings();

        public Bindings()
        {
            keyboardBinding = defaultKeyboardBinding;
        }

        public Dictionary<string, Key> defaultKeyboardBinding = new Dictionary<string, Key>()
        {
            { Control.Action, Key.X },
            { Control.Attack, Key.C },
            { Control.Item, Key.Z },
            { Control.ItemLeft, Key.A },
            { Control.ItemRight, Key.S},
            { Control.Toss, Key.D },
            { Control.Map, Key.Tab },
            { Control.Menu, Key.Escape },
            { Control.Up, Key.Up },
            { Control.Down, Key.Down },
            { Control.Left, Key.Left },
            { Control.Right, Key.Right },
            { Control.MenuUp, Key.Up },
            { Control.MenuDown, Key.Down },
            { Control.MenuLeft, Key.Left },
            { Control.MenuRight, Key.Right },
            { Control.MenuSelectPrimary, Key.X },
            { Control.MenuSelectSecondary, Key.C },
            { Control.MenuBack, Key.Z },
        };
    }
}
