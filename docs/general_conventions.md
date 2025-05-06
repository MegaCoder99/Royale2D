# Code/Naming Conventions

This doc only goes over general conventions and practices to this codebase. For more specific conventions and patterns in a certain topic, see the other md files on that topic.
These patterns are not set in stone. They are general guidelines only and can be broken when it makes sense (but should be the exception to the rule rather than the norm.)

## Methods PascalCase()

This is standard C# and not much reason not to follow this pattern from my experience.

## Local variables and parameters camelCase

Same as above, standard C# and no objections here.

## Field and Property Names camelCase, no matter the visibility

- For private fields with a public property, use `_field` for the private variable and `field` for the public property
- For class constants and enums, still use PascalCase for these.
- Use PascalCase for custom control properties in XAML (not data binding fields but actual custom properties like Text, etc), for consistency with other pre-defined XAML properties since you'll often be looking at them side-by-side with your custom ones

I understand this is a bit different from standard C# conventions which wants (public) fields to be capitalized. I like Java's conventions better for several reasons below.

One reason for camelCase fields is consistency with C++, another widely used game dev language, since often time code needs to be ported between the two languages and C++ generally doesn't use PascalCase for fields. In general, most "C-style" languages use camelCase fields/class variables, and C# is kind of going against the mold here. Other languages like JS/TS also use camelCase and so say if your game or tool is ported to be run in web browser it'd make the transition easier.

Another reason is that in JSON, camelCase is the convention for fields, and this makes C# model naming conventions consistent with JSON contents.

Finally, having capital field names can cause them to sometimes conflict with class names (which should always be PascalCase), if they are the same. Even if not, it does look a bit strange to have a `MyClass MyClass;` declaration.

## Use File-Scoped Namespaces

These reduce nesting in the file.

```
namespace Editor;

public class StateComponent
{
  ...
}
```

NOTE: as of right now, most engine code needs to migrate to this new convention. (Editor code is already there.)

## Use Nullable Reference Types

These are enabled in the project and help reduce null reference exceptions. Warnings will be shown for potential null reference cases.

Try to keep warning count minimal, as many of its warnings are valuable. However, oftentimes there are warnings that are simpler to just ignore. One big example is JSON deserialization which annoyingly requires an empty constructor for deserialized classes (or a constructor with ONLY parameters that match your class fields). This generates a lot of warnings in many cases and creating object instances of everything in that constructor (or inline in fields) is not worth it due to expense of instance creation. (i.e. SKBitmaps which involve managed memory.) Or perhaps you want it to be null by default to make sure your code flow is doing the right thing and catch issues via null reference exceptions instead of hiding them.

However, this should be a last resort. strings are easier to initialize and you can just set them to empty string. Even lists can be set to `[]`, and dictionaries to `new()` in constructor/field definition to avoid warnings. You can also lazy load expensive resources in some cases. In some cases the ! character can be used to avoid issues, i.e. `myFieldThatShouldNotBeNull!.foobar`. But this should also be a last resort, as ! everywhere could be covering up actual avoidable null refs (and IMO is less readable to the eyes). If you can design your code to avoid this, i.e. initialize everything in constructor or "inject" dependencies in constructors, even better.

## Use FilePath and FolderPath in Shared.csproj for file system manipulation

C#'s default file system libraries are heavily procedural and clunky. Unless you really need performance in a tight loop or something, these OOP classes were designed to be more streamlined.

## Shared generic C# code in Shared project

This makes it easier to share helpers with a C# game engine, which is what the Shared project was designed for.

If the code is specific to WPF, or Windows Forms, or SkiaSharp, or anything that is not very basic C#/.NET standard library, it should not go in Shared. Shared should not have any external nuget dependencies. Assumptions should not be made about the game engine referencing the editor code.

If your code is something unlikely to be shared with your game engine, you can keep it local to the project. But you should only put something in the editor Helpers.cs if it will truely be reused by many different classes/files. Otherwise just have the helper in the specific class/file that needs it.

## Minimize namespace usage

High level namespaces are currently used, usually one per project: Editor, MapEditor, SpriteEditor, Shared, Royale2D, etc.

Usually these four are enough to resolve naming ambiguities. Generally avoid further nested namespaces and do not make subfolders a separate namespace by default unless there is a strong case for it. Highly nested namespaces can get complex fast. Also, I find that the way I organize and group my code files and folders in my project for the best understandability is *constantly* changing based on needs, discoveries, changing requirements, etc. Game dev is an unpredictable field. Having to constantly change namespaces to keep it in sync would be annoying and error prone.

## Use Actions/Func, and lambdas; avoid manually typing out delegate/event keywords

`Action` and `Func` are great concise ways to represent functions and closures as first class citizens, getting that functional goodness that is often useful in UI development, especially when dealing with things like undo/redo.

Prefer calling `Invoke()` for clarity that it's an action invocation and not a "real" method call.

## File System Naming

Use PascalCase for any file related to C# code, csproj, or sln, or any folder that contains at least one of these
  - This matches C#'s expected conventions
  - It also lets you distinguish and identify whats a code file/folder quickly

For all other files (docs, build scripts, etc) and folders, use snake_case (always lowercase)
  - Reduces chance of git casing issues due to Windows filesystem's case insensitivity

Do NOT worry about adhering to these at all costs. It's a best effort and sometimes you'll have to violate a pattern here or there. If you do accidentally violate a pattern, don't bother fixing it: Git is known to have major issues on Windows when changing casing of files but keeping the filename otherwise the same.

## Use str.IsSet() and str.Unset() instead of String.IsNullOrEmpty(str) for conciseness

These are custom extensions that will check for null automatically and are a lot shorter and easier to type, given how often this check needs to be done. On a related note, use this instead of just doing str == "", it's safer in cases where str is somehow null even if it's not a nullable string (which is still possible at runtime).

## Prefer setting of default field/property values in definition instead of constructor

What it says on the tin.
