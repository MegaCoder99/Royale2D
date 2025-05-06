# Tips and Gotchas

The purpose of this doc is to explain subtle things that may not be obvious and can cause subtle issues if you don't know them off the top of your head. Obscure WPF-related bugs or oddities that took significant investigation to uncover will be documented here.

## In Map and Sprite Editor, use Style="{x:Null}" for icon buttons

There is global button styling in the XAML for these editors for padding, etc. But if you have a small button that just has an icon, it would mess these up, so you need to do Style="{x:Null}" on the button in the XAML to ignore the global styling.

## ScrollViewers can steal focus

ScrollViewer can steal focus! this caused a subtle bug where the canvas keydown inputs were not working, because the top level ScrollViewer (that has horizontal/vertical scrolling for the entire page if it's too big) was eating the inputs.

The fix is to set Focusable=False on all ScrollViewers. See `<Style TargetType="ScrollViewer">` for other common properties that all ScrollViewers should have to make focus/scroll work properly and not have any nasty surprises.

In addition:
  - Consider other possible things that can steal focus, use debugger to see what has focus
  - If events not firing in a canvas, focus "stealing" is the first thing you should check.

## In Data Bindings for ListBox, ComboBox, etc., when removing the selected item, change it before modifying the bound collection

Mentioned in the State Management doc so refer to that doc for details, but mentioning again here because it can cause a subtle bug where your selected item is suddenly and unexpectedly set to null after an OnPropertyChanged on the collection. Change the selected item first, before removing it from the collection.

## In Data Bindings for ListBox, ComboBox, etc., when adding a new item and changing selected item to it, change selected item after modifying bound collection

Mentioned in the State Management doc so refer to that doc for details, but mentioning again here due to how subtle it is. Add the new item first, then change the selected item to it.

## Cleaning up BitmapDrawer references manually (via Dispose) is not necessary in the class that references it

`BitmapDrawer` is smart enough to clean itself up on destructor if no references exist to it. It may be tempting to manually clean it up with `Dispose()` in a class that holds a reference to it when it seems like it's no longer being used. But this can result in bugs and even crashes because it's hard to know who still references it. For example, you could have a copy of the class in an undo lambda somewhere that still holds it, and upon undo'ing, it now tries to use an improperly disposed `BitmapDrawer`.

This is not to say never clean it up manually, do it where it's safe and easy (like if it's being held only in a local variable), since it's more performant to clean it up manually, but you have to heed caution and not be overzealous in trying to clean up every possible reference to it in a containing class. Err on the side of letting `BitmapDrawer` clean itself up, and see if `BitmapDrawer` itself is smart enough to do so in your particular case. There are other examples, like on resize it will dispose its old size instance automatically, and callers/containing classes should NOT be doing this.

## WPF requires an initial window to be shown or else app shutdown fails to work properly

For some reason, in WPF you need to actually call Show() on a Window class (showing a MessageBox is not enough) or else your app process can get stuck being alive and not being terminated, even if you call `Application.Current.Shutdown()`.

This applies if you are not automatically loading a Window XAML on start in App.xaml. This can happen in the editors in first time setup wizards which just show a sequence of modals before showing the main editor window. For this reason, the startup factory code ensures you show a Window (and not just a generic MessageBox prompt) on start.

Exceptions being thrown will bypass this and shut the app down as expected even if a window was not shown. You could also call `Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;` and shutdown might work, but it seemed a bit laggy/slow to shutdown when I tried, potentially not reliable, and it's complicated since you need to set it back to the default after showing your window.

## A "heisenbug" problem in C#: get property can be invoked in debugger, but not in actual real code path.

If get { ... } is needed for something to happen, this will cause issues. Caused a trackable bug once where OnAlignmentChange oldAlignment parameter was null, and trying to debug invoked the get property only in the debug flow, leading to massive confusion!

## Weird WPF "fake" compile issues?

Sometimes this happens, not sure if a VS bug or something, but `git clean -fdx` in the editor project folder can help fix those things. Just make sure you backed up any unversioned/unstaged additions first or you will lose them forever!