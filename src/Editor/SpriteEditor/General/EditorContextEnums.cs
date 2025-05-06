namespace Editor;

// A common pattern used with shared projects, followed here, is that the shared project depends on the "sharee" providing definition + implementation of certain types themselves
// In this case, we need to provide types like RedrawData, DirtyFlag and more.
// We declare them under the general "Editor" namespace so the shared project code can elegantly reference them with no extra work involved

// Unlike map editor, sprite editor, being far simpler, does not have any need for a separate redraw flag concept (as of right now)
// so it uses a consolidated, single redraw data/flag type below, this is the equivalent of map editor's "RedrawTarget" enum

// Be careful in changing these enums below. If you change them, you must also change the XAML converters in the shared project to match.
public enum RedrawData
{
    All,
    Sprite,
    Spritesheet
}

public enum DirtyFlag
{
    // For sprite editor, Default dirties the current selected sprite. Again, a common theme is sprite editor being far simpler than map editor and it may not even need a
    // dirty flag concept right now since there's only one type, but because we're sharing a lot of this stuff we have to provide something, and there may be more in future.
    Default,
}

public enum EditorEvent
{
    SESelectedFrameChange,
}

// Unfortunately, XAML's parameter syntax is extremely verbose and clunky, particularly when you get to passing in arrays of stuff.
// Thus we will take string CSV parameters and parse them into the actual enum values here implicitly. Implicit is bad, but XAML is worse
// and if we were to design our own UI framework, we'd make our system not suck, unlike XAML, and not need the implictness here
public class SideEffectConverters
{
    public static RedrawData ParseXamlRedrawParameter(string parameter)
    {
        if (!Enum.TryParse(parameter, out RedrawData result))
        {
            throw new Exception();
        }

        return result;
    }

    public static DirtyFlag ParseXamlDirtyParameter(string parameter)
    {
        if (!Enum.TryParse(parameter, out DirtyFlag dirtyFlag))
        {
            throw new Exception();
        }

        return dirtyFlag;
    }

    public static (DirtyFlag dirtyFlag, RedrawData redrawData) ParseXamlDirtyAndRedrawParameter(string parameter)
    {
        DirtyFlag? dirtyFlag = null;
        RedrawData? redrawData = null;

        string[] pieces = parameter.Split(',');
        if (pieces.Length != 2)
        {
            throw new Exception("Dirty and redraw parameter must be a string of length 3");
        }
        for (int i = 0; i < pieces.Length; i++)
        {
            if (i == 0)
            {
                if (!Enum.TryParse(pieces[i], out DirtyFlag result))
                {
                    throw new Exception();
                }
                dirtyFlag = result;
            }
            else if (i == 1)
            {
                if (!Enum.TryParse(pieces[i], out RedrawData result))
                {
                    throw new Exception();
                }
                redrawData = result;
            }
        }

        return (dirtyFlag!.Value, redrawData!.Value);
    }
}
