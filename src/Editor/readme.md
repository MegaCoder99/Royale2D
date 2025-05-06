## Overview

The editors for Royale2D, a 2D WIP battle royale game engine, are C# WPF game development tools with two components:
* **Map Editor**: For building 2D, top-down tile-based maps
* **Sprite Editor**: For building 2D sprite animations

This doc goes over the basics of both editors. For editor specific help, see the relevant readme files:
- [Map editor readme](MapEditor/readme.md)
- [Sprite editor readme](SpriteEditor/readme.md)

## Local Dev - Getting Started

For initial setup and build, there should be no special, non-standard dependencies/steps required. Download the latest Visual Studio and the latest C# desktop components (basically, whatever you'd do to run any WPF app locally).

Currently the project uses .NET 8, but I will try to keep this project up-to-date with the newest .NET versions as they release.

After cloning locally, open Editor.sln to get started. This has 4 projects:
- **SpriteEditor**: Sprite editor WPF project.
- **MapEditor**: Map editor WPF project.
- **Editor**: Shared project with code that both editors use. Has code specific to WPF, SkiaSharp, etc.
- **Shared**: Another shared project used by both editor projects. The difference is that this was really meant for sharing common C# code with a C# game engine using this editor. As such it only has dependencies on very generic C#/.NET standard libraries. The idea is that any helper methods or classes that are generic and do not have dependencies on WPF, Winforms, etc should ideally go in Shared code. The Shared project is designed to avoid any references to external dependencies to make it as generic and reusable as possible by a variety of C# game engines. The "model" classes and "workspace" containers are also stored here so the C# engine can reuse them to load the assets easily without any duplicate code.

Set the editor you want to run as startup. Then simply F5 or Ctrl+F5 to run it locally. The Output type of the WPF editor projects (right click Editor project > properties) when running locally is set to Console Application. This gives a console window along with the main UI window, useful for local debug logging with Console.WriteLine().

When running for the first time, you'll be prompted for First Time Setup to load a workspace. You can choose the `<repo_root>\asset_workspaces\map_field_workspace` for map editor to load the sample "field" map. For sprite editor you can choose the `<repo_root>\asset_workspaces\sprite_workspace` folder to load the sample sprites.

External nuget dependencies are minimal. SkiaSharp is the main dependency, which is a rendering library for quick and efficient drawing of large tilesets and images.

## Basic Folder/Code Structure

Some common folder names in the projects include:

* **Controls**: Contains WPF/XAML components, such as "dialogs" (modal window popup prompts shown to the user) and other UI components like a number input ("NumericInputControl") with increment/decrement arrows.
* **Models**: Classes that are deserialized to JSON workspace asset files go here. This folder makes it easy for the game engine to see what contracts are exposed in the data model saved to disk and what it would care about parsing.
* **State**: Contains classes in your State.cs class/object hierarchy, including StateComponents and more.
* **Canvas**: Contains canvas classes. Canvases are those graphical windows in the editors where you do most of your work visually manipulating sprites/map sections, distinct from the general XAML UI.
* **Scripts**: Contains "scripts", which are runnable strongly-typed commands (C# functions) you can run in the script text box UI that let you automate some action. These are usually some scrappy one-off tasks suited to a very specific purpose. Script could end up being "promoted" to "first-class" editor functionality if they are deemed useful enough in general, in which case ideally the script code would be moved out of this folder.
* **General**: General classes that don't fit in one of the categories above.
* **resources**: Contains internal images, etc that the editor uses in its UI.

## Creating Release Builds

This was designed to be as simple as possible. Simply run the `Build.ps1` script found in Editor folder and pass it a flag of -EditorType "me" to build the map editor or "se" for sprite editor. (Or you can just run either BuildMapEditor.ps1 or BuildSpriteEditor.ps1.) By default this will export a x64 release build in a folder called "MapEditor" or "SpriteEditor" to your desktop path, but you can change the parameters like so:

`.\build.ps1 -EditorType "me" -OutputPath "C:\path\to\desired\output" -Platform "x86"`

This will build the x86 binary files to the folder C:\path\to\desired\output\Editor

Only Windows is currently supported and only recent versions (10 and above) that support .NET 8 and have it installed are supported. (Recent Windows versions should have it installed by default.)

## Further Reading

More detailed documentation targeted to specific topics can be found in separate readme files:

- [Workspaces and Exporting](docs/workspaces_and_export.md): Goes into the key tenets of the Royale2D asset pipeline: workspaces and exporting.
- [State Management](docs/state_management.md): Goes into the abstraction system I designed and built for supporting changes in WPF application state, such as data binding/re-rendering abstractions and undo/redo.
- [MVVM](docs/mvvm.md): Goes into this codebase's implementation/take on WPF's MVVM (Model-View-ViewModel) pattern
- [Naming Conventions And Patterns](docs/conventions_and_patterns.md)
- [Tips And Gotchas](docs/conventions_and_patterns.md)