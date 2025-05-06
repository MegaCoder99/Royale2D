# I. Architecture Rationale

## Design Tenants

Managing state and undo in applications, especially complex, rich client tools for game development where performance is much more of a concern, is hard. WPF provides some of this functionality out of the box, such as XAML data binding, but in general it's manual and verbose with no pre-built undo/redo system or change tracking.

Here are some of the design tenants I had in mind:
- **Undo/redo**: these must be supported for a good user experience and productive game creators and artists. This was not a decision take lightly as undo/redo introduces serious complexity in any codebase.
- **Performance**: state changes, undo/redo tracking, and re-renders should be fast and strive for small memory footprint.
- **Conciseness and DRY**: Classical WPF design is well known for having very verbose and redundant property declarations for data bound fields and classes, to the point where you have to scroll thousands of lines to see any actual logic. Having constant copy-pasted, duplicated code is also error prone, so it' be great to adopt principles from more modern UI frameworks like React for reducing boilerplate. This also allows for faster development and iteration.
- **Streamlined re-renders**: WPF is also well known for requiring constant manual "OnPropertyChanged" calls to update the XAML UI from the code due to lack of declarative, unidirectional data flow principles. While this can be added to the setters of the properties, it is verbose boilerplate and doesn't handle complex interactions where a state change in some deeply nested `a.b.c.d` field must change some other deeply nested `a.x.y.z` field in the state hierarchy. Design streamlined systems to handle any field dependencies that need re-rendering.
- **"Natural" feeling state change code**: Code that changes a field in the state (like say an int) should not involve weirdness like drilling down to a nested field, hash set modification, etc. You should just to `foobar.intVar = 3` and be done with it. In general, state change code should be written like there was no undo/redo abstraction happening at all. Note that this applies to places consuming the abstraction, i.e. "consumer code". As this will be like 90% of the editor code and what a dev would spend the most time writing/reading/maintaining. Of course, the abstraction code itself can do whatever it wants/needs.

While it allows for RAD and simple, concise code without massive boilerplate, I am aware of some weakness/cons with the current design:

- **Easier to mess something up**: Removing boilerplate/copy-paste and relying on simplicity and implicitness can remove some guards on doing the right thing. Compile time checks will take a hit and generally undo state issues are found at runtime. Workarounds include debug asserts to verify callers aren't doing the wrong thing.
- **Learning curve**: the abstraction system needs to be learned and understood, although code documentation and existing code can help set examples for reference.
- **Abstraction leaks**: the state management system can be confusing when running into unexpected issues, or if you need a special use case that it might not have been built for, requiring some new pattern. Workarounds include careful documentation of existing patterns and gotchas to improve understandability of the system for when there is a new pattern that needs to be added in future.
- **Internal complexity**: while for consuming code, things are made much simpler and more concise, if there is a bug in the system, the internal "library" code needs to be fixed which is much more complex behind the scenes.

These pros/cons are subjective. Implict is not always bad. There are plenty of examples of highly successful systems in IT that were designed with a leaning towards implicitness. For example, REST (as opposed to SOAP), JSON (as opposed to XML), JSON libraries that allow you to automatically serialize/deserialize to fields defined in your classes,  ReactJS, and Ruby on Rails are all examples of tech with implicit designs with minimal verbosity, duplication and boilerplate. Few complain these are bad, in fact the alternatives listed are often derided for being way too complex/verbose/annoying to work with.

## Why WPF? What Else Was Considered?

Some of my previous projects used JS/TS with React + Electron for tooling. While React is a well-designed modern UI library we could use or take inspiration from, such libraries as React/Redux often rely on immutable state, and creating new objects for changed state fields when they are modified. This works well if your state is small (i.e. web dev, where your entire set of content is not stored locally but in a SQL database in the cloud). But in game development and rich client game editors, oftentimes the state is huge and all local. For example, the Map Editor is designed to support maps with 10-100 million tile instances and 10-100K tile blueprints. Changes to these must support undo and canvas redraw operations. It would not be performant to constantly re-create these large lists and their associated data, including any cached rendered images, etc, and leave many copies of these large memory objects lingering in the undo queue. React virtual DOM diffs would also be slow with such large items in memory.

It is possible to adopt a "clone-what-changed" approach to state changes but have the "large data" fields be an exception to this rule, similarly to React's ref system. And in fact, I tried this approach initially. But constantly having to re-assign references to large database-like lists, bitmap images, etc that are closely tied to the data model itself (like map section layers) and which are often in deeply nested object models, resulted in very cumbersome and verbose, error-prone code that was managed in a completely different code path from the rest of the "small data" fields. In addition, WPF would oftentimes behave in unexpected and unfathomable ways when large portions of the DataContext are suddenly changed, including clearing out selected items in list views, combo boxes, etc. (See "In Data Bindings for ListBox..." gotcha above, and I'm not even sure this is the only case where OnPropertyChanged has an unexpected side-effect.)

Instead, the general architecture relies on mutable fields. Each field that can be undo/redo'd is wrapped with a "Trackable" class or the like, and changes to these are automatically are sent to the undo-redo queue. When an undo needs to happen, the Trackable abstraction will handle changing the value back to the old one.

In addition, there were pain points with Electron regarding awkward interaction with the file system, requiring a bridge and complex async chains for any sort of file manipulation, which a rich client is constantly doing. A JS/TS project is also quite complex to setup, build and maintain, due to a fragmented ecosystem of build tools and libraries, as opposed to a C#/WPF project where you just Ctrl+F5 in VS and everything works.

If perf is such a concern, why not use C++? (i.e. Qt Framework.) C# is a managed language that provides fast iteration and great debuggability along with reasonable performance (a lot better than interpreted/scripting languages at least). I found that for tools, its performance is more than enough, and it chugged through processing large tilesets at an impressive rate, way faster than JS/TS. (There is also "unsafe" keyword in case you need even more perf.) The only issue, then, is that while C# is an excellent language, WPF is an old, archaic framework that isn't always easy to work with. It is however an improvement over WinForms due to its data binding capabilities and IMO the best C# option for complex Win32-based, rich client executables, giving it a weird niche in game development tools. The state management system you'll read further in this doc describes an attempt to create a state management framework around WPF that attempts to patch up some of the shortcomings of WPF.

Because this is a rich-client game development tool and not a typical "business enterprise" application, some of the patterns touted by WPF don't really apply, or apply less, since they were targeted torwards the former. So there may be some deviation on "traditional" WPF patterns due to specific requirements around performance, manual pixel rendering to custom canvases, etc.

# II. Overview - Basic System Components

## State and State Components

In WPF, there is a "DataContext" object which you can bind to your window/control. The editor uses this for storing its centralized root state object, `State.cs`. The root state object contains all fields that the XAML UI binds to, as well as fields that need to be undo'd/redo'd. Basically, most in-memory state in the application will live somewhere in this class hierarchy.

All objects in the state hierarchy that have undo'able fields and/or data-bound fields need to inherit from the `StateComponent` base class, which provides useful boilerplate helper methods and abstractions. The StateComponent class can be thought of as a traditional base "ViewModel" class in WPF MVVM (Model-View-Viewmodel) pattern.

There are two flavors of StateComponent. State components that map directly to models and save/load from them on disk use the class `StateComponentWithModel.cs`. State components that are just containers of StateComponentWithModel objects, i.e. for organization of your UI state hierarchy, which can have other XAML UI bindings not directly tied to a model, simply use the `StateComponent.cs` base class.

More on this pattern's implementation can be found in the MVVM.md doc.

## Editor Context

Abstracts away global editor state management into a singleton, though this singleton should rarely be referenced directly; instead, pass down the context variable in existing StateComponents to new ones you create. This "EditorContext" class has several responsibilities. It stores a singular reference to the current editor state (which there can only be one of) which it invokes callbacks on. It also holds a reference to an UndoManager which encapsulates undo/redo logic and internal fields. Most important of all, it has the concept of a single "commit context", more on that next:

## Commit Context

Any time you want to make a discrete set of changes in the state that is undo'able in one undo operation, there must be a commit context created that represents this set of changes. There are two types: Code and UI. Code is done manually, whereas UI is done automatically when the appropriate Side Effect Converter is used on the data binding for a field in the XAML (more on that later). After a commit context is executed, it pushes an undo entry to the undo stack and performs a "side effect". Side effects include setting the current selection to "dirty" or redrawing one or multiple canvases.

Here is an example of setting up a code commit context for making a discrete set of changes in state:

```
// Redraws with default canvas redraw and dirty flags. Each editor will interpret what the default flag does
editorContext.ApplyCodeCommit(RedrawFlag.Default, DirtyFlag.Default, () =>
{
    // Any trackable changes made in a commit context lambda/action will automatically have undo/redo entries generated behind the scenes.
    MapSectionLayer newLayer = selectedMapSection.AddLayer();
    selectedMapSection.selectedLayer = newLayer;
});
```

The selectedLayer property would have to be configured to be a trackable for this to undo properly. More on that later.

After the commit context call, the state will now have an undo/redo entry with your changes to selectedLayer. You could change more properties too, and those would be undo'd in the same undo operation as long as they are in the lambda block. Convention is to name all commit context methods with Commit at the end for clarification, so if you want to think about calling these, you'll know they'll be generating a commit context just by glancing at the method name.

Note that you do NOT have to manually specify undo and redo code with this system, since the Trackable system handles it automatically. This is a great benefit that reduces amount of code by 2x or more and improves readability, maintainability, and reduces bug risk. (However, a few places in the code will still manually be setting undo nodes, i.e. in Tileset, due to needing more fine-grained control and optimizations in such massive datasets.)

## Side Effect Converters / UI Commit Context

It is often useful for customizable side effects to happen after a property is changed, that should ONLY happen when they are changed from the UI/XAML layer. 

WPF does not have a way of passing additional metadata to a data binding from target (XAML) to source (C#). You would have to create a whole new property in your code viewmodel with different getter/setter. We want to avoid that for many reasons discussed and to be discussed around avoiding boilerplate/code duplication. As a workaround, binding converters (I am aware this is a weird unintended use of converters but it does suit the requirements quite well) are leveraged on data bound properties as a mechanism to inject a UI commit context into the editor context with customizable side effects. Side effect converters include UndoConverter (adds a UI commit context for the property change only, not performing any other side effect), RedrawConverter (same as previous but also redraws) and DirtyRedrawConverter (same as previous but also dirties state). The UI commit context would end once the trackable property it is bound to is changed.

*I am throwing out the terms Redraw and Rerender a lot, and they are actually different concepts. **Redraw** refers to redrawing the canvas UIs, i.e. with SkiaSharp graphics library, while **Rerender** (or sometimes referred to as "refresh" in the codebase) refers to re-rendering/refreshing the XAML UI with OnPropertyChanged calls.*

Here is an example of a checkbox that, when checked from the UI, will trigger a redraw and undo queue entry for changing the `showTileHitboxes` bool property:

```
<CheckBox Content="Tile Hitboxes" IsChecked="{Binding showTileHitboxes, Converter={StaticResource RedrawConverter}, Mode=TwoWay}" />
```

The code behind for this property would need to be configured to use a trackable for these side effects to work (more on that next).

Why not just have `showTileHitboxes` property always redraw/add to undo stack when it's changed, i.e. in a simple set property, and avoid the need for the converters? In general, StateComponent trackable properties should be generic and re-usable. We could want to change showTileHitboxes in a code commit context, but not want it to do any redraw or cutting an undo commit because we are making many other property changes in one bulk method. Also, different editors/consumers may have different requirements when using a generic StateComponent data model like TrPoint, like firing different redraw or dirty flags, and specifying this logic in the StateComponent model would result in inelegant copy-pasting of multiple unrelated scenarios in one big file when that file should remain generic. The XAML should be the place deciding these sorts of side effects since it's the ultimate consumer of this abstraction system.

*Side note*: One thing I considered for UI commit context was OnSourceUpdated which runs after the property changed:
```
<TextBox Text="{Binding SomeProperty, NotifyOnSourceUpdated=True}" SourceUpdated="OnSourceUpdated" />
// XAML Code-behind
private void OnSourceUpdated(object sender, DataTransferEventArgs e)
{
    Console.WriteLine("Source updated!");
}
```
But several things are undesirable here: one, WPF doesn't support parameters here, forcing a ton of combinations of function signatures for each parameter combination for the side effect parameters. Second, it's work each editor has to do in its code-behind to add these functions instead of being automatic and while it can be abstracted, it adds code/complexity/error potential. Also, there is benefit on setting the UI commit context BEFORE the property changes in terms of debug assert validation and making sure side effect converters or property setters aren't doing the wrong thing.

## Trackables

Fields that need to be undo/redo'able in the state are internally wrapped in a `Trackable` class (or `TrackableList` for lists). These classes are not directly referenced from the state hierarchy classes because it's abstracted by concise helper methods in StateComponent like TrGet and TrSet. Here's the property syntax pattern for a trackable field you'll see a lot in the codebase:

```
public InstanceType selectedInstanceType { get => TrGet<InstanceType>(); set => TrSet(value); }
```

Note that you MUST use these get/set helpers for anything you want trackable. TrackableLists and TrackableDictionaries have similar, but distinct helpers. Use those instead, see below for TrackableList example.

✅ Correct - uses specialized TrListGet and TrListSet methods built for the special needs of trackable list data structures
```
public TrackableList<MapSection> mapSections { get => TrListGet<MapSection>(); init => TrListSet(value); }
```

❌ Incorrect - DO NOT USE, WILL NOT WORK
```
public TrackableList<MapSection> mapSections { get => TrGet<TrackableList<MapSection>>(); set => TrSet(value); }
```

Any change to this property, when done in a commit context, will handle undo logic and state changes appropriately. This concise syntax avoids having to generate 3+ properties and dozens of repeated lines for every single property in your state. It does a lot of work for you behind the scenes, not just for undo/redo but also calling OnPropertyChanged on this property name whenever a change happens, if the new value is different from old, and even adding said OnPropertyChanged call to the undo system so it invokes it on any undo/redo to properly refresh the XAML UI automatically, without any manual effort on your end.

Not every field needs to be a trackable. You can do a plain { get; set; } or even a flat field if you have say a container class or some other reference that will not change. Note that WPF requires a property for data bound fields. You can still use data bound fields in the "classical" WPF way without using the trackable system/syntax, if you don't need the field to be undo'able. And in fact a lot of the "dialog" prompt controls do just this because undo/redo isn't generally implemented in modals. Just keep in mind you'll need to manually call OnPropertyChanged or include it in your setter since the trackable won't exist to handle it for you.

## Dirty State

Some state objects in the hierarchy can have a concept of being in a "dirty" state. This is represented by a normal trackable bool `isDirty`, and if true signifies that the state was changed and is savable to disk, and the Save button will be enabled. When Save is pressed and the contents saved to disk, the state is undirtied.

The dirty system is just abstracting some already existing system (trackable bool flag) that we can set and manage manually, but it does provide value in those XAML side effect converters because we can pass the flag from a XAML side variable there and have it sent to C# code-behind as a specific dirty action. (Though honestly this necessity seems like it originates from a limitation of XAML/WPF more than anything else...)

DirtyFlag has limitations: for example it can't target a specific map section, it always assumes selectedMapSection. This is a problem for "script-like" operations that affect all map sections. As such, DirtyFlag is an "optional" abstraction and if your script needs to set isDirty manually instead of the flag, that should TOTALLY be fine and recommended in cases where the abstraction "leaks".

# III. Basic Flow

When an undo'able state change needs to happen in the editor, here is the typical flow of events:

- Commit context is created. (Without a commit context, undo nodes will not be pushed.)
    - UI context originates from a data binding in XAML with a side effect converter when data is changed from UI.
    - Code context is manual, originates from anywhere in code that calls ApplyCodeCommit()
- C# code run during duration of commit context will change trackable fields.
    - Each trackable changed adds undo nodes to pending undo list.
    - Re-renders (OnPropertyChanged's, etc) will also happen on each trackable change.
    - As re-renders happen, these OnPropertyChanged calls and whatnot are also included in undo nodes (so re-renders happen on undo automatically)
- Finish the commit context. 
    - Dirtying state is done, if specified as a side effect/dirty flag. (Done before cutting undo branch below so it can be undo'd if necessary)
    - An undo group is created from the pending undo node list which is then cleared. This undo group is added to the undo stack.
    - Canvas redraw is done, if specified as a side effect/redraw flag.

When an undo/redo is invoked:
- Pop the last undo/redo node in the stack.
- Take its lambda actions that were queued up, and run each of them in reverse order of addition.
    - This would include the trackable changes, OnPropertyChanged calls, and any other custom undo action enqueued.
- If applicable, apply canvas redraw side effect if set in the undo/redo node. (Dirty side effect is implicitly represented by isDirty trackable changes)

# IV. Common Patterns

## Dependent UI Refreshes

It is very common for one property B to be dependent on another A, and when A changes and is re-rendered, B must do so as well. For example, the name could change and then the display name has to change as well.

TrSet accepts a list of property names to refresh (in addition to itself) when it changes. As with all Trackable goodness, it handles undo OnPropertyChanged re-renders automatically so you don't have to think about a thing.

```
public string name { get => TrGet<string>(); set => TrSet(value, [nameof(displayName)]); }
public string displayName => name + (isDirty ? "*" : "");
```

Do note, this only works if the property is in the same class. If a dependent property somewhere else in the state hierarchy needs to change, you'll need to use editor events or property change callbacks (covered next).

## Editor Events

Oftentimes when a certain important state field changes, it needs to cause re-renders in unrelated parts of the state hierarchy. You can fire an **EditorEvent**, an event representing a change in an editor property, with TrSetE (or TrSetAll):

```
public MapSectionLayer selectedLayer { get => TrGet<MapSectionLayer>(); set => TrSetE(value, EditorEvent.MELayerChange); }
```

Then the top level state object can respond to these event enums via the EditorEventHandler callback of IEditorState.

```
public void EditorEventHandler(EditorEvent editorEvent)
{
    if (editorEvent == EditorEvent.MELayerChange)
    {
        QueueOnPropertyChanged(nameof(topLevelSelectedLayerText));
    }
    ...
}
```

Some notes on editor events:
- The handler code is called instantly after firing the event.
- Try not to fire these manually unless absolutely necessary due to error/perf issue potential. Use TrSetE or TrSetAll to have trackable system automatically fire these for you.
- If firing manually, you need to fire in a commit context, or else trackable changes/undo will not work.
- If firing manually, try to fire sparingly since there can be perf issues. OnPropertyChanged calls can be slow if fired 100's or 1000's of times in a tight loop for the same property. For example, be sure to not to fire these events in a tight loop and only call it once after the loop completes.
- QueueOnPropertyChanged is needed in the example because as mentioned above, these will be in a commit context and you need to call these helpers to make these OnPropertyChanged calls invoked as part of undo.

If you want an inner nested field in your state hierarchy to also handle an editor event, you'll have to drill down to it manually from the top-level EditorEventHandler. Note that at one point I made a "ComputedProperty" class/abstraction. This would have parameters to allow listening and responding to EditorEvent enums fired from any StateComponent. Thus preventing the need for this top-level handler and allowing the actual dependent properties to listen themselves for the event. It ended up being complex to automatically bind and hard to read/understand and a potential perf issue in that it relied on event handlers on each StateComponents' ComputedProperty that could linger due to old StateComponent entries in the undo stack lambda closures. Also, it ended up not being too common where one property change needed to re-render some completely different location in the state hierarchy; the places were at the top of the hierarchy anyway, or close to it, as with SelectedCellViewModel.

## Property Change Callbacks

Another very frequent use case when changing a state field A is to change other state fields B, C, etc. whenever A changes. An example is when the "Show Instances/Zones" checkbox is unchecked, it should also unselect any selected instances or zones. Here is how it should be done in the trackable:

✅ Correct
```
public bool showInstancesAndZones { get => TrGet<bool>(); set => TrSetC(value, OnShowInstancesAndZonesChange); }
public void OnShowInstancesAndZonesChange(bool oldValue, bool newValue)
{
    // By using a callback passed to TrSet system, it ensures the following null sets below are undo'able no matter what context it is invoked from.
    // Another benefit is this callback will automatically NOT be run if the value didn't change, without having to do a manual check again if the setter value changed.
    if (!newValue)
    {
        mapCanvas.selectedMapSection.selectedInstance = null;
        mapCanvas.selectedMapSection.selectedZone = null;
    }
}
```

TrSetC is used which accepts a change method callback with old and new values as parameters.
It is important to use this and NOT to add more lines to the setter:

❌ Incorrect
```
public bool showInstancesAndZones 
{
    get => TrGet<bool>();
    set
    {
        TrSet(value);
        // This will not make the following null sets undo'able if invoked from a UI context, i.e. from XAML UI target to source
        if (!value)
        {
            mapCanvas.selectedMapSection.selectedInstance = null;
            mapCanvas.selectedMapSection.selectedZone = null;
        }
    }
}
```

Remember that the callback is NOT an undo block. It will not be run on undo. Only trackable changes made in it will be reverted on undo. If you need stuff in the callback to run also on undo, that isn't handled by Trackable field changes, you need to manually queue undo entries. You can use the helpers below in StateComponent for queueing undo entries for property changed, editor event firing, or just any generic action in general:

```
public void QueueOnPropertyChanged(string propertyName)
{
    editorContext?.AddUndoNode(null, null, () => OnPropertyChanged(propertyName));
}

public void QueueGenericAction(Action action)
{
    editorContext?.AddUndoNode(null, null, action);
}
```

Basically, everything about EditorEventHandler code also applies to TrSetC callbacks, as they are super related and similar. Both will be within commit contexts, both allow you to change trackables and queue undos, etc.

## Merging last N undo commits

Sometimes this is useful, if you have N last undo commits that you want to merge. Some examples include: merging paint tile commits when holding mouse down to paint so that they all get undo'd at once, when you have two separate commit functions that would be annoying/complex to abstract to avoid the commit part, etc.

Use `UndoManager.MergeLastNUndoGroups()` method for this.

# V. Best Practices / Gotchas

The following best practices/gotchas are meant to stop devs from doing the wrong thing. Violating them is not the end of the world, it generally shouldn't cause a crash or data corruption due to failsafe guards in the abstraction classes, but if not followed, bugs and missing undo functionality can happen.

## QueueGenericAction should not perform trackable changes or EditorContext mutations

QueueGenericAction was meant for a custom undo'able "side-effect" unrelated to state management and trackable changes. For example, updating some UI widget that isn't tracked like the current visual cursor, updating some cached data structures that also aren't tracked for efficient retrieval of something that is tracked, etc. In general, if you are changing any trackable or referencing "context" within QueueGenericAction action, you are doing something wrong.

## Firing editor events manually should be done sparingly

If you must fire manually, don't do it in tight loops since perf can suffer. Remember, each editor event fired happens immediately, can trigger lots of expensive OnPropertyChanged calls, and does not do any de-duping magic for you, so the same event fired 100 times will actually invoke the handler 100 times!

## Be careful with infinite recursion in callbacks/editor event handlers

Don't change the same property as the one that triggered it, or you could end up with infinite recursion.

## Methods that invoke commit contexts should end in "Commit"

Already mentioned, mentioning here since it's the practices section.

## Do not change trackables in non-commit contexts

This is usually a mistake since the change would not be undo'able.

There are some exceptions to this rule, like the code that sets isDirty=false after saving, or maybe a quick and dirty Script you write where you don't care about it being undo'able, but exceptions should be minimal.

Note that anything in a TrSetC callback will be in a commit context if the base trackable set call was in a commit context (which it should almost always be, otherwise this or another pattern was already being violated.)

## Do not run commit contexts in existing commit contexts

Nesting code commit contexts is a mistake and will not work properly. Do not do this.

## Code commit contexts should only be triggered from input events

Do not invoke code commit contexts from trackable property setters or anything of the like. These should only come from events fired by user input (i.e. button press, canvas mouse/keyboard input, etc) that could not be triggered by some other property change.

NOTE: It is ok to add code commit contexts in a setter property IF that setter property is NOT a trackable. Sometimes you'll want getter/setter properties that do not actually have an underlying Trackable field, but are more of a "logical" set/set abstraction for another existing field. Here's an example:

```
// This controls a text input that logically changes multiple fields in bulk in selected tiles. As such there is no underlying trackable or private field in this property.
// Hence, having a code commit context here is fine and recommended. Be sure NOT to use the side effect converters on this property in the XAML. We don't want a UI commit context being generated here.
public string cellOverrideZMask
{
    get => UniqueValHelper(selectedTileCoords.Select(c => selectedLayer.GetCellOverride(c)?.zIndexMaskColor));
    set
    {
        editorContext.ApplyCodeCommit(RedrawFlag.Default, DirtyFlag.Default, () =>
        {
            foreach (GridCoords coords in selectedTileCoords)
            {
                CellOverride cellOverride = selectedLayer.GetOrCreateCellOverride(coords);
                cellOverride.zIndexMaskColor = value;
                selectedLayer.SetCellOverride(coords, cellOverride);
            }
            // Since this isn't a trackable, there is manual effort required in re-rendering the field on change. We don't get that for free here. Not only that, we need to handle re-render on undo/redo too.
            // Hence QueueOnPropertyChanged helper is used, not the standard "classical" OnPropertyChanged
            QueueOnPropertyChanged(nameof(cellOverrideZMask));
            return true;
        });
    }
}
```

## Do not reassign trackable lists / trackable dicts

TrackableList's object reference should not change. To clear it out, call Clear() on it. This is because TrackableList's change events only happen if its contents change, not if the reference itself change. The same applies to TrackableDictionary and any other trackable collection type built in the future. It's also unnecessary performance impact and complexity to support `= [];` or something; one way to do things is better.

The easiest way to enforce this is to use the new `init` C# syntax:

```
public TrackableList<MapSection> mapSections { get => TrListGet<MapSection>(); init => TrListSet(value); }
```

Or better yet, if you never need to assign the trackable list (i.e. it's not saved/loaded from disk and always has initial value of empty list) just have a getter:

```
// TrListGet handles initialization behind the scenes in an implicit hash set structure
public TrackableList<GridCoords> selectedTileCoords => TrListGet<GridCoords>(OnSelectedTileCoordsChange);
```

## In Data Bindings for ListBox, ComboBox, etc., when removing the selected item, change it before modifying the bound collection

You must do this because OnPropertyChanged() calls in WPF can modify other data-bound properties when called in certain cases, and we need to prevent this behavior. This prevents cases where the selected item is suddenly and unexpectedly set to null.

Let's say you have a list view (or really, any list control with selectable element(s)) of type T bound to an ObservableCollection<T> field (or in the case of this repo, a TrackableList which merely extends ObservableCollection) and its selected item bound to an object/field of type T.

You want to remove the selected item from the ObservableCollection, and change the selected item to another item in the list that still exists.

If you were to first remove the selected item from the collection, then change the selected item binding, and assuming OnPropertyChanged is called on each of these mutations, (which it should be in both the editor abstraction system and most WPF codebases) you called OnPropertyChanged on the ObservableCollection BEFORE you called it on the selected item. WPF would clear the selected item to null! This is because WPF doesn't think the selected item exists anymore on the collection, hence there is some weird automatic behavior where it clears it out to null. This will mess with the trackable system, which doesn't expect OnPropertyChanged to actually change any properties, improperly adding an undo entry for clearing it to null which can cause problems like null reference exceptions (an example is if you have a change callback/editor event for that property and a listener doesn't expect it to ever be null).

As such, when dealing with list views and bound items: if you want to bind the selected item to an object field directly, you'll have to change that first and OnPropertyChanged that first if removing it from list.

You might be wondering if the same issue can happen if you clear the whole list. In a sense, the answer is yes, but you'd be setting the selected item to null anyway, which implies your selected item variable is nullable, so you would not run into null reference exceptions. There are cases where you don't want your selected item variable to be nullable, and this gotcha addresses those cases.

## In Data Bindings for ListBox, ComboBox, etc., when adding a new item and changing selected item to it, change selected item after modifying bound collection

This is the reverse/inverse of the above. The reason this matters is undo: when you undo adding a new item and changing selected item to it, you'd run into the same issue above if you had changed the selected item, then added the selected item to the bound collection. This is because undo will apply your previous action sequence in reverse order.