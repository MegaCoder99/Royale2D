using System.Globalization;
using System.Windows.Data;

namespace Editor;

public class UndoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        EditorContext.GetContext().SetCommitContext(CommitContextType.UI, null, []);
        return value;
    }
}

// Note: this implicitly also applies undo (as dirty always should). Could be named UndoDirtyConverter but that's verbose and XAML is already verbose.
public class DirtyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        DirtyFlag dirtyFlag = default;
        if (parameter is string parameterStr)
        {
            dirtyFlag = SideEffectConverters.ParseXamlDirtyParameter(parameterStr);
        }

        EditorContext.GetContext().SetCommitContext(CommitContextType.UI, null, [dirtyFlag]);
        return value;
    }
}

// Note: this implicitly also applies undo. Could be named UndoRedrawConverter but that's verbose and XAML is already verbose.
// If you JUST want to redraw and not undo, don't use this converter or any side effect converter. Bind to a non-trackable getter/setter that does the redraw there.
public class RedrawConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        RedrawData redrawData = new();

        if (parameter is string parameterStr)
        {
            redrawData = SideEffectConverters.ParseXamlRedrawParameter(parameterStr);
        }

        EditorContext.GetContext().SetCommitContext(CommitContextType.UI, redrawData, []);
        return value;
    }
}

// Note: this implicitly also applies undo. Could be named UndoDirtyRedrawConverter but that's VERY verbose and XAML is already verbose.
public class DirtyRedrawConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        DirtyFlag dirtyFlag = default;
        RedrawData redrawData = default;

        if (parameter is string parameterStr)
        {
            (dirtyFlag, redrawData) = SideEffectConverters.ParseXamlDirtyAndRedrawParameter(parameterStr);
        }

        EditorContext.GetContext().SetCommitContext(CommitContextType.UI, redrawData, [dirtyFlag]);
        return value;
    }
}
