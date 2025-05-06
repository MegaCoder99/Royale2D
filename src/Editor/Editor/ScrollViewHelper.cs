using System.Windows;
using System.Windows.Controls;

namespace Editor;

// Prevent focus on an element in a scroll view from adjusting the scroll view's position. Oftentimes you'll want to resize the window to only take half the screen, especially on a 4K monitor, for multi-tasking.
// In those times, without this helper on scroll views, any focus being obtained anywhere will snap the top-level window scroll position to the focused element, which is annoying and disruptive.
public static class ScrollViewerHelper
{
    public static readonly DependencyProperty PreventFocusScrollProperty =
        DependencyProperty.RegisterAttached(
            "PreventFocusScroll",
            typeof(bool),
            typeof(ScrollViewerHelper),
            new PropertyMetadata(false, OnPreventFocusScrollChanged));

    public static bool GetPreventFocusScroll(DependencyObject obj) =>
        (bool)obj.GetValue(PreventFocusScrollProperty);

    public static void SetPreventFocusScroll(DependencyObject obj, bool value) =>
        obj.SetValue(PreventFocusScrollProperty, value);

    private static void OnPreventFocusScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollViewer scrollViewer && e.NewValue is bool isEnabled)
        {
            if (isEnabled)
            {
                scrollViewer.PreviewGotKeyboardFocus += ScrollViewer_PreviewGotKeyboardFocus;
            }
            else
            {
                scrollViewer.PreviewGotKeyboardFocus -= ScrollViewer_PreviewGotKeyboardFocus;
            }
        }
    }

    private static void ScrollViewer_PreviewGotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
    {
        if (sender is ScrollViewer scrollViewer)
        {
            var verticalOffset = scrollViewer.VerticalOffset;
            var horizontalOffset = scrollViewer.HorizontalOffset;

            // Delay to ensure focus logic finishes before restoring offsets
            scrollViewer.Dispatcher.InvokeAsync(() =>
            {
                scrollViewer.ScrollToVerticalOffset(verticalOffset);
                scrollViewer.ScrollToHorizontalOffset(horizontalOffset);
            });
        }
    }
}