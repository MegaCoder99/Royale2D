using System.Windows.Input;

namespace MapEditor;

public class HotkeyManager
{
    public List<HotkeyConfig> hotkeys = [];

    public void AddHotkeys(List<HotkeyConfig> hotkeys, MapEditorMode? tool)
    {
        foreach (var hotkey in hotkeys)
        {
            hotkey.specificMode = tool;
            this.hotkeys.Add(hotkey);
        }
    }
}

public enum HotkeyModifier
{
    Shift,
    Control,
    Any
}

public class HotkeyConfig
{
    public Key key;
    public HotkeyModifier? modifier;
    public Action action;
    public MapEditorMode? specificMode;

    public HotkeyConfig(Key key, Action action, MapEditorMode? specificMode = null)
    {
        this.key = key;
        this.action = action;
        this.specificMode = specificMode;
    }

    public HotkeyConfig(Key key, HotkeyModifier modifier, Action action, MapEditorMode? specificMode = null)
    {
        this.key = key;
        this.modifier = modifier;
        this.action = action;
        this.specificMode = specificMode;
    }

    public bool IsMatch(Key key, MapEditorMode? specificMode)
    {
        if (this.key == key && (this.specificMode == null || this.specificMode == specificMode))
        {
            if (modifier == null)
            {
                return !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift) && !Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl);
            }
            else if (modifier == HotkeyModifier.Shift)
            {
                return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            }
            else if (modifier == HotkeyModifier.Control)
            {
                return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            }
            else if (modifier == HotkeyModifier.Any)
            {
                return true;
            }

            return true;
        }
        return false;
    }
}