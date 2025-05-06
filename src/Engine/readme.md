## Overview

To run the engine code locally, open "Royale2D.sln", set Royale2D as startup if not already, and F5 or Ctrl+F5.

This is a WIP and many custom assets (not provided) are required to enable a lot of the features under development. Otherwise, you'll load with a very basic, barebones player experience that tests the basics of exported sprites and maps. (i.e. a character with exported walk sprites that can walk around in an exported map file/tileset).

## Patterns

Some patterns used in this project:
- **Entity-Component** (similar to Unity). This makes things quite flexible with generic re-usable components attachable to base "Actor" (GameObject-equivalent) objects.
- **Game loop** and with **separate render method**/call stack. Helps with perf (can move this to separate thread eventually and vary the refresh rate independently of game logic) and netcode (rendering logic doesn't need to be deterministic or run when advancing replays or inputs)
- **Deterministic lockstep netcode** is used. The concept of deterministic code, data types, etc. is referred to internally as **Netcode Safety**. We avoid non-determinism like float types, trig math, time-based RNG seeds, etc in game logic. For example, instead of floats we create an "FD" (Fast Decimal) struct that uses deterministic int math under the hood.

## Folder Structure

The folder structure is pretty simple and pragmatic:
- This top-level folder has things like icon resources, build scripts, etc.
- In Royale2D folder: all C# classes/code files except top-level "entry points" should be put in a folder describing a coherent feature set, module or category that code belongs to
- If there isn't a folder where a code file makes sense to live (or if it'd only have a few classes in it tops), put it in the "General" folder until clear complex categories emerge.
- Namespacing to be avoided. At least while the project is under heavy development, I want classes to be fluid and easily moved to different folders as needs arise to adapt to changing dev designs and requirements.

## Build

Run build.ps1 to build a release build to your desktop. Right now only Windows supported, but will eventually try to support Linux and Mac too if I get to it.

## Tests

There's a Tests folder/project with unit tests for the Royale2D engine. Not very big right now and more could be added if there's time and value.

I understand unit testing is a debated subject in game dev (as opposed to "regular" software). It's a creative industry very demanding in workload, results and deadlines, and games are generally highly visual/stateful and difficult to test (especially rendering logic). So while idealistic, it may not always be worth the effort, especially near-100% code coverage. However, personally I think there is some value for highly "algorithmic", "input-output" type code, like math libraries, custom markup parsing, etc. Those tend to be easy to test (require minimal mocking) and have hairy subtle corner cases QA might have trouble repro'ing, so are easier wins. Some effort was made to write very basic tests to aid in development. There's always more that can be added here as time permits.