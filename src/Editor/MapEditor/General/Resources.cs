using System.Windows;
using System.Windows.Input;

namespace MapEditor;

public static class Resources
{
    public static readonly Uri DefaultInstanceImageUri = new("pack://application:,,,/resources/default_instance.png", UriKind.Absolute);
    public static readonly Uri EntranceInstanceImageUri = new("pack://application:,,,/resources/entrance.png", UriKind.Absolute);
    public static Cursor? EraserCursor = new Cursor(Application.GetResourceStream(new Uri("pack://application:,,,/resources/eraser_cursor.cur")).Stream);
    public static Cursor? BucketCursor = new Cursor(Application.GetResourceStream(new Uri("pack://application:,,,/resources/bucket_cursor.cur")).Stream);
}
