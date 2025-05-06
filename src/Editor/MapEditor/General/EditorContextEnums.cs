namespace Editor;

// A common pattern used with shared projects, followed here, is that the shared project depends on the "sharee" providing definition + implementation of certain types themselves
// In this case, we need to provide types like RedrawData, DirtyFlag and more.
// We declare them under the general "Editor" namespace so the shared project code can elegantly reference them with no extra work involved

// Why are there so many map editor redraw flags? In one word: optimization. Drawing large maps and zooming/panning them is
// EXTREMELY slow without clever caching and diff patterns, a problem sprite editor doesn't have because sprites are small

// Be careful in changing these enums below. If you change them, you must also change the XAML converters in the shared project to match.
public enum RedrawFlag
{
    Tooling,        // Redraws the tooling layer only (not the map section layer contents).
    Diffs,          // Redraw only the layers that have diffs. If any do, the containing bitmap will also be redrawn
    Container,      // Redraw only the containing bitmap.
    TileHighlight,  // Redraw highlighted tiles
    All,            // Redraw all layers and the containing bitmap. This is extremely slow (in the order of seconds for large maps) and should be done rarely
}

public enum RedrawTarget
{
    All,
    Map,
    Scratch
}

// We could have merged the above flags into one consolidated one for simplicity, but combinatorial explosion says hi. (Especially if a third or fourth enum is ever added.)
public struct RedrawData(RedrawFlag redrawFlag, RedrawTarget redrawTarget)
{
    public RedrawFlag redrawFlag = redrawFlag;
    public RedrawTarget redrawTarget = redrawTarget;

    public static RedrawData ToolingAll = new(RedrawFlag.Tooling, RedrawTarget.All);
}

public enum DirtyFlag
{
    Map,
    Tile,
    Scratch,
    TileAnimation,
    TileClump,
}

public enum EditorEvent
{
    SelectedTileChange,
    SectionChange,
    LayerChange,
    LayerTileGridChange,
    TileDataChange,
    TileAnimationChange,
    SelectedTileAnimationChange,
    TileClumpChange,
    SelectedTileClumpChange,
}

// Unfortunately, XAML's parameter syntax is verbose and clunky, particularly when you get to passing in arrays of stuff.
// Thus we will take string CSV parameters and parse them into the actual enum values here implicitly.
// Not ideal but better than XAML parameters...
public class SideEffectConverters
{
    public static RedrawData ParseXamlRedrawParameter(string parameter)
    {
        RedrawFlag? redrawFlag = null;
        RedrawTarget? redrawTarget = null;

        string[] pieces = parameter.Split(',');
        if (pieces.Length != 2)
        {
            throw new Exception("Redraw parameter must be a string of length 2");
        }
        for (int i = 0; i < pieces.Length; i++)
        {
            if (i == 0)
            {
                if (!Enum.TryParse(pieces[i], out RedrawFlag result))
                {
                    throw new Exception();
                }
                redrawFlag = result;
            }
            else if (i == 1)
            {
                if (!Enum.TryParse(pieces[i], out RedrawTarget result))
                {
                    throw new Exception();
                }
                redrawTarget = result;
            }
        }

        return new(redrawFlag!.Value, redrawTarget!.Value);
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
        RedrawFlag? redrawFlag = null;
        RedrawTarget? redrawTarget = null;

        string[] pieces = parameter.Split(',');
        if (pieces.Length != 3)
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
                if (!Enum.TryParse(pieces[i], out RedrawFlag result))
                {
                    throw new Exception();
                }
                redrawFlag = result;
            }
            else if (i == 2)
            {
                if (!Enum.TryParse(pieces[i], out RedrawTarget result))
                {
                    throw new Exception();
                }
                redrawTarget = result;
            }
        }

        return (dirtyFlag!.Value, new(redrawFlag!.Value, redrawTarget!.Value));
    }
}