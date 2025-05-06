## Overview

Royale2D is a WIP 2D sprite-based battle royale engine with a custom-built sprite and map editor.

Everything is written in C#. The engine uses SFML.NET (C# bindings for SFML). The editors use WPF.

As mentioned, this project has two main components: the editors, and the game engine.
- For editor docs and readme, see the readme [here](src/editor/readme.md)
- For engine docs and readme, see the readme [here](src/engine/readme.md)

For general conventions that apply to all projects, see the doc [here](docs/general_conventions.md)

## Repo Structure

The "asset_workspaces" folder contain the map and sprite asset "workspaces" which an artist/designer/developer would work in as their main project files for sprite and map asset creation and modification, opening these workspaces in the relevant editor.

The "assets" folder stores the exported assets from the asset workspaces. "sprites", "spritesheets" and "maps" contents would come from exported workspaces. This folder also stores other raw assets directly that are simple enough to not warrant a separate editor workspace (sounds, music, non-sprite images, etc)

The "src" contains the source code for the various components. Editor folder contains the main sln and csprojs for the WPF sprite and map editors. Engine folder contains the main sln and csproj for the SFML.NET-based game engine. Shared is code used by both.

## Screenshots

### Engine

![Alt Text](docs/screenshots/engine.png "Optional Title")

### Map Editor

![Alt Text](docs/screenshots/map_editor.png "Optional Title")

### Sprite Editor

![Alt Text](docs/screenshots/sprite_editor.png "Optional Title")

## License

MIT. Do what ever you want with it!
Assets used are creative commons.